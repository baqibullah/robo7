namespace RoboLink.Core;

public class RoboHandshake(IRoboFolder folder, HandshakeOptions options)
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
        var reply = RoboReply.Pending;

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

            if (kind == ControlResult.Ng)
                return RoboReply.Ng;

            reply = RoboReply.Ok;
        }

        return reply;
    }

    public async Task CleanupAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        throw new NotImplementedException();
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
            catch
            {
                // We'll add logging later.
            }
        }
    }
}
