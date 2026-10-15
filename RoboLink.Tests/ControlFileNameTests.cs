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

    [Theory]
    [InlineData("0005,0001,1126,0032,DEF.TXT")]
    [InlineData("0005,0001,1126,OK.txt")]
    [InlineData("0005,0001,1126,0032,0,OK.txt")]
    [InlineData("5,1,1126,32,OK.txt")]
    [InlineData("0005,0001,1326,0032,OK.txt")]
    [InlineData("0005,0001,11AB,0032,OK.txt")]
    [InlineData("+005,0001,1126,0032,OK.txt")]
    public void Rejects_anything_else(string name)
    {
        Assert.False(ControlFileName.TryParse(name, out _, out _));
    }

    [Fact]
    public void From_order_matches_reply()
    {
        var order = new TubeOrder(5, 1, new DateOnly(2026, 11, 26), 32, false, 1, 1234,
        "0123456789", "Tiger woods", "HbA1c", 1.0m, "Pediatrics", 2, 99);

        Assert.True(ControlFileName.TryParse("0005,0001,1126,0032,OK.txt", out var result, out _));
        Assert.Equal(ControlFileName.From(order), result);
    }
}
