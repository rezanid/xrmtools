namespace XrmGen.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class OutputPaneLoggerExtensions
{
    public static ILoggingBuilder AddOutputLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, OutputLoggerProvider>();
        return builder;
    }
}