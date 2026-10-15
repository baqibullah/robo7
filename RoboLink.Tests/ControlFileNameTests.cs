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
}