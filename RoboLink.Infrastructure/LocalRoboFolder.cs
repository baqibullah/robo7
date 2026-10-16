using RoboLink.Core;

namespace RoboLink.Infrastructure;


public class LocalRoboFolder : IRoboFolder
{
    public Task DeleteAsync(string name, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string name, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(string name, byte[] content, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
