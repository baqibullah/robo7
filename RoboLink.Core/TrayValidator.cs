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

        foreach (var tube in tray)
        {
            foreach (var e in OrderValidator.ValidateOrder(tube.TubeOrder, tube.Ward, tube.Barcode))
                errors.Add(e with { Field = $"Tube {tube.TubeOrder.SeqNo}: {e.Field}" });
        }

        return errors;
    }
}
