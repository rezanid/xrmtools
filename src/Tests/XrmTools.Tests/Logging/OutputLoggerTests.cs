namespace XrmTools.Tests.Logging;
using System;
using FluentAssertions;
using Xunit;
using XrmTools.Logging.Compatibility;

public class OutputLoggerTests
{
    [Fact]
    public void AppendException_Should_Include_Exception_Details()
    {
        var logRecord = "[Error] TestCategory: Failed to register plugin assembly.";
        var exception = new InvalidOperationException("Boom");

        var result = OutputLogger.AppendException(logRecord, exception);

        result.Should().StartWith(logRecord + Environment.NewLine);
        result.Should().Contain(exception.ToString());
    }
}
