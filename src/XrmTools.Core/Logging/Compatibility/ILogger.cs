namespace XrmTools.Logging.Compatibility;
using System;

public enum LogLevel
{
    /// <summary>
    /// Logs that contain the most detailed messages. These messages may contain sensitive
    /// application data. These messages are disabled by default and should never be
    /// enabled in a production environment.
    /// </summary>
    Trace,
    /// <summary>
    /// Logs that are used for interactive investigation during development. These logs
    /// should primarily contain information useful for debugging and have no long-term
    /// value.
    /// </summary>
    Debug,
    /// <summary>
    /// Logs that track the general flow of the application. These logs should have long-term
    /// value.
    /// </summary>
    Information,
    /// <summary>
    /// Logs that highlight an abnormal or unexpected event in the application flow,
    /// but do not otherwise cause the application execution to stop.
    /// </summary>
    Warning,
    /// Logs that highlight when the current flow of execution is stopped due to a failure.
    /// These should indicate a failure in the current activity, not an application-wide
    /// failure.
    /// </summary>
    Error,
    /// <summary>
    /// Logs that describe an unrecoverable application or system crash, or a catastrophic
    /// failure that requires immediate attention.
    /// </summary>
    Critical,
    /// <summary>
    /// Not used for writing log messages. Specifies that a logging category should not
    /// write any messages.
    /// </summary>
    None
}

/// <summary>
/// Represents a type used to perform logging.
/// </summary>
/// <remarks>Aggregates most logging patterns to a single method.</remarks>
public interface ILogger
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a System.String message of the state and exception.</param>
    void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);

    /// <summary>
    /// Checks if the given logLevel is enabled.
    /// </summary>
    /// <param name="logLevel">level to be checked.</param>
    /// <returns>true if enabled.</returns>
    bool IsEnabled(LogLevel logLevel);

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The identifier for the scope.</typeparam>
    /// <param name="state"></param>
    /// <returns>An System.IDisposable that ends the logical operation scope on dispose.</returns>
    IDisposable BeginScope<TState>(TState state);
}

/// <summary>
/// A generic interface for logging where the category name is derived from the specified
/// TCategoryName type name. Generally used to enable activation of a named <seealso cref="ILogger"/>
/// from MEF.
/// </summary>
/// <typeparam name="TCategoryName">The type who's name is used for the logger category name.</typeparam>
public interface ILogger<out TCategoryName> : ILogger
{
}