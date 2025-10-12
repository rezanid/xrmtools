namespace XrmTools.FetchXml;

using Microsoft.VisualStudio.Text;
using System;
using XrmTools.FetchXml.Margin;
using XrmTools.Logging.Compatibility;

public static class TextBufferExtensions
{
    public static FetchXmlDocument GetFetchXmlDocument(this ITextBuffer buffer, ILogger logger)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        return buffer.Properties.GetOrCreateSingletonProperty(() => new FetchXmlDocument(buffer, logger));
    }

    internal static Debouncer GetDebouncer(this ITextBuffer buffer, int millisecondsToWait = 500)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        return buffer.Properties.GetOrCreateSingletonProperty(() => new Debouncer(millisecondsToWait));
    }

    internal static Debouncer GetDebouncer(this ITextBuffer buffer, string name, int millisecondsToWait = 500)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        var key = $"__debouncer::{name}";
        if (!buffer.Properties.TryGetProperty(key, out Debouncer debouncer))
        {
            debouncer = new Debouncer(millisecondsToWait);
            buffer.Properties.AddProperty(key, debouncer);
        }
        return debouncer;
    }
}
