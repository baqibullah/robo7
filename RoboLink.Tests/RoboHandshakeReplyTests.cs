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

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        // As per CA1816 IDisposable release mechanism.
        GC.SuppressFinalize(this);
    }
}