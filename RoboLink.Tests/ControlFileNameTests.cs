using RoboLink.Core;

namespace RoboLink.Tests;


public class ControlFileNameTests
{
    private static readonly ControlFileName Sample = new(5, 1, 11, 26, 32);

    [Fact]
    public static void Build_def_name()
    {
        Assert.Equal("0005,0001,1126,0032,DEF.TXT", Sample.BuildDef());
    }

    [Theory]
    [InlineData("0005,0001,1126,0032,OK.txt", ControlResult.Ok)]
    [InlineData("0005,0001,1126,0032,ok.TXT", ControlResult.Ok)]
    [InlineData("0005,0001,1126,0032,NG.txt", ControlResult.Ng)]
    public void Parses_ok_and_ng(string name, ControlResult expected)
    {
        Assert.True(ControlFileName.TryParse(name, out var result, out var kind));
        Assert.Equal(Sample, result);
        Assert.Equal(expected, kind);
    }
}
