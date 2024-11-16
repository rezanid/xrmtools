namespace XrmTools.Logging.Compatibility;
using System;
/// <summary>
/// Represents a type that can create instances of <see cref="ILogger"/>
/// </summary>
internal interface ILoggerProvider : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance.
    /// </summary>
    /// <param name="categoryName">The instance of <see cref="ILogger"/> that was created.</param>
    /// <returns>The instance of <see cref="ILogger"/> that was created.</returns>
    ILogger CreateLogger(string categoryName);
}