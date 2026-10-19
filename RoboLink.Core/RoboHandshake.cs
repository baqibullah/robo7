namespace RoboLink.Core;

public class RoboHandshake(IRoboFolder folder, HandshakeOptions options)
{
    public async Task<List<ValidationError>> SendAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        var errors = TrayValidator.Validate(tray);
        if (errors.Count > 0)
            return errors;


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
                // We'll add loggin later.
            }
        }
    }
}
