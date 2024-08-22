using Amazon.Runtime;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.AI;
using System.Net.Http;
using Microsoft.Extensions.Options;
using TemplateDocumentGenerator.Models;
using Microsoft.SemanticKernel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.ChatCompletion;
using TemplateDocumentGenerator.Data;
using System.IO;
using Npgsql.Replication.PgOutput.Messages;

namespace TemplateDocumentGenerator.Services
{
    public class AIService
    {
        private readonly HttpClient httpClient;
        private readonly GPT4Tokenizer textTokenizer;
        private readonly AppSettings settings;
        private readonly MemoryServerless kernelMemory;
        public readonly IChatCompletionService ChatCompletionService;
        private readonly ApplicationDbContext dbContext;
        private readonly Kernel kernel;

        //public properties
        public string SystemPrompt { get; set; } = string.Empty;

        public AIService(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            #pragma warning disable SKEXP0010

            dbContext = new ApplicationDbContext(appSettings);
            httpClient = httpClientFactory.CreateClient("retryHttpClient");
            textTokenizer = new GPT4Tokenizer();
            settings = appSettings.Value;
            SystemPrompt = settings.SystemPrompt;

            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(settings.AzureOpenAIChatCompletion.Model, settings.AzureOpenAIChatCompletion.Endpoint, settings.AzureOpenAIChatCompletion.ApiKey, httpClient: httpClient)
                .AddAzureOpenAITextEmbeddingGeneration(settings.AzureOpenAIEmbedding.Model, settings.AzureOpenAIChatCompletion.Endpoint, settings.AzureOpenAIChatCompletion.ApiKey, httpClient: httpClient);

            kernel = builder.Build();

            var kernelMemoryBuilder = new KernelMemoryBuilder()
                 .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
                 {
                     APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                     Endpoint = settings.AzureOpenAIChatCompletion.Endpoint,
                     Deployment = settings.AzureOpenAIEmbedding.Model,
                     Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                     APIKey = settings.AzureOpenAIChatCompletion.ApiKey,
                     MaxTokenTotal = settings.AzureOpenAIEmbedding.MaxInputTokens,
                     MaxRetries = 3
                 },
                     httpClient: httpClient)
                 .WithAzureOpenAITextGeneration(new AzureOpenAIConfig
                 {
                     APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                     Endpoint = settings.AzureOpenAIChatCompletion.Endpoint,
                     Deployment = settings.AzureOpenAIChatCompletion.Model,
                     Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                     APIKey = settings.AzureOpenAIChatCompletion.ApiKey,
                     MaxTokenTotal = settings.AzureOpenAIChatCompletion.MaxInputTokens,
                     MaxRetries = 3
                 }, httpClient: httpClient, textTokenizer: textTokenizer)
                 .WithSimpleVectorDb(new Microsoft.KernelMemory.MemoryStorage.DevTools.SimpleVectorDbConfig { StorageType = Microsoft.KernelMemory.FileSystem.DevTools.FileSystemTypes.Disk, Directory = "KNN" });

            kernelMemory = kernelMemoryBuilder.Build<MemoryServerless>();

            ChatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        }

        public async Task AddDocumentAsync(string filename)
        {
            //Check if the file exists
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The file does not exist", filename);
            }
            else
            {
                //Load file into MemoryStream
                using var fileStream = new FileStream(filename,FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                //Add document to KernelMemory
                await AddDocumentAsync(memoryStream, filename);
            }

        }

        /// <summary>
        /// Add a document to the kernel memory
        /// </summary>
        /// <param name="document">MemoryStream of the document</param>
        /// <param name="filename">Filename of the document</param>
        /// <returns></returns>
        public async Task<bool> AddDocumentAsync(MemoryStream document, string filename)
        {
            //ensure that the source is only the source and not full path
            filename = Path.GetFileName(filename);

            //lets see if the document already exists
            bool docAlreadyExists = dbContext.DocxTemplates.Any(x => x.Name == filename);
            if (docAlreadyExists)
            {
                return false;
            }
            else
            {
                Guid docId = Guid.NewGuid();
                await kernelMemory.ImportDocumentAsync(document, filename, docId.ToString());
                dbContext.Knowledge.Add(new PieceOfKnowledge() { Id=docId, Source=filename });
                await dbContext.SaveChangesAsync();

                while (!await kernelMemory.IsDocumentReadyAsync(docId.ToString()))
                {
                    Thread.Sleep(500);
                }

                return true;
            }
        }

        /// <summary>
        /// Remove a document from Kernel Memory
        /// </summary>
        /// <param name="source">source of the document to remove. This will be a filename or URL</param>
        /// <returns></returns>
        public async Task<bool> RemoveItemAsync(string source)
        {
            //ensure that the source is only the source and not full path
            if (!source.ToLower().StartsWith("http"))
                source = Path.GetFileName(source);

            //lets see if the document already exists
            var doc = dbContext.Knowledge.FirstOrDefault(x => x.Source == source);
            if (doc == null)
            {
                return false;
            }
            else
            {
                await kernelMemory.DeleteDocumentAsync(doc.Id.ToString());
                dbContext.Knowledge.Remove(doc);
                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> AddURLSAsync(List<string> urls)
        {
            foreach (var url in urls)
            {
                //check if the url is already in the session
                var doc = dbContext.Knowledge.Any(d => d.Source == url);
                if (doc==false)
                {
                    var docId = Guid.NewGuid();
                    await kernelMemory.ImportWebPageAsync(url, docId.ToString());
                    while (!await kernelMemory.IsDocumentReadyAsync(docId.ToString()))
                    {
                        Thread.Sleep(500);
                    }
                    dbContext.Knowledge.Add(new PieceOfKnowledge() { Id = docId,Source = url});
                    dbContext.SaveChanges();
                }
            }
            return true;
        }

        public async Task<string> GetAnswerAsync(string question)
        {
            var answer = await kernelMemory.AskAsync(question);
            if (answer.NoResult)
            {
                return string.Empty;
            }
            else
            {
                return answer.Result;
            }
        }

        public async Task<SearchResult> SearchAsync(string question)
        {
            return await kernelMemory.SearchAsync(question);
        }

        public async Task<string> GenerateTextAsync(string prompt, string? systemPrompt=null, string? userPrompt = null)
        {
            string completionText = string.Empty;
            var skChatHistory = new ChatHistory();
            if (!string.IsNullOrEmpty(systemPrompt))
                SystemPrompt += Environment.NewLine + systemPrompt;

            skChatHistory.AddSystemMessage(SystemPrompt);
            skChatHistory.AddUserMessage(prompt);
            if (!string.IsNullOrEmpty(userPrompt))
                skChatHistory.AddUserMessage(userPrompt);
            PromptExecutionSettings settings = new()
            {
                ExtensionData = new Dictionary<string, object>()
                {
                    { "Temperature", 0.8 }
                }
            };

            var result = await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(skChatHistory, settings);
            completionText = result.Items[0].ToString()!;
            return completionText;
        }

        public async Task<String> GenerateTextUsingMemoriesAsync(string prompt, string? systemPrompt=null, string? userPrompt=null)
        {
            //Do an Ask
            string answer = await GetAnswerAsync(prompt);
            if (string.IsNullOrEmpty(answer))
            {
                //Try to get data using search
                var searchResult = await SearchAsync(prompt);
                if (!searchResult.NoResult)
                {
                    string newPrompt = "The following are facts: \n\n";
                    foreach (var c in searchResult.Results)
                    {
                        foreach (var p in c.Partitions)
                        {
                            newPrompt += $"{p.Text} \n";
                        }
                    }

                    prompt = newPrompt + "\n" + prompt;
                }
            }
            else
            {
                prompt = $"The following is a fact: {answer} \n\n {prompt}";
            }

            return await GenerateTextAsync(prompt, systemPrompt, userPrompt);
        }
    }
}
