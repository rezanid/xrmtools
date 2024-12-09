#nullable enable
namespace XrmTools.Helpers;

using System.Collections.Immutable;
using System.Text;

public static class ByteExtensions
{
    public static string? ToHexString(this byte[] bytes)
    {
        if (bytes == null)
        {
            return null;
        }
        var hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    public static string? ToHexString(this ImmutableArray<byte> bytes)
    {
        if (bytes.IsDefaultOrEmpty)
        {
            return null;
        }
        var hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }
}
