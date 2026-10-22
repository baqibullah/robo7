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

    [Fact]
    public async Task CancelAsync_OtherTrayFiles_AreNotDeleted()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions(), NullLogger<RoboHandshake>.Instance);

        var ours = TestData.MakeTube();
        var other = TestData.MakeTube();
        other = other with { TubeOrder = other.TubeOrder with { TrayNo = 33 } };

        Assert.Empty(await handshake.SendAsync([ours]));
        Assert.Empty(await handshake.SendAsync([other]));

        await handshake.CancelAsync([ours]);

        var expected = new[]
        {
            DatFileName.Build(other.TubeOrder),
            ControlFileName.From(other.TubeOrder).BuildDef(),
        }.Order().ToList();

        Assert.Equal(expected, [.. (await folder.ListAsync()).Order()]);
    }

    [Fact]
    public async Task CancelAsync_DefDeleteFails_ThrowsAndDeletesNoDats()
    {
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };
        var defName = ControlFileName.From(tray[0].TubeOrder).BuildDef();
        var recording = new RecordingFolder { FailDeleteOn = defName };
        var handshake = new RoboHandshake(recording, new HandshakeOptions(), NullLogger<RoboHandshake>.Instance);

        await Assert.ThrowsAsync<IOException>(() => handshake.CancelAsync(tray));

        Assert.Empty(recording.Deletes);
    }

    [Fact]
    public async Task CancelAsync_CalledTwice_DoesNotThrow()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions(), NullLogger<RoboHandshake>.Instance);
        var tray = new List<Tube> { TestData.MakeTube() };

        Assert.Empty(await handshake.SendAsync(tray));

        await handshake.CancelAsync(tray);
        await handshake.CancelAsync(tray);

        Assert.Empty(await folder.ListAsync());
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        GC.SuppressFinalize(this);
    }
}
