namespace RoboLink.Core;

public static class LabelTemplate
{
    public static readonly string[] Default = [
        "STR|1.0^1.0^Tahoma^9.0^{Department}^0^^0^0|",
        "STR|21.0^1.0^Tahoma^9.0^{PatientId}^0^^0^0|",
        "STR|1.0^5.0^Tahoma^9.0^{Ward}^0^^0^0|",
        "STR|21.0^6.0^Tahoma^12.0^{PatientName}^0^^0^0|",
        "STR|1.0^24.5^Tahoma^9.0^{Container}^0^^0^0|",
        "STR|8.0^18.5^Tahoma^9.0^{Barcode}^0^^0^0|",
        "STR|27.0^21.5^Tahoma^9.0^NW7^0^^0^0|",
        "STR|27.0^24.5^Tahoma^9.0^Non check digit^0^^0^0|",
        "BAR|5^10^9^4^1^4^9^0^0^{Barcode}|",
        "BOX|42.0^0.5^47.0^4.5^0^1|",
        "STR|43.0^1.0^Tahoma^10.0^E^0^^1^0|",
    ];
}
