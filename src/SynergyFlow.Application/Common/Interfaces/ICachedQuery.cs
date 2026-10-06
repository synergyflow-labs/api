using MediatR;

namespace SynergyFlow.Application.Common.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }

    string[] Tags { get; }
}

public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;
