using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.AI.Models;

internal record ShopeeApiResponse(
    [property: JsonPropertyName("data")] ShopeeData? Data
);

internal record ShopeeData(
    [property: JsonPropertyName("item")] ShopeeItem? Item
);

internal record ShopeeItem(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("price")] long? Price,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("images")] List<string>? Images,
    [property: JsonPropertyName("item_rating")] ShopeeRating? ItemRating,
    [property: JsonPropertyName("categories")] List<ShopeeCategory>? Categories
);

internal record ShopeeRating(
    [property: JsonPropertyName("rating_tags")] List<ShopeeTag>? RatingTags
);

internal record ShopeeTag(
    [property: JsonPropertyName("tag_content")] string? TagContent
);

internal record ShopeeCategory(
    [property: JsonPropertyName("display_name")] string? DisplayName
);
