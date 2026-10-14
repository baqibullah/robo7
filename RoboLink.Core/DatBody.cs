namespace RoboLink.Core;

public static class DatBody
{
    public const string NewLine = "\r";

    public static string Build(TubeOrder o, string ward, string barcode)
    {
        var lines = LabelTemplate.Default.Select(l => l
            .Replace("{Department}", o.Department)
            .Replace("{PatientId}", o.PatientId)
            .Replace("{Ward}", ward)
            .Replace("{PatientName}", o.PatientName)
            .Replace("{Container}", o.Container)
            .Replace("{Barcode}", barcode));

        return string.Join(NewLine, lines);
    }
}
