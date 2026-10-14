using System.Text;

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

        CheckRequired(errors, "Patient ID", o.PatientId);
        CheckRequired(errors, "Patient name", o.PatientName);
        CheckRequired(errors, "Container", o.Container);
        CheckRequired(errors, "Department", o.Department);
        CheckRequired(errors, "Barcode", barcode);

        CheckMaxBytes(errors, "Patient ID", o.PatientId, 20);
        CheckMaxBytes(errors, "Patient name", o.PatientName, 20);
        CheckMaxBytes(errors, "Container", o.Container, 20);
        CheckMaxBytes(errors, "Department", o.Department, 20);
        CheckMaxBytes(errors, "RFID", o.Rfid, 15);

        return errors;
    }

    private static void CheckForbiddenChars(List<ValidationError> errors, string field, string value)
    {
        if (value.IndexOfAny(InvalidCharacters) > 0)
        {
            errors.Add(new ValidationError(field, $"{field} can't contain a comma, |, ^ or a line break."));
        }
    }

    private static void CheckRequired(List<ValidationError> errors, string field, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError(field, $"{field} is required."));
        }
    }

    private static void CheckMaxBytes(List<ValidationError> errors, string field, string value, int max)
    {
        if (Encoding.UTF8.GetByteCount(value) > max)
        {
            errors.Add(new ValidationError(field, $"{field} is too long (max {max} bytes)."));
        }
    }
}
