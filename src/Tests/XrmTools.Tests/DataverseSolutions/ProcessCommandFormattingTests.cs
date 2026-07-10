namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Xunit;
using XrmTools.DataverseSolutions;

public class ProcessCommandFormattingTests
{
    [Fact]
    public void FormatCommand_MasksSensitiveArguments()
    {
        var request = new ProcessCommandRequest
        {
            FileName = "pac",
            Arguments = ["auth", "create", "--clientSecret", "top secret"],
            WorkingDirectory = "C:\\temp",
            SensitiveValues = ["top secret"]
        };

        var formatted = ProcessCommandFormatting.FormatCommand(request);

        formatted.Should().Contain("pac auth create --clientSecret ***");
        formatted.Should().NotContain("top secret");
    }

    [Fact]
    public void SanitizeOutput_StripsAnsiCodes_AndMasksSensitiveValues()
    {
        var sanitized = ProcessCommandFormatting.SanitizeOutput("\u001b[31msecret\u001b[0m", ["secret"]);

        sanitized.Should().Be("***");
    }
}
