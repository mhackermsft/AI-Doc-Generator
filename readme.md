# Blazor AI Doc Gen Demo
##### AI-Powered Document Generation

## Overview
This application, built on the .NET 8 Blazor Server framework, facilitates the generation of documents from a Word template using uploaded knowledge. Users can define a document template by naming it, uploading a base .docx file, and specifying various placeholders that will be replaced with AI-generated content. During the placeholder definition process, users will provide the AI prompts necessary to generate content based on the uploaded knowledge.

Note: The current version of the applicaiton does not have any authentication or authorization. Users will also share the same uploaded templates and knowledge. 

## Components
This solution leverages several open-source libraries to facilitate the processing and storage of knowledge, as well as to interact with the Azure OpenAI service. Some of these libraries include:
* Semantic Kernel
* Semantic Memory

## Features
- **AI Integration**: Utilizes advanced AI models to provide intelligent document generation.
- **Knowldege Upload**: Users can upload Word, PowerPoint, Excel, Text, and PDF documents, or reference specific URLs, which will be utilized as knowledge for document generation.
- **Retry Handling**: Automatically pauses and retries calls to Azure OpenAI if API rate limits are exceeded.

# Requirements
- **Azure Subscription**: Must include at least one Azure OpenAI chat model and one Azure OpenAI embedding model deployed. 
- **Deployment Options**: 
  - **Local**: Can be run locally.
  - **Azure App Service**: Can be published to an Azure App Service. If deployed on Azure App Service, EasyAuth can be enabled for authentication, however users will still share the same knowledge and document templates.

## Configuration
The appsettings.json file has a few configuration parameters that must be set for the app to properly work:

```
"ConnectionStrings": {
  "LocalDB": "Data Source=docgen.db"
},
"AzureOpenAIChatCompletion": {
  "Endpoint": "",
  "ApiKey": "",
  "Model": "",
  "MaxInputTokens": 128000
},
"AzureOpenAIEmbedding": {
  "Model": "",
  "MaxInputTokens": 8192
},
"SystemPrompt": "You are a helpful AI assistant. Respond in a friendly and professional tone. Answer questions directly using only the information provided. NEVER respond that you cannot access external links."
```

- **AzureOpenAIChatCompletion Configuration**: 
  - Include your Azure OpenAI endpoint URL, API Key, and the name of the deployed chat model you intend to use. Update the MaxInputTokens to match the model selected.

- **AzureOpenAIEmbedding Configuration**: 
  - Specify the deployed embedding model you plan to use. Update the MaxInputTokens to match the model selected.
  - Both the chat and embedding models are assumed to be accessed through the same Azure OpenAI endpoint and API key.

## Deployment

### Manual Deployment

- **Azure OpenAI Service Setup**: Manually create your Azure OpenAI Service and deploy both a chat model and an embedding model.
- **Repository Cloning**: Clone this repository and open it in Visual Studio 2022.
- **Configuration**: Update the `appsettings.json` file with the appropriate values.
- **Running the Application**: You can run the application locally through Visual Studio or publish it to an Azure App Service or another .NET web host.

### Automatic Deployment to Azure

To deploy the application to Azure, you can use the button below. This process will create an Azure App Service Plan, an Azure App Service, and an Azure OpenAI Service with two models deployed. It will also deploy the website to the Azure App Service. 

**Important**: Please read and understand all the information in this section before pressing the "Deploy to Azure" button. NOTE: Once published to Azure your website is available to **all** visitors. You may want to enable authentication on the App Serivce to prevent anonymous access to the site.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmhackermsft%2FAI-Doc-Generator%2Fmaster%2FInfra%2Fazuredeploy.json)

If you encounter an error indicating that the deployment has failed, please verify whether the web application has been created. If it has, access the deployment center logs to determine if the code deployment is still in progress. Due to the time required for code compilation and deployment, the portal may incorrectly report a timeout error while the deployment is ongoing. By continuously monitoring the deployment status, you should see it complete successfully, resulting in the website being deployed.

#### Important Azure Deployment Notes  
The button above will deploy Azure services that will be billed against your Azure subscription. The deployment template allows you to choose region, web app SKU, AI Chat model, AI embed model, along with OpenAI capacity size.  These options will impact the cost of the deployed solutions. Since the automatic deployment uses the Azure App Service build feature, it will use a significant amount of local web storage to complete the build process. After the web site has been deployed you can open the App Service console and run the command `dotnet nuget locals all --clear` to clear the local Nuget package store which will reclaim over 800MB of storage. After running that command you may be able to scale down to the Azure App Service Plan free SKU. The web server's file storage is used for knowledge storage. You may need to scale up your App Service Plan SKU to support the additional amount of storage needed for your knowledge store.

**Warning:** If you do not enable App Service authentication, your website will be available **on the public internet** by default. This means that **other people can connect and use the website**. You will **incur Azure OpenAI charges** for all usage of your website. If you upload documents, that **knowledge will be accessible to all users**. 

All of the settings noted above in the appsettings.json file can be configured in the Azure Portal by going to the Azure App Service environment settings.

### AI Model Capacity
When deploying to Azure, you can set various deployment properties, including the AI model capacity. This capacity represents the quota assigned to the model deployment.

- 1 unit = 1,000 Tokens per Minute (TPM)
- 10 units = 10,000 Tokens per Minute (TPM)

Select a value that meets your token and request requirements while staying within the available capacity for the model.

Read more about Azure OpenAI Service quota here: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/quota?tabs=rest

### Costs
The cost to operate this demo application in your subscription will depend upon a few factors:
- **App Service Plan size** - The deployment script by default uses the B1 tier. You can, however, adjust this to increase performance and features.
- **Azure OpenAI Service** - Two Azure OpenAI Models are required in order for this demo to function properly. The recommended models are `gpt-4o` and `text-embedding-ada-002`. The chat models are priced based on the number of input and output tokens. The embedding model is priced based on the number of tokens.

You can learn more about the cost for Azure App Service and Azure OpenAI models at the links below.

- **Azure App Service** - https://azure.microsoft.com/en-us/pricing/details/app-service/windows/
- **Azure OpenAI** - https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/

## Impact of Azure OpenAI Capacity Settings

This demonstration application is designed for low-volume use and does not include retry logic for Azure OpenAI calls. If a request exceeds the allocated Azure OpenAI quota for the chat or embedding model, a notification will appear at the top of the application. 

To address this issue, please ensure that your Azure OpenAI models are configured with the appropriate quota to accommodate the volume of tokens and requests being submitted.

For more information on managing your Azure OpenAI service quotas, please visit this link: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/quota?tabs=rest

## Known Limitation
Word, Excel or PowerPoint documents that have data classification or DRM enabled cannot be uploaded. You will receive a file corruption error. Only upload documents that are not protected.

## Disclaimer
This code is for demonstration purposes only. It has not been evaluated or reviewed for production purposes. Please utilize caution and do your own due diligence before using this code. I am not responsible for any issues you experience or damages caused by the use or misuse of this code.