namespace RoboLink.Core;

/// <summary>
/// One physical sample tube: one label and one DAT file.
/// <br/>
/// A Patient's tray is made up of several tubes, so one patient is a list of Tube.
/// </summary>
/// <param name="TubeOrder">Data for the DAT file name, as defined in the manual.</param>
/// <param name="Ward">Ward text printed on the label.</param>
/// <param name="Barcode">Barcode printed on the label.</param>
public record Tube(TubeOrder TubeOrder, string Ward, string Barcode);