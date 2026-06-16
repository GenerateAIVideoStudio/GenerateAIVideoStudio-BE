using Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Domain.Constants;

namespace Infrastructure.Fakes;

public class FakeProductCategoryDetector : IProductCategoryDetector
{
    public Task<string> DetectCategoryAsync(string productName, string? description, CancellationToken ct = default)
    {
        var combined = $"{productName} {description}".ToLower();
        if (combined.Contains("áo") || combined.Contains("quần") || combined.Contains("váy") || combined.Contains("thời trang") || combined.Contains("t-shirt") || combined.Contains("shirt"))
        {
            return Task.FromResult("Thời trang");
        }
        if (combined.Contains("mỹ phẩm") || combined.Contains("kem") || combined.Contains("son") || combined.Contains("phấn") || combined.Contains("serum"))
        {
            return Task.FromResult("Mỹ phẩm");
        }
        return Task.FromResult("Gia dụng");
    }

    public string GetFlowType(string category)
    {
        if (category == "Thời trang")
        {
            return FlowType.TryOn;
        }
        if (category == "Mỹ phẩm")
        {
            return FlowType.Avatar;
        }
        return FlowType.ImageVideo;
    }
}
