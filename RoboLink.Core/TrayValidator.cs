namespace RoboLink.Core;

public static class TrayValidator
{
    public static List<ValidationError> Validate(IReadOnlyList<Tube> tray)
    {
        var errors = new List<ValidationError>();

        if (tray.Count == 0)
        {
            errors.Add(new ValidationError("Tray", "A tray needs at least one tube."));
            return errors;
        }

        var first = tray[0].TubeOrder;
        foreach (var tube in tray.Skip(1))
        {
            var o = tube.TubeOrder;
            if (o.HostTerminal != first.HostTerminal
            || o.RoboTerminal != first.RoboTerminal
            || o.Date != first.Date
            || o.TrayNo != first.TrayNo)
            {
                errors.Add(new ValidationError("Tray",
                $"Tube {o.SeqNo} has different terminals, date or tray number than the first tube."));
            }
        }

        foreach (var tube in tray)
        {
            foreach (var e in OrderValidator.ValidateOrder(tube.TubeOrder, tube.Ward, tube.Barcode))
                errors.Add(e with { Field = $"Tube {tube.TubeOrder.SeqNo}: {e.Field}" });
        }

        return errors;
    }
}
