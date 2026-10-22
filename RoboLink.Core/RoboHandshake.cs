using Microsoft.Extensions.Logging;

namespace RoboLink.Core;

public class RoboHandshake(IRoboFolder folder, HandshakeOptions options, ILogger<RoboHandshake> logger)
{
    public async Task<List<ValidationError>> SendAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        var errors = TrayValidator.Validate(tray);
        if (errors.Count > 0)
            return errors;

        // keeps track of the files that are written.
        // assuming halfway IO throws error, some files still be in that folder.
        var written = new List<string>();
        try
        {
            foreach (var tube in tray.OrderBy(t => t.TubeOrder.SeqNo))
            {
                var name = DatFileName.Build(tube.TubeOrder);
                var body = DatBody.Build(tube.TubeOrder, tube.Ward, tube.Barcode);

                await folder.WriteAsync(name, DatEncoder.Encode(body, options.UseBom), ct);
                written.Add(name);
            }

            var defName = ControlFileName.From(tray[0].TubeOrder).BuildDef();
            written.Add(defName);
            await folder.WriteAsync(defName, [], ct);
        }
        catch
        {
            logger.LogWarning("Send failed for tray {TrayNo}, removing {Count} written files.",
            tray[0].TubeOrder.TrayNo, written.Count);
            await RemoveAsync(written);
            throw;
        }

        return errors;
    }

    /// <summary>
    /// Goes through each file in the folder and check the replied state of the machine. 
    /// </summary>
    /// <param name="tray">Holds the tray's identity.</param>
    /// <param name="ct"></param>
    /// <returns>Replied state of the machine</returns>
    public async Task<RoboReply> CheckReplyAsync(ControlFileName tray, CancellationToken ct = default)
    {
        var sawOk = false;
        var sawNg = false;

        // We iterate through each file in the folder.
        foreach (var name in await folder.ListAsync(ct))
        {
            // Skip if it doesn't parses as OK/NG (so DEF and junk files are skipped)
            // Folder can hold files for multiple trays we use the ControlFileName parameter to match our tray's file.

            // Example: ControlFileName is 0005,0001,1126,0032,OK.txt

            // Folder containing several files:
            // 0005,0001,1126,0031,OK.txt: parses fine, but tray 31 isn’t ours, so skip.
            // 0005,0001,1126,0032,DEF.TXT: TryParse returns false because it’s not OK or NG, so skip.
            // 0005,0001,1126,0032,OK.txt: parses, tray 32 matches ours, so it counts.
            if (!ControlFileName.TryParse(name, out var parsed, out var kind) || parsed != tray)
                continue;

            if (kind == ControlResult.Ng) sawNg = true;
            else sawOk = true;
        }

        if (sawNg)
        {
            if (sawOk)
                logger.LogWarning("Tray {TrayNo}: both OK and NG replies found, treating as NG.", tray.TrayNo);

            logger.LogWarning("Machine rejected tray {TrayNo} (NG).", tray.TrayNo);
            return RoboReply.Ng;
        }
        return sawOk ? RoboReply.Ok : RoboReply.Pending;
    }

    // Deletes the DAT files in sequence order, so last tube's DAT goes last.
    // List the 
    public async Task CleanupAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        // We iterate through all DAT files and delete them in sequence order.
        foreach (var tube in tray.OrderBy(t => t.TubeOrder.SeqNo))
            await folder.DeleteAsync(DatFileName.Build(tube.TubeOrder), ct);

        var id = ControlFileName.From(tray[0].TubeOrder);

        // Deletes any OK/NG files matching our tray.
        foreach (var name in await folder.ListAsync(ct))
        {
            if (ControlFileName.TryParse(name, out var parsed, out _) && parsed == id)
                await folder.DeleteAsync(name, ct);
        }
    }

    /// <summary>
    /// Takes back an order machine hasn't answered yet.
    /// Deletes the DEF file first so the machine can't start on half-removed tray, then DAT files.
    /// </summary>
    public async Task CancelAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        await folder.DeleteAsync(ControlFileName.From(tray[0].TubeOrder).BuildDef(), ct);

        foreach (var tube in tray.OrderBy(t => t.TubeOrder.SeqNo))
        {
            await folder.DeleteAsync(DatFileName.Build(tube.TubeOrder), ct);
        }

    }

    private async Task RemoveAsync(List<string> names)
    {
        names.Reverse();
        foreach (var name in names)
        {
            try
            {
                await folder.DeleteAsync(name, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not delete {File} during rollback. It may still be in the robo folder.",
                    LogName(name));
            }
        }
    }

    // DAT names carry patient ID and name. Keep only the first six parts
    // (host, robo, date, tray, last flag, seq). A DEF name has five parts, so it stays whole.
    private static string LogName(string name) => string.Join(",", name.Split(',').Take(6));
}
