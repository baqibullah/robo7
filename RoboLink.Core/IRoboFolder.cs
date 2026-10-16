namespace RoboLink.Core;

public interface IRoboFolder
{
    Task WriteAsync(string name, byte[] content, CancellationToken ct = default);
    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default);
    Task DeleteAsync(string name, CancellationToken ct = default);
}
