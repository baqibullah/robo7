using RoboLink.Core;

namespace RoboLink.Tests;

public class TrayValidatorTests
{
    [Fact]
    public void Validate_ValidTray_ReturnsNoErrors()
    {
        var tray = new List<Tube> { TestData.MakeTube() };

        var errors = TrayValidator.Validate(tray);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyTray_ReturnsError()
    {
        var errors = TrayValidator.Validate([]);

        var error = Assert.Single(errors);
        Assert.Equal("Tray", error.Field);
    }

    [Fact]
    public void Validate_BadTube_PrefixesFieldWithTubeNumber()
    {
        var tube = TestData.MakeTube();
        var bad = tube with { TubeOrder = tube.TubeOrder with { PatientName = "" } };

        var errors = TrayValidator.Validate([bad]);

        var error = Assert.Single(errors);
        Assert.Equal("Tube 1: Patient name", error.Field);
    }

    [Fact]
    public void Validate_TwoMatchingTubes_ReturnsNoErrors()
    {
        var tray = new List<Tube> { TestData.MakeTube(1, isLast: false), TestData.MakeTube(2) };
        Assert.Empty(TrayValidator.Validate(tray));
    }

    [Fact]
    public void Validate_DifferentTrayNumber_ReturnsError()
    {
        var second = TestData.MakeTube(2);
        second = second with { TubeOrder = second.TubeOrder with { TrayNo = 99 } };

        var errors = TrayValidator.Validate([TestData.MakeTube(1, isLast: false), second]);
        var error = Assert.Single(errors);
        Assert.Equal("Tray", error.Field);
    }

    [Fact]
    public void Validate_SequenceGap_ReturnsError()
    {
        var errors = TrayValidator.Validate([TestData.MakeTube(1, isLast: false), TestData.MakeTube(3)]);
        Assert.Contains(errors, e => e.Message.Contains("Sequence numbers"));
    }


    [Fact]
    public void Validate_DuplicateSequence_ReturnsError()
    {
        var errors = TrayValidator.Validate([TestData.MakeTube(1, isLast: false), TestData.MakeTube(1)]);

        Assert.Contains(errors, e => e.Message.Contains("Sequence numbers"));
    }

    [Fact]
    public void Validate_NoLastTube_ReturnsError()
    {
        var errors = TrayValidator.Validate([TestData.MakeTube(1, isLast: false), TestData.MakeTube(2, isLast: false)]);

        var error = Assert.Single(errors);
        Assert.Contains("marked last", error.Message);
    }

    [Fact]
    public void Validate_TwoLastTubes_ReturnsError()
    {
        var errors = TrayValidator.Validate([TestData.MakeTube(1), TestData.MakeTube(2)]);

        var error = Assert.Single(errors);
        Assert.Contains("marked last", error.Message);
    }

    [Fact]
    public void Validate_LastTubeNotHighest_ReturnsError()
    {
        var errors = TrayValidator.Validate([TestData.MakeTube(1), TestData.MakeTube(2, isLast: false)]);

        var error = Assert.Single(errors);
        Assert.Contains("marked last", error.Message);
    }

}
