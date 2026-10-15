using System.Buffers;
using System.Text;

namespace RoboLink.Core;

public record ValidationError(string Field, string Message);

public static class OrderValidator
{

    private static readonly SearchValues<char> InvalidCharacters = SearchValues.Create(",|^\r\n");
    private static readonly SearchValues<char> InvalidFileNameChars = SearchValues.Create("\\/:*?\"<>");

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

        CheckRange(errors, "Host terminal", o.HostTerminal, 0, 9999);
        CheckRange(errors, "ROBO terminal", o.RoboTerminal, 0, 9999);
        CheckRange(errors, "Tray number", o.TrayNo, 0, 9999);
        CheckRange(errors, "Sequence number", o.SeqNo, 1, 99);

        if (o.QueueNo is { } q)
            CheckRange(errors, "Queue number", q, 0, 9999);

        CheckRange(errors, "Label count", o.LabelCount, 1, 99);
        CheckRange(errors, "Priority", o.Priority, 0, 0);

        if (o.Volume < 0 || o.Volume > 999.9m)
            errors.Add(new ValidationError("Volume", "Volume must be between 0 and 999.9."));

        int[] stockers = [1, 2, 3, 4, 5, 6, 99];
        if (!stockers.Contains(o.StockerCode))
            errors.Add(new ValidationError("Stocker code", "Stocker code must be 1 to 6, or 99 for a manual label."));


        CheckFileNameChars(errors, "Patient ID", o.PatientId);
        CheckFileNameChars(errors, "Patient name", o.PatientName);
        CheckFileNameChars(errors, "Container", o.Container);
        CheckFileNameChars(errors, "Department", o.Department);

        return errors;
    }

    private static void CheckForbiddenChars(List<ValidationError> errors, string field, string value)
    {
        if (value.AsSpan().ContainsAny(InvalidCharacters))
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

    private static void CheckRange(List<ValidationError> errors, string field, int value, int min, int max)
    {
        if (value < min || value > max)
            errors.Add(new ValidationError(field, $"{field} must be between {min} and {max}."));
    }

    private static void CheckFileNameChars(List<ValidationError> errors, string field, string value)
    {
        if (value.AsSpan().ContainsAny(InvalidFileNameChars))
            errors.Add(new ValidationError(field, $"{field} can't contain \\ / : * ? \" < or >."));
    }
}
