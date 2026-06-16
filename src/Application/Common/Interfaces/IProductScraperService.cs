namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IProductScraperService
{
    Task<ProductInfo> ScrapeAsync(string url, CancellationToken ct = default);
}
