using RoboLink.Core;

namespace RoboLink.Tests;

public class DatFileNameTests
{
    [Fact]
    public void Build_first_sample_name()
    {
        var order = new TubeOrder(HostTerminal: 5, RoboTerminal: 1, Date: new DateOnly(2026, 11, 26),
            TrayNo: 32, IsLastTube: false, SeqNo: 1, QueueNo: 1234,
            PatientId: "0123456789", PatientName: "Tiger Woods",
            Container: "HbA1c", Volume: 1.0m, Department: "Pediatrics",
            LabelCount: 2, StockerCode: 99);

        Assert.Equal(
            "0005,0001,1126,0032,0,01,1234,0123456789,Tiger Woods,HbA1c,1.0,Pediatrics,2,99,,0.dat",
            DatFileName.Build(order));
    }

    [Fact]
    public void Blank_queue_no_is_four_spaces()
    {
        var order = new TubeOrder(HostTerminal: 5, RoboTerminal: 1, Date: new DateOnly(2026, 11, 26),
            TrayNo: 32, IsLastTube: false, SeqNo: 1, QueueNo: null,
            PatientId: "0123456789", PatientName: "Tiger Woods",
            Container: "HbA1c", Volume: 1.0m, Department: "Pediatrics",
            LabelCount: 2, StockerCode: 99);

        Assert.Equal(
            "0005,0001,1126,0032,0,01,    ,0123456789,Tiger Woods,HbA1c,1.0,Pediatrics,2,99,,0.dat",
            DatFileName.Build(order));
    }
}
