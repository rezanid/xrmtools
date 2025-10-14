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
/// Synchronous single-file generator for FetchXML (VS reliably invokes sync generators).
/// </summary>
internal sealed class FetchXmlCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmTools FetchXml Generator";
    public const string Description = "Generates code artifacts from FetchXML.";

    [Import]
    internal ILogger<FetchXmlCodeGenerator> Logger { get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    public IXrmCodeGenerator Generator { get; set; }

    public override string GetDefaultExtension() => ".g.cs";

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

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent))
        {
            return Encoding.UTF8.GetBytes("// Empty FetchXML input.");
        }
        return ThreadHelper.JoinableTaskFactory.Run(() => GenerateCodeAsync(inputFileName, inputFileContent));
    }

    private async Task<byte[]> GenerateCodeAsync(string inputFileName, string inputFileContent)
    { 
        try
        {
            var inputFile = await PhysicalFile.FromFileAsync(inputFileName);
            if (inputFile is null || inputFile.FindParent(SolutionItemType.Project) is not Project project)
            {
                return Encoding.UTF8.GetBytes(
                    "// Xrm Tools FetchXML Code Generator was not able to find the parent project of the input file " +
                    InputFilePath);
            }

            var templateFilePath = await TemplateFinder.FindFetchXmlTemplatePathAsync(InputFilePath);
            if (templateFilePath is null)
            {
                Logger.LogTrace("No template found for FetchXML code generation.");
                if (templateFilePath == string.Empty) return Encoding.UTF8.GetBytes("// No template found for FetchXML code generation.");
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
            var inputModel = await ParseFetchXmlAsync(cleanDocument, CancellationToken.None).ConfigureAwait(false);
            inputModel.Raw = fetchXmlString;

            Generator.Config = new XrmCodeGenConfig
            {
                DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
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

            string? generatedCode = null;
            if (project.IsSdkStyle())
            {
                var lastGenFileName = await inputFile.GetAttributeAsync("LastGenOutput");
                if (string.IsNullOrWhiteSpace(lastGenFileName))
                {
                    lastGenFileName = Path.ChangeExtension(Path.GetFileName(inputFileName), ".g.cs");
                    await inputFile.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, lastGenFileName);
                }
                var lastGenFilePath = Path.Combine(Path.GetDirectoryName(inputFileName), lastGenFileName);
                generatedCode = Generator.GenerateCode(inputModel);
                File.WriteAllText(lastGenFilePath, "// SDK-Style Code Gen\r\n" + generatedCode);
            }
            generatedCode ??= Generator.GenerateCode(inputModel);
            return Encoding.UTF8.GetBytes(generatedCode);
        }
        catch (Exception ex)
        {
            return Encoding.UTF8.GetBytes("// Error generating from FetchXML: " + ex.Message);
        }
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

    private async Task<Model.FetchQuery> ParseFetchXmlAsync(XmlDocumentSyntax docSyntax, CancellationToken cancellationToken)
    {
        if (docSyntax == null) throw new ArgumentNullException(nameof(docSyntax));
        var parser = new FetchXmlParser();
        return await parser.ParseAsync(docSyntax, cancellationToken).ConfigureAwait(false);
    }

    private string GetDefaultNamespace()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(InputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out var defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }
}
#nullable restore