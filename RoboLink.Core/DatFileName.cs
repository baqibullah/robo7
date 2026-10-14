using System.Globalization;

namespace RoboLink.Core;

public static class DatFileName
{
    public static string Build(TubeOrder o) => string.Join(",",
        o.HostTerminal.ToString("D4"),
        o.RoboTerminal.ToString("D4"),
        o.Date.ToString("MMdd", CultureInfo.InvariantCulture),
        o.TrayNo.ToString("D4"),
        o.IsLastTube ? "9" : "0",
        o.SeqNo.ToString("D2"),
        o.QueueNo is { } q ? q.ToString("D4") : new string(' ', 4),
        o.PatientId,
        o.PatientName,
        o.Container,
        o.Volume.ToString("0.0", CultureInfo.InvariantCulture),
        o.Department,
        o.LabelCount.ToString(CultureInfo.InvariantCulture),
        o.StockerCode.ToString("D2"),
        o.Rfid,
        o.Priority.ToString(CultureInfo.InvariantCulture)) + ".dat";
}
