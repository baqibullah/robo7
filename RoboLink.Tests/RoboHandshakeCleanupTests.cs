using RoboLink.Core;
using RoboLink.Infrastructure;

namespace RoboLink.Tests;

public class RoboHandshakeCleanupTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Fact]
    public async Task CleanupAsync_AfterReply_RemovesDatsAndReplyFile()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions());
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };

        var errors = await handshake.SendAsync(tray);
        Assert.Empty(errors);

        await folder.DeleteAsync(ControlFileName.From(tray[0].TubeOrder).BuildDef());
        await folder.WriteAsync("0005,0001,1126,0032,OK.txt", []);

        await handshake.CleanupAsync(tray);

        Assert.Empty(await folder.ListAsync());
    }

    [Fact]
    public async Task CleanupAsync_OtherTrayFiles_AreNotDeleted()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions());

        var ours = TestData.MakeTube();
        var other = TestData.MakeTube();
        other = other with { TubeOrder = other.TubeOrder with { TrayNo = 33 } };

        Assert.Empty(await handshake.SendAsync([ours]));
        Assert.Empty(await handshake.SendAsync([other]));

        await folder.WriteAsync("0005,0001,1126,0032,OK.txt", []);
        await folder.WriteAsync("0005,0001,1126,0033,OK.txt", []);

        await handshake.CleanupAsync([ours]);

        // our DAT and OK are gone, our DEF stays (the machine deletes it, not us)
        var expected = new[]{ControlFileName.From(ours.TubeOrder).BuildDef(),
        DatFileName.Build(other.TubeOrder),
        ControlFileName.From(other.TubeOrder).BuildDef(),
        "0005,0001,1126,0033,OK.txt",}.Order().ToList();

        Assert.Equal(expected, [.. (await folder.ListAsync()).Order()]);
    }

    [Fact]
    public async Task CleanupAsync_DeletesDatsInOrderThenReplyFile()
    {
        var recording = new RecordingFolder();
        recording.Existing.Add("0005,0001,1126,0032,OK.txt");
        var handshake = new RoboHandshake(recording, new HandshakeOptions());
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };

        await handshake.CleanupAsync(tray);

        Assert.Equal(
            [
                DatFileName.Build(tray[0].TubeOrder),
            DatFileName.Build(tray[1].TubeOrder),
            "0005,0001,1126,0032,OK.txt",
        ],
            recording.Deletes);
    }

    [Fact]
    public async Task CleanupAsync_DeleteFails_Throws()
    {
        var tray = new List<Tube> { TestData.MakeTube() };
        var recording = new RecordingFolder { FailDeleteOn = DatFileName.Build(tray[0].TubeOrder) };
        var handshake = new RoboHandshake(recording, new HandshakeOptions());

        await Assert.ThrowsAsync<IOException>(() => handshake.CleanupAsync(tray));
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        // As per CA1816 IDisposable release mechanism.
        GC.SuppressFinalize(this);
    }
}
