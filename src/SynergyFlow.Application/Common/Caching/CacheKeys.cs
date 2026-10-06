namespace SynergyFlow.Application.Common.Caching;

public static class CacheKeys
{
    public static string Product(Guid id) => $"product:{id}";
}
