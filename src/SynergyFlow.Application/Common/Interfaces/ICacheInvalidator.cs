namespace SynergyFlow.Application.Common.Interfaces;

public interface ICacheInvalidator
{
    Task InvalidateAsync(string[] tags, CancellationToken ct = default);
}
