using Microsoft.Extensions.Logging.Abstractions;
using RoboLink.Core;
using RoboLink.Infrastructure;

namespace RoboLink.Tests;

public class RoboHandshakeCancelTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Fact]
    public async Task CancelAsync_DeletesDefFirstThenDatsInOrder()
    {
        var recording = new RecordingFolder();
        var handshake = new RoboHandshake(recording, new HandshakeOptions(), NullLogger<RoboHandshake>.Instance);
        var tray = new List<Tube> { TestData.MakeTube(2), TestData.MakeTube(1, isLast: false) };

        await handshake.CancelAsync(tray);

        Assert.Equal(
            [
            ControlFileName.From(tray[0].TubeOrder).BuildDef(),
            DatFileName.Build(tray[1].TubeOrder),
            DatFileName.Build(tray[0].TubeOrder)
            ],
            recording.Deletes);
    }

    [Fact]
    public async Task CancelAsync_AfterSend_LeavesFolderEmpty()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions(), NullLogger<RoboHandshake>.Instance);
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };

        Assert.Empty(await handshake.SendAsync(tray));
        Assert.NotEmpty(await folder.ListAsync());

        await handshake.CancelAsync(tray);

        Assert.Empty(await folder.ListAsync());
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        GC.SuppressFinalize(this);
    }
}
