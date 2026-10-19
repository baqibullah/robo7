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
        var written = Directory.GetFiles(_folder).Select(Path.GetFileName).Order().Cast<string>().ToList();
        var expected = tray.Select(t => DatFileName.Build(t.TubeOrder)).Order().ToList();

        Assert.Equal(expected, written);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        GC.SuppressFinalize(this);
    }
}