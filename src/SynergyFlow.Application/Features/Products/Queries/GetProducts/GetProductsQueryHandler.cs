using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Common.Models;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Application.Features.Products.Mappers;
using SynergyFlow.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace SynergyFlow.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.ConvertAll(p => p.ToDto());

        return new PaginatedList<ProductDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
