using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmGen.Extensions;
internal static class StringExtensions
{
    public static string LastSegment(this string str, char separator = '.')
    {
        var lastSeparator = str.LastIndexOf(separator);
        return lastSeparator == -1 ? str : str.Substring(lastSeparator + 1);
    }

    public static string FirstSegment(this string str, char separator = '.')
    {
        var firstSeparator = str.IndexOf(separator);
        return firstSeparator == -1 ? str : str.Substring(0, firstSeparator);
    }
}
