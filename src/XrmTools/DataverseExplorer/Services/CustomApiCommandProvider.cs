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
            var dte = await VS.GetServiceAsync<EnvDTE.DTE, EnvDTE.DTE>();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(target.CancellationToken);
            // A new document retains the suggested filename and prompts for a destination on Save.
            // Insert into the returned window, since another document may already be active.
            var window = dte.ItemOperations.NewFile(@"General\Text File", result.FileName, EnvDTE.Constants.vsViewKindTextView);
            var document = window.Document;
            if (document.Object("TextDocument") is not EnvDTE.TextDocument textDocument)
                throw new InvalidOperationException("Visual Studio could not create a text document for the generated client.");
            textDocument.StartPoint.CreateEditPoint().Insert(result.Content);
            document.Saved = false;
            window.Activate();
            await VS.StatusBar.ShowMessageAsync("Generated client opened as an unsaved document. Save to choose its location.");
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
