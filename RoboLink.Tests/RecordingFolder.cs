using RoboLink.Core;

namespace RoboLink.Tests;

/// Records the order of writes and deletes. Can fail on chosen names.
internal class RecordingFolder : IRoboFolder
{
    public List<string> Writes { get; } = [];
    public List<string> Deletes { get; } = [];
    public List<string> Existing { get; } = [];
    public string? FailOn { get; set; }
    public string? FailDeleteOn { get; set; }

    public Task WriteAsync(string name, byte[] content, CancellationToken ct = default)
    {
        if (name == FailOn)
            throw new IOException("Simulated write failure.");

        Writes.Add(name);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string name, CancellationToken ct = default)
    {
        if (name == FailDeleteOn)
            throw new IOException("Simulated delete failure.");

        Deletes.Add(name);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string name, CancellationToken ct = default) => Task.FromResult(false);

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>([.. Existing]);
}
