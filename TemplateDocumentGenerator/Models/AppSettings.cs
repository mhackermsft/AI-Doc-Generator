namespace TemplateDocumentGenerator.Models
{
    public class AppSettings
    {
        public AzureOpenAIChatCompletionSettings AzureOpenAIChatCompletion { get; set; } = new AzureOpenAIChatCompletionSettings();
        public AzureOpenAIEmbeddingSettings AzureOpenAIEmbedding { get; set; } = new AzureOpenAIEmbeddingSettings();
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new ConnectionStringsSettings();

        public string SystemPrompt { get; set; } = string.Empty;
    }

    public class AzureOpenAIChatCompletionSettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaxInputTokens { get; set; } = 128000;
        public bool SupportsImages { get; set; } = false;
    }

    public class AzureOpenAIEmbeddingSettings
    {
        public string Model { get; set; } = string.Empty;
        public int MaxInputTokens { get; set; } = 8192;
    }

    public class ConnectionStringsSettings
    {
        public string LocalDB { get; set; } = string.Empty;
    }
}
