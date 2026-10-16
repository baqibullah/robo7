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

        Assert.DoesNotContain(Directory.GetFiles(_folder), f => f.EndsWith(".tmp"));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueAfterWrite()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        await localRoboFolder.WriteAsync("a.dat", [1, 2, 3]);

        Assert.True(await localRoboFolder.ExistsAsync("a.dat"));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForMissingFile()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);

        Assert.False(await localRoboFolder.ExistsAsync("a.dat"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        await localRoboFolder.WriteAsync("a.dat", [1, 2, 3]);

        await localRoboFolder.DeleteAsync("a.dat");

        Assert.False(File.Exists(Path.Combine(_folder, "a.dat")));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForMissingFile()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);

        await localRoboFolder.DeleteAsync("missing.dat");
    }

    [Fact]
    public async Task ListAsync_ReturnsWrittenNames()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        await localRoboFolder.WriteAsync("a.dat", [1]);
        await localRoboFolder.WriteAsync("b.dat", [2]);

        var names = await localRoboFolder.ListAsync();

        Assert.Equal(2, names.Count);
        Assert.Contains("a.dat", names);
        Assert.Contains("b.dat", names);
    }

    [Fact]
    public async Task ListAsync_HidesTmpFiles()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);
        File.WriteAllBytes(Path.Combine(_folder, "half.dat.tmp"), [1]);

        var names = await localRoboFolder.ListAsync();

        Assert.Empty(names);
    }

    [Fact]
    public async Task WriteAsync_RejectsNameWithFolder()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);

        await Assert.ThrowsAsync<ArgumentException>(() => localRoboFolder.WriteAsync("../x", [1]));
    }

    [Fact]
    public async Task ExistsAsync_RejectsNameWithFolder()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);

        await Assert.ThrowsAsync<ArgumentException>(() => localRoboFolder.ExistsAsync("../x"));
    }

    [Fact]
    public async Task DeleteAsync_RejectsNameWithFolder()
    {
        var localRoboFolder = new LocalRoboFolder(_folder);

        await Assert.ThrowsAsync<ArgumentException>(() => localRoboFolder.DeleteAsync("../x"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        // As per CA1816 IDisposable release mechanism.
        GC.SuppressFinalize(this);
    }
}
