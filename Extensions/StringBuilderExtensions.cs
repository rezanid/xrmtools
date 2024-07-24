using System.Text;

namespace XrmGen.Extensions;

internal static class StringBuilderExtensions
{
    public static void AppendLineWhenNotEmpty(this StringBuilder sb, string value)
    {
        if (!string.IsNullOrEmpty(value)) sb.AppendLine(value);
    }

    public static void AppendWhenNotEmpty(this StringBuilder sb, string value)
    {
        if (!string.IsNullOrEmpty(value)) sb.AppendLine(value);
    }
}
