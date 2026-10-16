using System.Text;

namespace RoboLink.Core;

public static class DatEncoder
{
    private static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];

    public static byte[] Encode(string body, bool useBom)
    {
        var bytes = new UTF8Encoding(false).GetBytes(body);
        return useBom ? [.. Bom, .. bytes] : bytes;
    }
}
