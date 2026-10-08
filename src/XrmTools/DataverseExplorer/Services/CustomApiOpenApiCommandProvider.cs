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
internal sealed class CustomApiOpenApiCommandProvider : IExplorerCommandProvider
{
    private readonly ICustomApiOpenApiGenerator _generator;
    private readonly ILogger _logger;
    private readonly AsyncRelayCommand<GenerateOpenApiTarget> _command;

    [ImportingConstructor]
    public CustomApiOpenApiCommandProvider(ICustomApiOpenApiGenerator generator, ILogger<CustomApiOpenApiCommandProvider> logger)
    {
        _generator = generator;
        _logger = logger;
        _command = new AsyncRelayCommand<GenerateOpenApiTarget>(ExecuteAsync, target => target != null && !target.Token.IsCancellationRequested);
    }

    public IEnumerable<ExplorerMenuItem> GetCommands(ExplorerNodeBase node)
    {
        if (node is not CustomApiNode api || api.CustomApiId == Guid.Empty) yield break;
        yield return new ExplorerMenuItem { Header = "Generate OpenAPI specification", Command = _command,
            CommandParameter = new GenerateOpenApiTarget(api.CustomApiId, node.SessionToken, api.TypeName, (api.Parent as AssemblyNode)?.DisplayName) };
    }

    private async Task ExecuteAsync(GenerateOpenApiTarget? target)
    {
        if (target == null || target.Token.IsCancellationRequested) return;
        try
        {
            var result = await _generator.GenerateAsync(target.ApiId, target.Token, target.TypeName, target.AssemblyName);
            await GeneratedCustomApiDocument.OpenAsync(result, "OpenAPI specification", target.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom API OpenAPI generation failed.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "OpenAPI generation failed. " + ex.Message);
        }
    }
}

internal sealed record GenerateOpenApiTarget(Guid ApiId, CancellationToken Token, string? TypeName, string? AssemblyName);

/// <summary>Shared presentation for generated Custom API artifacts.</summary>
internal static class GeneratedCustomApiDocument
{
    public static async Task OpenAsync(GeneratedClient result, string artifactName, CancellationToken token)
    {
        var dte = await VS.GetServiceAsync<EnvDTE.DTE, EnvDTE.DTE>()
            ?? throw new InvalidOperationException("Visual Studio's document service is unavailable.");
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var window = dte.ItemOperations.NewFile(@"General\Text File", result.FileName, EnvDTE.Constants.vsViewKindTextView);
        var document = window.Document;
        if (document.Object("TextDocument") is not EnvDTE.TextDocument textDocument)
            throw new InvalidOperationException("Visual Studio could not create a text document for the generated artifact.");
        textDocument.StartPoint.CreateEditPoint().Insert(result.Content);
        document.Saved = false;
        window.Activate();
        await VS.StatusBar.ShowMessageAsync("Generated " + artifactName + " opened as an unsaved document. Save to choose its location.");
    }
}
