namespace RoboLink.Core;

public record ValidationError(string Field, string Message);

public static class OrderValidator
{
    private static readonly char[] InvalidCharacters = [',', '|', '^', '\r', '\n'];

    public static List<ValidationError> ValidateOrder(TubeOrder o, string ward, string barcode)
    {
        var errors = new List<ValidationError>();

        CheckForbiddenChars(errors, "Patient ID", o.PatientId);
        CheckForbiddenChars(errors, "Patient name", o.PatientName);
        CheckForbiddenChars(errors, "Container", o.Container);
        CheckForbiddenChars(errors, "Department", o.Department);
        CheckForbiddenChars(errors, "Ward", ward);
        CheckForbiddenChars(errors, "Barcode", barcode);
        CheckForbiddenChars(errors, "RFID", o.Rfid);

        return errors;
    }

    private static void CheckForbiddenChars(List<ValidationError> errors, string field, string value)
    {
        if (value.IndexOfAny(InvalidCharacters) > 0)
        {
            errors.Add(new ValidationError(field, $"{field} can't contain a comma, |, ^ or a line break."));
        }
    }
}
