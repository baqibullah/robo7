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

    [Fact]
    public async Task SendAsync_DatWriteFails_RemovesWrittenDatsAndRethrows()
    {
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };
        var recording = new RecordingFolder { FailOn = DatFileName.Build(tray[1].TubeOrder) };
        var handshake = new RoboHandshake(recording, new HandshakeOptions());

        await Assert.ThrowsAsync<IOException>(() => handshake.SendAsync(tray));

        Assert.Contains(DatFileName.Build(tray[0].TubeOrder), recording.Deletes);
    }

    [Fact]
    public async Task SendAsync_DefWriteFails_RemovesDefAndDats()
    {
        var tray = new List<Tube> { TestData.MakeTube() };
        var defName = ControlFileName.From(tray[0].TubeOrder).BuildDef();
        var recording = new RecordingFolder { FailOn = defName };
        var handshake = new RoboHandshake(recording, new HandshakeOptions());

        await Assert.ThrowsAsync<IOException>(() => handshake.SendAsync(tray));

        Assert.Equal([defName, DatFileName.Build(tray[0].TubeOrder)], recording.Deletes);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        GC.SuppressFinalize(this);
    }
}
