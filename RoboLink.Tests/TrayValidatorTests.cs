using RoboLink.Core;

namespace RoboLink.Tests;

public class TrayValidatorTests
{
    [Fact]
    public void Validate_ValidTray_ReturnsNoErrors()
    {
        var tray = new List<Tube> { MakeTube() };

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
        var tube = MakeTube();
        var bad = tube with { TubeOrder = tube.TubeOrder with { PatientName = "" } };

        var errors = TrayValidator.Validate([bad]);

        var error = Assert.Single(errors);
        Assert.Equal("Tube 1: Patient name", error.Field);
    }

    [Fact]
    public void Validate_TwoMatchingTubes_ReturnsNoErrors()
    {
        var tray = new List<Tube> { MakeTube(1, isLast: false), MakeTube(2) };
        Assert.Empty(TrayValidator.Validate(tray));
    }

    [Fact]
    public void Validate_DifferentTrayNumber_ReturnsError()
    {
        var second = MakeTube(2);
        second = second with { TubeOrder = second.TubeOrder with { TrayNo = 99 } };

        var errors = TrayValidator.Validate([MakeTube(1, isLast: false), second]);
        var error = Assert.Single(errors);
        Assert.Equal("Tray", error.Field);
    }


    private static Tube MakeTube(int seqNo = 1, bool isLast = true) => new(
        new TubeOrder(
            HostTerminal: 5,
            RoboTerminal: 1,
            Date: new DateOnly(2026, 11, 26),
            TrayNo: 32,
            IsLastTube: isLast,
            SeqNo: seqNo,
            QueueNo: 1234,
            PatientId: "0123456789",
            PatientName: "Tiger Woods",
            Container: "HbA1c",
            Volume: 1.0m,
            Department: "Pediatrics",
            LabelCount: 1,
            StockerCode: 99),
        Ward: "Pediatrics ward",
        Barcode: "A12345678901A");
}
