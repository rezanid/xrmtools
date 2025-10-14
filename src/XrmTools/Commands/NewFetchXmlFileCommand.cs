#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Resources;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.NewFetchXmlFileCmdId)]
internal sealed class NewFetchXmlFileCommand : BaseCommand<NewFetchXmlFileCommand>
{
    private const string content = """
<?xml version="1.0" encoding="utf-8" ?>
<fetch xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="{0}">
  <entity name="account">
  </entity>
</fetch>
""";
    [Import]
    public ILogger<NewFetchXmlFileCommand>? Logger { get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        EnsureDependencies();
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Could not determine the active project. Select a file or folder in Solution Explorer and try again.");
            return;
        }
        var fileName = GenerateUniqueFilename(proj);
        var activeItem = await FileHelper.FindActiveItemAsync();

        string projectRoot = Path.GetDirectoryName(proj.FullPath);
        var schemaAbs = Path.Combine(projectRoot ?? string.Empty, ".xrmtools", "schemas", "Fetch.xsd");
        var relativeSchema = PathHelper.GetRelativePath(fileName, schemaAbs, '/');

        await FileHelper.AddItemAsync(fileName, string.Format(content, relativeSchema), activeItem ?? proj, selectItem: true);
    }

    [MemberNotNull(nameof(Logger))]
    private void EnsureDependencies()
    {
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(NewFetchXmlFileCommand), nameof(Logger)));
    }

    private string GenerateUniqueFilename(Project proj)
    {
        var existingNames = Directory.EnumerateFiles(Path.GetDirectoryName(proj.FullPath), "*.fetch");
        for (int i = 1; i < 1000; i++)
        {
            var name = $"Fetch{i}.fetch";
            if (!existingNames.Any(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase)))
                return name;
        }
        throw new InvalidOperationException("Unable to generate a unique filename.");
    }
}
#nullable restore