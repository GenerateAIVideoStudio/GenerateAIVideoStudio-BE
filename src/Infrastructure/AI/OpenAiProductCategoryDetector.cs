using Application.Common.Interfaces;
using Domain.Constants;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class OpenAiProductCategoryDetector : IProductCategoryDetector
{
    private readonly OpenAIClient _client;

    private static readonly Dictionary<string, string> CategoryToFlow = new()
    {
        ["fashion"] = FlowType.TryOn,
        ["shoes"] = FlowType.TryOn,
        ["bag"] = FlowType.TryOn,
        ["accessories"] = FlowType.TryOn,
        ["beauty"] = FlowType.ImageVideo,
        ["skincare"] = FlowType.ImageVideo,
        ["food"] = FlowType.ImageVideo,
        ["gadget"] = FlowType.ImageVideo,
        ["jewelry"] = FlowType.ImageVideo,
    };

    public OpenAiProductCategoryDetector(IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException("OpenAI:ApiKey configuration is missing");
        _client = new OpenAIClient(apiKey);
    }

    public async Task<string> DetectCategoryAsync(
        string productName, string? description, CancellationToken ct = default)
    {
        var prompt = $"""
            Classify this product into ONE of these categories: fashion, shoes, bag, accessories, beauty, skincare, food, gadget, jewelry, home, general
            
            Product: {productName}
            Description: {description?[..Math.Min(100, description?.Length ?? 0)] ?? ""}
            
            Reply with exactly one word, lowercase, no punctuation, no explanation.
            """;

        var chatClient = _client.GetChatClient("gpt-4o-mini");
        var chat = await chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: ct);

        return chat.Value.Content[0].Text.Trim().ToLower();
    }

    public string GetFlowType(string category) =>
        CategoryToFlow.GetValueOrDefault(category, FlowType.Avatar);
}
