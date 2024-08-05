#nullable enable

namespace XrmGen.Extensions;
internal static class StringExtensions
{
    public static string LastSegment(this string str, char separator = '.')
    {
        var lastSeparator = str.LastIndexOf(separator);
        return lastSeparator == -1 ? str : str[(lastSeparator + 1)..];
    }

    public static string FirstSegment(this string str, char separator = '.')
    {
        var firstSeparator = str.IndexOf(separator);
        return firstSeparator == -1 ? str : str[..firstSeparator];
    }

    /// <summary>
    /// Satisfy NRT checks by ensuring a null string is never propagated
    /// </summary>
    /// <remarks>
    /// Various legacy APIs still return nullable strings (even if, in practice they
    /// never will actually be null) so we can use this extension to keep the NRT
    /// checks quiet</remarks>
    public static string EmptyWhenNull(this string? str) => str ?? string.Empty;
}
#nullable restore