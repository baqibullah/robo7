using RoboLink.Core;
using RoboLink.Infrastructure;

namespace RoboLink.Tests;

public class RoboHandshakeTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Fact]
    public async Task SendAsync_InvalidTray_ReturnsErrorsAndWritesNothing()
    {
        var handshake = new RoboHandshake(new LocalRoboFolder(_folder), new HandshakeOptions());
        var errors = await handshake.SendAsync([]);
        Assert.NotEmpty(errors);
        Assert.Empty(Directory.GetFiles(_folder));
    }

    [Fact]
    public async Task SendAsync_ValidTray_WritesOneDatPerTube()
    {
        var handshake = new RoboHandshake(new LocalRoboFolder(_folder), new HandshakeOptions());
        var tray = new List<Tube> { TestData.MakeTube(1, false), TestData.MakeTube(2) };

        var errors = await handshake.SendAsync(tray);
        Assert.Empty(errors);

        // Cast<string> as per: CS8620
        var written = Directory.GetFiles(_folder, "*.dat").Select(Path.GetFileName).Order().Cast<string>().ToList();
        var expected = tray.Select(t => DatFileName.Build(t.TubeOrder)).Order().ToList();

        Assert.Equal(expected, written);
    }

    [Fact]
    public async Task SendAsync_ValidTray_WritesEmptyDefFile()
    {
        var handshake = new RoboHandshake(new LocalRoboFolder(_folder), new HandshakeOptions());
        var tray = new List<Tube> { TestData.MakeTube() };

        await handshake.SendAsync(tray);

        var defPath = Path.Combine(_folder, ControlFileName.From(tray[0].TubeOrder).BuildDef());
        Assert.True(File.Exists(defPath));
        Assert.Empty(File.ReadAllBytes(defPath));
    }

    [Fact]
    public async Task SendAsync_ValidTray_WritesDefLast()
    {
        var recording = new RecordingFolder();
        var handshake = new RoboHandshake(recording, new HandshakeOptions());
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };

        await handshake.SendAsync(tray);

        Assert.Equal(3, recording.Writes.Count);
        Assert.Equal(ControlFileName.From(tray[0].TubeOrder).BuildDef(), recording.Writes[^1]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        GC.SuppressFinalize(this);
    }

    /// Hold records of the order of writes.
    private class RecordingFolder : IRoboFolder
    {
        public List<string> Writes { get; } = [];

        public Task WriteAsync(string name, byte[] content, CancellationToken ct = default)
        {
            Writes.Add(name);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string name, CancellationToken ct = default) => Task.FromResult(false);
        public Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<string>>([]);
        public Task DeleteAsync(string name, CancellationToken ct = default) => Task.CompletedTask;
    }
}
