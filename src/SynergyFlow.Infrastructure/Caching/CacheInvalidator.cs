using SynergyFlow.Application.Common.Interfaces;

using Microsoft.Extensions.Caching.Hybrid;

namespace SynergyFlow.Infrastructure.Caching;

public class CacheInvalidator(HybridCache hybridCache) : ICacheInvalidator
{
    public async Task InvalidateAsync(string[] tags, CancellationToken ct = default)
    {
        await hybridCache.RemoveByTagAsync(tags, ct);
    }
}
