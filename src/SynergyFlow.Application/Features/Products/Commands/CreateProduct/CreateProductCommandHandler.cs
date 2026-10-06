using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Common.Events;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Application.Features.Products.Mappers;
using SynergyFlow.Domain.Common.Results;
using SynergyFlow.Domain.Entities.Products;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace SynergyFlow.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IAppDbContext dbContext,
    IDomainEventTracker eventTracker,
    ICacheInvalidator cacheInvalidator)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var skuExists = await dbContext.Products
            .AnyAsync(p => p.Sku == normalizedSku, ct);

        if (skuExists)
        {
            return ProductErrors.SkuAlreadyExists(request.Sku);
        }

        var createResult = Product.Create(
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity,
            request.Description);

        if (createResult.IsFailure)
        {
            return createResult.Errors;
        }

        var product = createResult.Value;

        eventTracker.TrackEntity(product);
        await dbContext.Products.AddAsync(product, ct);
        await dbContext.SaveChangesAsync(ct);

        await cacheInvalidator.InvalidateAsync([CacheTags.Products], ct);

        return product.ToDto();
    }
}
