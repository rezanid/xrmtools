#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using XrmTools.Logging;

[Export]
[method: ImportingConstructor]
internal sealed class DataverseSolutionOutput(IOutputLoggerService outputLoggerService)
{
    private readonly IOutputLoggerService _outputLoggerService = outputLoggerService;

    public void WriteHeader(CdsProjectInfo project, string? environmentUrl, string commandText)
    {
        _outputLoggerService.OutputString($"XrmTools Dataverse Solution{Environment.NewLine}");
        _outputLoggerService.OutputString($"Project: {project.ProjectName}{Environment.NewLine}");
        if (!string.IsNullOrWhiteSpace(environmentUrl))
        {
            _outputLoggerService.OutputString($"Environment: {environmentUrl}{Environment.NewLine}");
        }

        _outputLoggerService.OutputString($"Command: {commandText}{Environment.NewLine}");
    }

    public IProgress<ProcessOutputLine> CreateProgress()
        => new Progress<ProcessOutputLine>(line =>
            _outputLoggerService.OutputString($"{line.Text}{Environment.NewLine}"));

    public void WriteBlankLine() => _outputLoggerService.OutputString(Environment.NewLine);

    public void WriteCompleted() => _outputLoggerService.OutputString($"Completed successfully.{Environment.NewLine}");

    public void WriteWarning(string message) => _outputLoggerService.OutputString($"Warning: {message}{Environment.NewLine}");

    public Task ShowPaneAsync() => VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name);
}
#nullable restore
