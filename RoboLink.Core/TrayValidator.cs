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

        // Cross tube check making sure each Tube in belonging to on tray has the same:
        // 1. HostTerminal
        // 2. RoboTerminal
        // 3. Date
        // 4. TrayNo
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

        // Sequenece check: 1,2,3 (involving no gaps and exactly one tube is last)
        var seqNumbers = tray.Select(t => t.TubeOrder.SeqNo).Order().ToList();
        if (!seqNumbers.SequenceEqual(Enumerable.Range(1, tray.Count)))
            errors.Add(new ValidationError("Tray", "Sequence numbers must run 1, 2, 3... with no gaps or duplicates."));

        var lastTubes = tray.Where(t => t.TubeOrder.IsLastTube).ToList();
        if (lastTubes.Count != 1 || lastTubes[0].TubeOrder.SeqNo != tray.Count)
            errors.Add(new ValidationError("Tray", "Exactly one tube must be marked last, and it must have the highest sequence number."));

        foreach (var tube in tray)
        {
            foreach (var e in OrderValidator.ValidateOrder(tube.TubeOrder, tube.Ward, tube.Barcode))
                errors.Add(e with { Field = $"Tube {tube.TubeOrder.SeqNo}: {e.Field}" });
        }

        return errors;
    }
}
