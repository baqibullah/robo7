using RoboLink.Core;

namespace RoboLink.Tests;

public class DataEncoderTests
{

    [Fact]
    public void Without_bom_starts_with_first_character()
    {
        var bytes = DatEncoder.Encode("STR|", false);
        Assert.Equal("STR|"u8, bytes);
    }

    [Fact]
    public void With_bom_starts_with_three_bom_bytes()
    {
        var bytes = DatEncoder.Encode("STR|", true);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF, 0x53, 0x54, 0x52, 0x7C }, bytes);
    }

    [Fact]
    public void Matches_vendor_sample_byte_for_byte()
    {
        var order = new TubeOrder(5, 1, new DateOnly(2026, 11, 26), 32, false, 1, 1234,
            "0123456789", "Tiger Woods", "HbA1c", 1.0m, "Pediatrics", 2, 99);

        var expected = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "sample.dat"));

        var actual = DatEncoder.Encode(
            DatBody.Build(order, "Pediatrics ward", "A12345678901A"), useBom: false);

        Assert.Equal(expected, actual);
    }
}
