using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeProductScraperService : IProductScraperService
{
    public Task<ProductInfo> ScrapeAsync(string url, CancellationToken ct = default)
    {
        var mockProduct = new ProductInfo(
            Name: "Mock Premium Cotton T-Shirt",
            Price: "150,000 VND",
            Description: "This is a wonderful mock product description that highlights all the beautiful features and quality materials of this item.",
            ImageUrls: new List<string> { "https://picsum.photos/400/600?shirt1", "https://picsum.photos/400/600?shirt2", "https://picsum.photos/400/600?shirt3" },
            ReviewHighlights: new List<string> { "Sản phẩm rất đẹp và chất lượng!", "Giao hàng nhanh, đóng gói cẩn thận.", "Đáng tiền, sẽ mua lại." },
            Category: "Thời trang"
        );
        return Task.FromResult(mockProduct);
    }
}
