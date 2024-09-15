namespace XrmGen.Logging;

using Microsoft.Extensions.Logging;

internal static class OutputPaneLoggerExtensions
{    
    public static ILoggingBuilder AddOutputLogger(this ILoggingBuilder builder)
    {
        builder.AddProvider(new OutputLoggerProvider());
        return builder;
    }
}