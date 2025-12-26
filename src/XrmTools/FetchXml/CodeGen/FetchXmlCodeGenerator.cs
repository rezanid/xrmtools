#nullable enable
namespace XrmTools.FetchXml.CodeGen;

using Community.VisualStudio.Toolkit;
using Microsoft.Language.Xml;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Serialization;
using XrmTools.Xrm.Generators;

/// <summary>
/// Single-file generator for FetchXML.
/// </summary>
internal sealed class FetchXmlCodeGenerator : BaseCodeGeneratorWithSiteAsync
{
    public const string Name = "XrmTools FetchXml Generator";
    public const string Description = "Generates Fetch Query code from FetchXML.";

    [Import]
    internal ILogger<FetchXmlCodeGenerator> Logger { get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    public IXrmCodeGenerator Generator { get; set; }

    protected override string GetDefaultExtension() => ".g.cs";

    public FetchXmlCodeGenerator() => SatisfyImports();

    [MemberNotNull(nameof(TemplateFinder), nameof(Logger), nameof(Generator))]
    private void SatisfyImports()
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(FetchXmlCodeGenerator), nameof(Logger)));
        if (TemplateFinder == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(FetchXmlCodeGenerator), nameof(TemplateFinder)));
        if (Generator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(FetchXmlCodeGenerator), nameof(Generator)));
    }

    protected override async Task<byte[]?> GenerateCodeAsync(
        string inputFileName, string inputFileContent, string defaultNamespace,
        IVsGeneratorProgress generateProgress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var inputFile = await PhysicalFile.FromFileAsync(inputFileName);

        var templateFilePath = await TemplateFinder.FindFetchXmlTemplatePathAsync(inputFileName);

        if (string.IsNullOrEmpty(templateFilePath))
        {
            return Encoding.UTF8.GetBytes("// " + Strings.CodeGen_TemplateNotFound);
        }

        var document = Parser.ParseText(inputFileContent);
        if (document is null || document.RootSyntax is null)
        {
            return Encoding.UTF8.GetBytes("// Invalid FetchXML input.");
        }

        var fetchElement = document.Root;
        if (fetchElement is null || !string.Equals(fetchElement.Name, "fetch", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.UTF8.GetBytes("// Invalid FetchXML input: root element is not <fetch>.");
        }

        var cleanDocument = CleanupFetchXmlDocument(document);
        var fetchXmlString = cleanDocument.ToFullString();
        var inputModel = await ParseFetchXmlAsync(cleanDocument, fetchXmlString, CancellationToken.None).ConfigureAwait(false);
        inputModel.Raw = fetchXmlString;

        Generator.Config = new XrmCodeGenConfig
        {
            DefaultNamespace = defaultNamespace,
            TemplateFilePath = templateFilePath,
            InputFileName = Path.GetFileNameWithoutExtension(inputFileName)
        };

        if (GeneralOptions.Instance.LogLevel == LogLevel.Trace)
        {
            var serializedConfig = JsonConvert.SerializeObject(inputModel, new JsonSerializerSettings
            {
                ContractResolver = new PolymorphicContractResolver()
            });
            File.WriteAllText(Path.ChangeExtension(inputFileName, ".model.json"), serializedConfig);
        }

        return Encoding.UTF8.GetBytes(Generator.GenerateCode(inputModel));
    }

    private static XmlDocumentSyntax CleanupFetchXmlDocument(XmlDocumentSyntax document)
    {
        var nodeToRemove = new List<SyntaxNode>();
        foreach (var node in document.ChildNodes)
        {
            if (node.Kind is SyntaxKind.XmlComment or SyntaxKind.XmlProcessingInstruction or SyntaxKind.XmlDeclaration)
            {
                nodeToRemove.Add(node);
            }
        }
        foreach (var node in document.Root.AsSyntaxElement.AttributesNode)
        {
            if (node.Name.IndexOf(':') > -1)
            {
                nodeToRemove.Add(node);
            }
        }
        return document.RemoveNodes(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
    }

    private async Task<Model.FetchQuery> ParseFetchXmlAsync(
        XmlDocumentSyntax docSyntax, string rawDocument, CancellationToken cancellationToken)
    {
        if (docSyntax == null) throw new ArgumentNullException(nameof(docSyntax));
        var parser = new FetchXmlParser();
        return await parser.ParseAsync(docSyntax, rawDocument, cancellationToken).ConfigureAwait(false);
    }

    private string GetDefaultNamespace(string inputFilePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(inputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out var defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }
}
#nullable restore