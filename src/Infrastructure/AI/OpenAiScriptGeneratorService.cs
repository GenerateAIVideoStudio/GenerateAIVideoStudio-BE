using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class OpenAiScriptGeneratorService : IScriptGeneratorService
{
    private readonly OpenAIClient _client;

    public OpenAiScriptGeneratorService(IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException("OpenAI:ApiKey configuration is missing");
        _client = new OpenAIClient(apiKey);
    }

    public async Task<GeneratedScript> GenerateAsync(
        ProductInfo product, string videoType,
        string targetAudience, string? hookText = null, CancellationToken ct = default)
    {
        var system = """
            Bạn là chuyên gia viết script UGC ads cho TikTok Shop và Shopee Việt Nam.
            Luôn viết bằng tiếng Việt tự nhiên, như người thật đang review — không phải quảng cáo.
            
            Format output: JSON với keys:
            hook (string), body (string), cta (string), full_text (string),
            estimated_duration_sec (int), suggested_hooks (string[] — 5 hooks thay thế)
            
            Rules:
            - Hook: 3-5 giây, gây tò mò ngay lập tức
            - Body: 15-20 giây, problem → solution → social proof
            - CTA: 3-5 giây, rõ ràng (link bio, comment "MUỐN", bấm link...)
            - Tổng 25-35 giây
            - KHÔNG dùng các từ sáo rỗng hoặc từ cấm quảng cáo: "quảng cáo", "tài trợ", "sponsored", "được tài trợ"
            - Viết như người bình thường đang nói chuyện điện thoại hoặc đang kể với bạn bè
            - Trả về JSON thuần, không markdown code block (không bao gồm ```json ... ```)
            """;

        var user = $"""
            Sản phẩm: {product.Name}
            Giá: {product.Price ?? "xem link"}
            Mô tả: {product.Description?[..Math.Min(300, product.Description?.Length ?? 0)]}
            Reviews nổi bật: {(product.ReviewHighlights != null ? string.Join("; ", product.ReviewHighlights.Take(3)) : "")}
            
            Loại video: {videoType}
            Target audience: {targetAudience}
            {(hookText != null ? $"Hook muốn dùng: {hookText}" : "Gợi ý 5 hooks hay nhất cho ngành này")}
            """;

        var chatClient = _client.GetChatClient("gpt-4o");
        var chat = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(system),
                new UserChatMessage(user)
            ],
            new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            },
            cancellationToken: ct);

        var jsonText = chat.Value.Content[0].Text;
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var result = JsonSerializer.Deserialize<ScriptJson>(jsonText, options)
            ?? throw new InvalidOperationException("Failed to deserialize script JSON from OpenAI");

        return new GeneratedScript(
            result.Hook ?? "",
            result.Body ?? "",
            result.Cta ?? "",
            result.FullText ?? $"{result.Hook} {result.Body} {result.Cta}",
            result.EstimatedDurationSec,
            result.SuggestedHooks ?? new List<string>()
        );
    }
}

internal record ScriptJson(
    string Hook,
    string Body,
    string Cta,
    string FullText,
    int EstimatedDurationSec,
    List<string> SuggestedHooks
);
