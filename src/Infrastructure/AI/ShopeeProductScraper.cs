using Application.Common.DTOs;
using Application.Common.Interfaces;
using Infrastructure.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class ShopeeProductScraper : IProductScraperService
{
    private readonly HttpClient _http;

    public ShopeeProductScraper(HttpClient http)
    {
        _http = http;
    }

    public async Task<ProductInfo> ScrapeAsync(string url, CancellationToken ct = default)
    {
        if (url.Contains("tiktok.com") || url.Contains("tiktokshop"))
        {
            return await ScrapeTikTokShopAsync(url, ct);
        }

        return await ScrapeShopeeAsync(url, ct);
    }

    private async Task<ProductInfo> ScrapeShopeeAsync(string url, CancellationToken ct)
    {
        // Shopee internal API — extract IDs from URL
        // Pattern: /Product-Name-i.{shopId}.{itemId} or similar
        var match = Regex.Match(url, @"i\.(\d+)\.(\d+)");
        if (!match.Success)
        {
            throw new ArgumentException("Cannot extract Shopee product ID from URL. Ensure the URL format is similar to: https://shopee.vn/Product-Name-i.shopId.itemId");
        }

        var shopId = match.Groups[1].Value;
        var itemId = match.Groups[2].Value;
        var apiUrl = $"https://shopee.vn/api/v4/item/get?itemid={itemId}&shopid={shopId}";

        // Configure headers to look like a browser request
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Add("Referer", url);

        var response = await _http.GetFromJsonAsync<ShopeeApiResponse>(apiUrl, ct);
        var item = response?.Data?.Item
            ?? throw new InvalidOperationException("Shopee API returned no product data or failed to bypass bot protection");

        return new ProductInfo(
            Name: item.Name ?? "Unnamed Product",
            Price: FormatPrice(item.Price),
            Description: item.Description?[..Math.Min(400, item.Description?.Length ?? 0)],
            ImageUrls: item.Images?.Select(img => $"https://cf.shopee.vn/file/{img}").ToList() ?? new List<string>(),
            ReviewHighlights: ExtractTopReviews(item),
            Category: item.Categories?.LastOrDefault()?.DisplayName
        );
    }

    private async Task<ProductInfo> ScrapeTikTokShopAsync(string url, CancellationToken ct)
    {
        // Fallback or stub for TikTok Shop since API details are not fully specified
        return await Task.FromResult(new ProductInfo(
            Name: "TikTok Shop Product",
            Price: "Call for price",
            Description: "TikTok Shop scraping is not fully implemented yet, please enter product details manually if needed.",
            ImageUrls: new List<string>(),
            ReviewHighlights: new List<string>(),
            Category: "general"
        ));
    }

    private string FormatPrice(long? price)
    {
        // Shopee price is returned multiplied by 100,000 (e.g. 100,000,000 is 1,000k VND)
        return price.HasValue ? $"{price.Value / 100_000:N0}k VND" : "Xem link";
    }

    private List<string> ExtractTopReviews(ShopeeItem item)
    {
        return item.ItemRating?.RatingTags
            ?.Take(3)
            .Select(t => t.TagContent ?? "")
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList() ?? new List<string>();
    }
}
