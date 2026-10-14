using RoboLink.Core;

namespace RoboLink.Tests;

public class DatBodyTests
{
    [Fact]
    public void Builds_first_sample_body()
    {
        var order = new TubeOrder(5, 1, new DateOnly(2026, 11, 26), 32, false, 1, 1234,
            "0123456789", "Tiger Woods", "HbA1c", 1.0m, "Pediatrics", 2, 99);

        var body = DatBody.Build(order, ward: "Pediatrics ward", barcode: "A12345678901A");

        var expectedDataFormat = new[]
        {
            "STR|1.0^1.0^Tahoma^9.0^Pediatrics^0^^0^0|",
            "STR|21.0^1.0^Tahoma^9.0^0123456789^0^^0^0|",
            "STR|1.0^5.0^Tahoma^9.0^Pediatrics ward^0^^0^0|",
            "STR|21.0^6.0^Tahoma^12.0^Tiger Woods^0^^0^0|",
            "STR|1.0^24.5^Tahoma^9.0^HbA1c^0^^0^0|",
            "STR|8.0^18.5^Tahoma^9.0^A12345678901A^0^^0^0|",
            "STR|27.0^21.5^Tahoma^9.0^NW7^0^^0^0|",
            "STR|27.0^24.5^Tahoma^9.0^Non check digit^0^^0^0|",
            "BAR|5^10^9^4^1^4^9^0^0^A12345678901A|",
            "BOX|42.0^0.5^47.0^4.5^0^1|",
            "STR|43.0^1.0^Tahoma^10.0^E^0^^1^0|",
        };

        var expected = string.Join(DatBody.NewLine, expectedDataFormat);

        Assert.Equal(expected, body);
    }
}
