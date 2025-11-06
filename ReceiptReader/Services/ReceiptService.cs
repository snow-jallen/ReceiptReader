using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ReceiptReader.Models;

namespace ReceiptReader.Services;

public class ReceiptService(AzureOpenAIClient openAIClient, IConfiguration configuration)
{
    private readonly AzureOpenAIClient _openAIClient = openAIClient;
    private readonly string _deploymentName = configuration["AI:DeploymentName"] ?? "gpt-4o";

    public async Task<Receipt> ProcessReceiptImageAsync(byte[] imageBytes)
    {
        // Get the chat client and convert it to IChatClient
        var chatClient = _openAIClient.GetChatClient(_deploymentName).AsIChatClient();

        // Build the messages with system prompt and image using Microsoft.Extensions.AI types
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                "You are a receipt OCR assistant. Extract all information from the receipt image. " +
                "For items without quantity, use 1. If you cannot find a transaction date, use null. If you cannot find a strong category match, use Unknown."),
            new(ChatRole.User,
            [
                new TextContent("Extract all information from this receipt:"),
                new DataContent(imageBytes, "image/jpeg")
            ])
        };

        // Use GetResponseAsync<T> for structured output
        var response = await chatClient.GetResponseAsync<Receipt>(
            messages,
            new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.Json
            });

        return response.Result ?? throw new Exception("Failed to extract receipt data");
    }
}
