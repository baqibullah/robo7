using RoboLink.Core;
using RoboLink.Infrastructure;

namespace RoboLink.Tests;

public class RoboHandshakeReplyTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Fact]
    public async Task CheckReplyAsync_OkFileForTray_ReturnsOk()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions());
        var tray = new ControlFileName(5, 1, 11, 26, 32);

        await folder.WriteAsync("0005,0001,1126,0032,OK.txt", []);
        var reply = await handshake.CheckReplyAsync(tray);

        Assert.Equal(RoboReply.Ok, reply);
    }

    [Theory]
    [InlineData("0005,0001,1126,0032,NG.txt", RoboReply.Ng)]
    [InlineData("0005,0001,1126,0032,ng.TXT", RoboReply.Ng)]
    [InlineData("0005,0001,1126,0032,ok.TXT", RoboReply.Ok)]
    public async Task CheckReplyAsync_ReplyFileAnyCase_ReturnsMatchingResult(string name, RoboReply expected)
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions());
        var tray = new ControlFileName(5, 1, 11, 26, 32);

        await folder.WriteAsync(name, []);
        var reply = await handshake.CheckReplyAsync(tray);

        Assert.Equal(expected, reply);
    }

    [Fact]
    public async Task CheckReplyAsync_OnlyOurDefAndDatFiles_ReturnsPending()
    {
        var handshake = new RoboHandshake(new LocalRoboFolder(_folder), new HandshakeOptions());
        var tube = TestData.MakeTube(1);

        var errors = await handshake.SendAsync([tube]);
        Assert.Empty(errors);

        var reply = await handshake.CheckReplyAsync(ControlFileName.From(tube.TubeOrder));

        Assert.Equal(RoboReply.Pending, reply);
    }

    [Fact]
    public async Task CheckReplyAsync_OkAndNgBothPresent_ReturnsNg()
    {
        var folder = new LocalRoboFolder(_folder);
        var handshake = new RoboHandshake(folder, new HandshakeOptions());

        await folder.WriteAsync("0005,0001,1126,0032,OK.txt", []);
        await folder.WriteAsync("0005,0001,1126,0032,NG.txt", []);

        var reply = await handshake.CheckReplyAsync(new ControlFileName(5, 1, 11, 26, 32));

        Assert.Equal(RoboReply.Ng, reply);
    }

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

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        // As per CA1816 IDisposable release mechanism.
        GC.SuppressFinalize(this);
    }
}
