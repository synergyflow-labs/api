using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Common.Results;

namespace SynergyFlow.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : ICachedQuery<Result<ProductDto>>
{
    public string CacheKey => CacheKeys.Product(Id);

    public string[] Tags => [CacheTags.Products];
}
