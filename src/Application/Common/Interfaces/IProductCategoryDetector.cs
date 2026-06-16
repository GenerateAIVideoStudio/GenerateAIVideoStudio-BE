namespace Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;

public interface IProductCategoryDetector
{
    Task<string> DetectCategoryAsync(string productName, string? description, CancellationToken ct = default);
    string GetFlowType(string category);
}
