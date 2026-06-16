using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeScriptGeneratorService : IScriptGeneratorService
{
    public Task<GeneratedScript> GenerateAsync(
        ProductInfo product,
        string videoType,
        string targetAudience,
        string? hookText = null,
        CancellationToken ct = default)
    {
        var hook = hookText ?? "Bạn đã bao giờ thấy một sản phẩm tuyệt vời như thế này chưa?";
        var body = $"Sản phẩm {product.Name} là giải pháp hoàn hảo cho {targetAudience}. Với mức giá chỉ {product.Price}, đây là lựa chọn không thể bỏ qua.";
        var cta = "Hãy bấm vào link bên dưới để nhận ngay ưu đãi đặc biệt hôm nay!";
        
        var fullText = $"{hook} {body} {cta}";

        var script = new GeneratedScript(
            Hook: hook,
            Body: body,
            Cta: cta,
            FullText: fullText,
            EstimatedDurationSec: 30,
            SuggestedHooks: new List<string>
            {
                hook,
                "Bí quyết sở hữu sản phẩm chất lượng giá cực rẻ!",
                "Đừng mua sản phẩm khác trước khi biết đến điều này!"
            }
        );

        return Task.FromResult(script);
    }
}
