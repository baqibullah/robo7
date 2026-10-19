using RoboLink.Core;

namespace RoboLink.Tests;

internal static class TestData
{
    public static Tube MakeTube(int seqNo = 1, bool isLast = true) => new(
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
