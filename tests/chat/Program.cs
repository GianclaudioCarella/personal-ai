using OpenAI.Chat;
using Azure;
using Azure.AI.OpenAI;
using System.Text;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("prompt.json", optional: false, reloadOnChange: true)
    .Build();

var endpoint = new Uri(config["AzureOpenAI:Endpoint"] ??
    throw new InvalidOperationException("AzureOpenAI:Endpoint is missing in configuration."));
var deploymentName = config["AzureOpenAI:DeploymentName"] ??
    throw new InvalidOperationException("AzureOpenAI:DeploymentName is missing in configuration.");
var apiKey = config["AzureOpenAI:ApiKey"] ??
    throw new InvalidOperationException("AzureOpenAI:ApiKey is missing in configuration.");

AzureOpenAIClient azureClient = new(endpoint, new AzureKeyCredential(apiKey));
ChatClient chatClient = azureClient.GetChatClient(deploymentName);

// Read system message lines from prompts.json and join them
var systemMessageLines = config.GetSection("SystemMessage").Get<string[]>();
var systemMessagePrompt = new StringBuilder();

foreach (var line in systemMessageLines ?? [])
{
    systemMessagePrompt.Append(line);
}

// Initialize chat history
List<ChatMessage> chatHistory =
[
    new SystemChatMessage(systemMessagePrompt.ToString() ?? "You are a personal AI assistant."),
];

Console.WriteLine("Welcome to P.AI. Please go ahead and start chatting!");

while (true)
{
    // Get user prompt and add to chat history
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    string? userPrompt = Console.ReadLine();
    chatHistory.Add(new UserChatMessage(userPrompt));

    // Stream the AI response and add to chat history
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("P.AI: ");
    var response = chatClient.CompleteChatStreaming(chatHistory);

    foreach (StreamingChatCompletionUpdate update in response)
    {
        foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
        {
            System.Console.Write(updatePart.Text);
        }
    }

    System.Console.WriteLine("");
}
