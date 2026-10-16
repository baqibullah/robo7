using RoboLink.Infrastructure;

namespace RoboLink.Tests;


public class LocalRoboFolderTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Fact]
    public async Task WriteAsync_SavesFileWithSameBytes()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        byte[] bytes = [1, 2, 3];

        await localRoboFolder.WriteAsync("a.dat", bytes);

        var savedFile = File.ReadAllBytes(Path.Combine(_folder, "a.dat"));
        Assert.Equal(bytes, savedFile);
    }

    [Fact]
    public async Task WriteAsync_OverwritesExistingFile()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        byte[] bytesExisting = [1, 2, 3];
        await localRoboFolder.WriteAsync("a.dat", bytesExisting);

        byte[] bytesNew = [4, 5, 6];
        await localRoboFolder.WriteAsync("a.dat", bytesNew);
        var savedFile = File.ReadAllBytes(Path.Combine(_folder, "a.dat"));

        Assert.Equal(bytesNew, savedFile);
    }

    [Fact]
    public async Task WriteAsync_LeavesNoTmpFile()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        byte[] bytes = [1, 2, 3];
        await localRoboFolder.WriteAsync("a.dat", bytes);

        IReadOnlyList<string> names = [.. Directory.GetFiles(_folder)
        .Select(Path.GetFileName)
        .Where(n => n is not null && n.EndsWith(".tmp"))
        .Select(n => n!)];

        Assert.Empty(names);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        // As per CA1816 IDisposable release mechanism.
        GC.SuppressFinalize(this);
    }
}
