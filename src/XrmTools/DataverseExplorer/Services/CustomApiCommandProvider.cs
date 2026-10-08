#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        yield return new ExplorerMenuItem
        {
            Header = "Generate client code",
            Children =
            [
                Item("C# (for plugins)", CustomApiClientLanguage.PluginCSharp, api.CustomApiId, node.SessionToken),
                Item("TypeScript (Dataverse forms)", CustomApiClientLanguage.TypeScript, api.CustomApiId, node.SessionToken),
                Item("HTTP", CustomApiClientLanguage.Http, api.CustomApiId, node.SessionToken),
                Item("OData", CustomApiClientLanguage.OData, api.CustomApiId, node.SessionToken),
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
            await GeneratedCustomApiDocument.OpenAsync(result, "client", target.CancellationToken);
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
