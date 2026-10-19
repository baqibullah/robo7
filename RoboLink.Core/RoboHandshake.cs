namespace RoboLink.Core;

public class RoboHandshake(IRoboFolder folder, HandshakeOptions options)
{
    public async Task<List<ValidationError>> SendAsync(IReadOnlyList<Tube> tray, CancellationToken ct = default)
    {
        var errors = TrayValidator.Validate(tray);
        if (errors.Count > 0)
            return errors;

        foreach (var tube in tray.OrderBy(t => t.TubeOrder.SeqNo))
        {
            var name = DatFileName.Build(tube.TubeOrder);
            var body = DatBody.Build(tube.TubeOrder, tube.Ward, tube.Barcode);

            await folder.WriteAsync(name, DatEncoder.Encode(body, options.UseBom), ct);
        }

        return errors;
    }
}
