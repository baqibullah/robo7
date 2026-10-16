using RoboLink.Core;

namespace RoboLink.Infrastructure;


public class LocalRoboFolder : IRoboFolder
{
    private readonly string _root;

    public LocalRoboFolder(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public async Task WriteAsync(string name, byte[] content, CancellationToken ct = default)
    {
        var finalPath = PathFor(name);
        var tmpPath = finalPath + ".tmp";

        await File.WriteAllBytesAsync(tmpPath, content, ct);
        File.Move(tmpPath, finalPath, overwrite: true);
    }

    public Task<bool> ExistsAsync(string name, CancellationToken ct = default) => Task.FromResult(File.Exists(PathFor(name)));

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default)
    {
        IReadOnlyList<string> names = [.. Directory.EnumerateFiles(_root)
        .Select(Path.GetFileName)
        .Where(n => n is not null && !n.EndsWith(".tmp"))
        .Select(n => n!)];

        return Task.FromResult(names);
    }

    public Task DeleteAsync(string name, CancellationToken ct = default)
    {
        File.Delete(PathFor(name));
        return Task.CompletedTask;
    }

    private string PathFor(string name)
    {
        if (Path.GetFileName(name) != name)
            throw new ArgumentException("Name must be a file name only, no folders.", nameof(name));

        return Path.Combine(_root, name);
    }
}
