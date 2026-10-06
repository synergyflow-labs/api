using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Application.Features.Products.Mappers;
using SynergyFlow.Domain.Common.Results;
using SynergyFlow.Domain.Entities.Products;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace SynergyFlow.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null)
        {
            return ProductErrors.NotFound(request.Id);
        }

        return product.ToDto();
    }
}
