namespace RoboLink.Core;

/// <summary>
/// Describes the DAT filename, Ward and Barcode are not part of this record as per DAT file name structure.
/// </summary>
public record TubeOrder(
    int HostTerminal,
    int RoboTerminal,
    DateOnly Date,
    int TrayNo,
    bool IsLastTube,
    int SeqNo,
    int? QueueNo,
    string PatientId,
    string PatientName,
    string Container,
    decimal Volume,
    string Department,
    int LabelCount,
    int StockerCode,
    string Rfid = "",
    int Priority = 0
);
