using SynergyFlow.Application.Common.Models;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(int PageNumber = 1, int PageSize = 10, string? Search = null)
    : IRequest<Result<PaginatedList<ProductDto>>>;
