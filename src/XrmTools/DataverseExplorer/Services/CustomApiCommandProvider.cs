#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.CodeGen.CustomApi;
using XrmTools.DataverseExplorer.Models;
using XrmTools.Logging.Compatibility;

[Export(typeof(IExplorerCommandProvider))]
internal sealed class CustomApiCommandProvider : IExplorerCommandProvider
{
    private readonly ICustomApiClientGenerator _generator;
    private readonly ILogger _logger;
    private readonly AsyncRelayCommand<GenerateClientTarget> _command;

    [ImportingConstructor]
    public CustomApiCommandProvider(ICustomApiClientGenerator generator, ILogger<CustomApiCommandProvider> logger)
    {
        _generator = generator;
        _logger = logger;
        _command = new AsyncRelayCommand<GenerateClientTarget>(ExecuteAsync,
            target => target != null && !target.CancellationToken.IsCancellationRequested);
    }

    public IEnumerable<ExplorerMenuItem> GetCommands(ExplorerNodeBase node)
    {
        if (node is not CustomApiNode api || api.CustomApiId == Guid.Empty) yield break;
        var root = node;
        while (root.Parent != null) root = root.Parent;
        yield return new ExplorerMenuItem
        {
            Header = "Generate client code",
            Children =
            [
                Item("C# (for plugins)", CustomApiClientLanguage.PluginCSharp, api.CustomApiId, root.SessionToken),
                Item("TypeScript (Dataverse forms)", CustomApiClientLanguage.TypeScript, api.CustomApiId, root.SessionToken),
                Item("HTTP", CustomApiClientLanguage.Http, api.CustomApiId, root.SessionToken),
                Item("OData", CustomApiClientLanguage.OData, api.CustomApiId, root.SessionToken),
            ],
        };
    }

    private ExplorerMenuItem Item(string header, CustomApiClientLanguage language, Guid id, CancellationToken token) => new()
    {
        Header = header, Command = _command, CommandParameter = new GenerateClientTarget(id, language, token),
    };

    private async Task ExecuteAsync(GenerateClientTarget? target)
    {
        if (target == null || target.CancellationToken.IsCancellationRequested) return;
        try
        {
            var result = await _generator.GenerateAsync(target.ApiId, target.Language, target.CancellationToken);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(target.CancellationToken);
            // Each invocation has its own folder. No project items or existing files are changed.
            var directory = Path.Combine(Path.GetTempPath(), "XrmTools", "GeneratedClients", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, result.FileName);
            File.WriteAllText(path, result.Content, new UTF8Encoding(false));
            await VS.Documents.OpenAsync(path);
            await VS.StatusBar.ShowMessageAsync("Generated client opened as a temporary document. Use Save As to keep it.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom API client generation failed.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Client code generation failed. " + ex.Message);
        }
    }
}

internal sealed class GenerateClientTarget(Guid apiId, CustomApiClientLanguage language, CancellationToken cancellationToken)
{
    public Guid ApiId { get; } = apiId;
    public CustomApiClientLanguage Language { get; } = language;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}
