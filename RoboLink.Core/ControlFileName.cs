using System.Globalization;

namespace RoboLink.Core;

public enum ControlResult { Ok, Ng }

// Describes the shared DEF/OK/NG file names.
public record ControlFileName(int HostTerminal, int RoboTerminal, int Month, int Day, int TrayNo)
{
    public static ControlFileName From(TubeOrder o) =>
        new(o.HostTerminal, o.RoboTerminal, o.Date.Month, o.Date.Day, o.TrayNo);

    // Builds the DEF file.
    public string BuildDef() => string.Join(",",
       HostTerminal.ToString("D4"),
       RoboTerminal.ToString("D4"),
       Month.ToString("D2") + Day.ToString("D2"),
       TrayNo.ToString("D4"),
       "DEF.TXT");

    // Builds the OK/NG file.
    public static bool TryParse(string name, out ControlFileName? result, out ControlResult kind)
    {
        result = null;
        kind = default;

        // Split the file name by a comma into 4 equal parts: 0005,0001,1126,0032,DEF.TXT
        // Such that we have ['0005', '0001', '1126', '0032', 'DEF.TXT']
        var parts = name.Split(',');
        if (parts.Length != 5) return false;

        if (parts[4].Equals("OK.txt", StringComparison.OrdinalIgnoreCase)) kind = ControlResult.Ok;
        else if (parts[4].Equals("NG.txt", StringComparison.OrdinalIgnoreCase)) kind = ControlResult.Ng;
        else return false;

        if (parts[0].Length != 4 || parts[1].Length != 4 || parts[2].Length != 4 || parts[3].Length != 4)
            return false;

        const NumberStyles digitsOnly = NumberStyles.None;
        var culture = CultureInfo.InvariantCulture;

        if (!int.TryParse(parts[0], digitsOnly, culture, out var host)) return false;
        if (!int.TryParse(parts[1], digitsOnly, culture, out var robo)) return false;
        if (!int.TryParse(parts[2].AsSpan(0, 2), digitsOnly, culture, out var month)) return false;
        if (!int.TryParse(parts[2].AsSpan(2, 2), digitsOnly, culture, out var day)) return false;
        if (!int.TryParse(parts[3], digitsOnly, culture, out var tray)) return false;

        if (month is < 1 or > 12 || day is < 1 or > 31) return false;

        result = new ControlFileName(host, robo, month, day, tray);
        return true;
    }
}
