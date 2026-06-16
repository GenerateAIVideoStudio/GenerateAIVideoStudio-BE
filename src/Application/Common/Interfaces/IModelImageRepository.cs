namespace Application.Common.Interfaces;

using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IModelImageRepository
{
    Task<List<ModelImage>> GetByGenderAsync(string gender, int count, CancellationToken ct = default);
}
