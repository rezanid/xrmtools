#nullable enable
namespace XrmTools.SuggestedActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using XrmTools.Meta.Model;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using global::XrmTools.WebApi.Entities;
using global::XrmTools.WebApi;
using XrmTools.WebApi.Methods;
using XrmTools.WebApi.Types;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using XrmTools.Meta.Attributes;
using CustomApiFieldType = WebApi.Types.CustomApiFieldType;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.Imaging;
using XrmTools.UI.Controls;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Differencing;

internal abstract class BaseCustomApiAction(
    ITrackingSpan span,
    string customApiName,
    IWebApiService webApi) : ISuggestedAction
{
    protected readonly ITrackingSpan span = span;
    protected readonly string customApiName = customApiName;
    protected readonly IWebApiService webApi = webApi;
    protected readonly ITextSnapshot snapshot = span.TextBuffer.CurrentSnapshot;
    protected string generatedCode;

    public abstract string DisplayText { get; }
    public virtual bool HasActionSets => false;
    public virtual bool HasPreview => true;
    public virtual string IconAutomationText => null;
    public virtual string InputGestureText => null;
    public virtual ImageMoniker IconMoniker => default;

    public async Task<object?> GetPreviewAsync(CancellationToken cancellationToken)
    {
        var updatedCode = await GenerateCodeAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(updatedCode))
            return null;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

        var contentTypeRegistry = componentModel?.GetService<IContentTypeRegistryService>();
        var textBufferFactory = componentModel?.GetService<ITextBufferFactoryService>();
        var diffBufferFactory = componentModel?.GetService<IDifferenceBufferFactoryService>();
        var diffViewerFactory = componentModel?.GetService<IWpfDifferenceViewerFactoryService>();

        if (contentTypeRegistry == null || textBufferFactory == null || diffBufferFactory == null || diffViewerFactory == null)
            return null;

        var contentType = contentTypeRegistry.GetContentType("CSharp");

        var originalText = span.GetSpan(snapshot).GetText();
        var leftBuffer = textBufferFactory.CreateTextBuffer(originalText, contentType);
        var rightBuffer = textBufferFactory.CreateTextBuffer(updatedCode, contentType);

        var diffBuffer = diffBufferFactory.CreateDifferenceBuffer(
            leftBuffer,
            rightBuffer, new StringDifferenceOptions(StringDifferenceTypes.Line | StringDifferenceTypes.Word, 0, true));

        var diffViewer = diffViewerFactory.CreateDifferenceView(diffBuffer);

        diffViewer.ViewMode = DifferenceViewMode.Inline;
        diffViewer.InlineView.Background = System.Windows.Media.Brushes.Transparent;

        return diffViewer.VisualElement;
    }

    public virtual Task<IEnumerable<SuggestedActionSet>?> GetActionSetsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<SuggestedActionSet>?>(null);

    public void Invoke(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(generatedCode))
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
                generatedCode = await GenerateCodeAsync().ConfigureAwait(false));
        }
        if (!string.IsNullOrEmpty(generatedCode))
        {
            span.TextBuffer.Replace(span.GetSpan(snapshot), generatedCode);
        }
    }

    public void Dispose() { }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        telemetryId = Guid.Empty;
        return false;
    }

    protected abstract Task<string> GenerateCodeAsync();

    protected static string MapType(CustomApiFieldType type, bool isNullable)
    => type switch
    {
        CustomApiFieldType.Boolean => isNullable ? "bool?" : "bool",
        CustomApiFieldType.DateTime => isNullable ? "DateTime?" : "DateTime",
        CustomApiFieldType.Decimal => isNullable ? "decimal?" : "decimal",
        CustomApiFieldType.Entity => isNullable ? "Entity?" : "Entity",
        CustomApiFieldType.EntityCollection => isNullable ? "EntityCollection?" : "EntityCollection",
        CustomApiFieldType.EntityReference => isNullable ? "EntityReference?" : "EntityReference",
        CustomApiFieldType.Float => isNullable ? "float?" : "float",
        CustomApiFieldType.Integer => isNullable ? "int?" : "int",
        CustomApiFieldType.Money => isNullable ? "Money?" : "Money",
        CustomApiFieldType.Picklist => isNullable ? "OptionSetValue?" : "OptionSetValue",
        CustomApiFieldType.String => isNullable ? "string?" : "string",
        CustomApiFieldType.StringArray => isNullable ? "string[]?" : "string[]",
        CustomApiFieldType.Guid => isNullable ? "Guid?" : "Guid",
        _ => isNullable ? "object?" : "object",
    };

    protected string GenerateAttribute(CustomApi customApi)
    {
        var sb = new StringBuilder();
        sb.Append($"CustomApi(\"{customApi.UniqueName}\"");
        if (!string.IsNullOrEmpty(customApi.Name)) sb.Append($", Name = \"{customApi.Name}\"");
        if (!string.IsNullOrEmpty(customApi.DisplayName)) sb.Append($", DisplayName = \"{customApi.DisplayName}\"");
        if (!string.IsNullOrEmpty(customApi.Description)) sb.Append($", Description = \"{customApi.Description}\"");
        if (customApi.StepType != CustomApi.ProcessingStepTypes.None) sb.Append($", StepType = {customApi.StepType}");
        if (customApi.BindingType != CustomApi.BindingTypes.Global) sb.Append($", BindingType = {customApi.BindingType}");
        if (!string.IsNullOrEmpty(customApi.BoundEntityLogicalName)) sb.Append($", BoundEntityLogicalName = \"{customApi.BoundEntityLogicalName}\"");
        if (!string.IsNullOrEmpty(customApi.ExecutePrivilegeName)) sb.Append($", ExecutePrivilegeName = \"{customApi.ExecutePrivilegeName}\"");
        sb.Append(customApi.IsFunction ? ", IsFunction = true" : ", IsFunction = false");
        sb.Append(customApi.IsPrivate ? ", IsPrivate = true" : ", IsPrivate = false");
        sb.Append(")");
        return sb.ToString();
    }
}

internal class FixCustomApiAttributeAction(
    ITrackingSpan span,
    string customApiName,
    IWebApiService webApi) : BaseCustomApiAction(span, customApiName, webApi)
{
    public override ImageMoniker IconMoniker => new() { Guid = PackageGuids.AssetsGuid, Id = PackageIds.Dataverse};
    public override string DisplayText => $"Expand [CustomApi(\"{customApiName}\")] with all properties from Dataverse";

    protected override async Task<string> GenerateCodeAsync()
    {
        try
        {
            var customApiResponse = await webApi.RetrieveMultipleAsync<CustomApi>($"customapis?$filter=uniquename eq '{customApiName}'").ConfigureAwait(false);
            var api = customApiResponse?.Value?.FirstOrDefault();
            return api == null ? null! : "[" + GenerateAttribute(api) + "]";
        }
        catch
        {
            return null!;
        }
    }
}

internal class FixCustomApiAttributeWithRequestResponseAction : BaseCustomApiAction
{
    public FixCustomApiAttributeWithRequestResponseAction(
        ITrackingSpan classSpan, string customApiName, IWebApiService webApi) : base (classSpan, customApiName, webApi)
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    [Import]

    internal ITextEditorFactoryService TextEditorFactoryService { get; set; } = null!;


    public override ImageMoniker IconMoniker => new() { Guid = PackageGuids.AssetsGuid, Id = PackageIds.Dataverse};
    public override string DisplayText => $"Expand [CustomApi(\"{customApiName}\")] and add Request/Response nested classes";

    public async override Task<object?> GetPreviewAsync(CancellationToken cancellationToken)
    {
        generatedCode = await GenerateCodeAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(generatedCode)) return null;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var textBuffer = span.TextBuffer;

        var textViewRoleSet = TextEditorFactoryService.CreateTextViewRoleSet(
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.ChangePreview);
        var textView = TextEditorFactoryService.CreateTextView(textBuffer, textViewRoleSet);
        var TextViewHost = TextEditorFactoryService.CreateTextViewHost(textView, false);
        return TextViewHost.HostControl;
    }

    protected override async Task<string> GenerateCodeAsync()
    {
        try
        {
            var document = span.TextBuffer.GetRelatedDocuments().FirstOrDefault();
            if (document == null) return null;

            var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
            if (root == null || semanticModel == null) return null;

            var snapshotSpan = span.GetSpan(snapshot);
            var textSpan = new TextSpan(snapshotSpan.Start.Position, snapshotSpan.Length);
            var classNode = root.FindNode(textSpan) as ClassDeclarationSyntax;
            if (classNode == null) return null;

            var nullableEnabled = semanticModel.GetNullableContext(classNode.SpanStart).AnnotationsEnabled();

            var attributeNode = classNode.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(attr => semanticModel.GetSymbolInfo(attr).Symbol is IMethodSymbol methodSymbol &&
                                        methodSymbol.ContainingType.Name == nameof(CustomApiAttribute));
            if (attributeNode == null) return null!;

            var customApiResponse =
                await webApi.RetrieveMultipleAsync<CustomApi>(
                    $"customapis?$select=uniquename,name,displayname,description,allowedcustomprocessingsteptype,bindingtype,boundentitylogicalname,executeprivilegename,isfunction,isprivate" +
                    $"&$filter=uniquename eq '{customApiName}'&$expand=CustomAPIRequestParameters($select=uniquename,name,displayname,description,type,logicalentityname,isoptional),CustomAPIResponseProperties($select=uniquename,name,displayname,description,type,logicalentityname)")
                .ConfigureAwait(false);

            var customApi = customApiResponse?.Value?.FirstOrDefault();
            if (customApi == null) return null!;

            var editor = await DocumentEditor.CreateAsync(document);

            // Replace attribute with expanded version
            UpdateAttribute(editor, classNode, attributeNode, customApi);

            // Generate nested Request class
            editor.AddMember(classNode, GenerateRequestClass(classNode.Identifier.Text, customApi.RequestParameters, nullableEnabled));
            editor.AddMember(classNode, GenerateResponseClass(classNode.Identifier.Text, customApi.ResponseProperties, nullableEnabled));

            // Get the updated syntax root
            var updatedRoot = editor.GetChangedRoot();

            // Return only the updated class declaration
            var updatedClassNode = updatedRoot.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == classNode.Identifier.Text);

            return updatedClassNode?.NormalizeWhitespace()?.ToFullString()!;
        }
        catch
        {
            return null!;
        }
    }

    private void UpdateAttribute(DocumentEditor editor, ClassDeclarationSyntax classNode, AttributeSyntax attributeNode, CustomApi customApi)
    {
        var updatedAttributeSyntax = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName(nameof(CustomApiAttribute)),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SeparatedList(
                    new[] { SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(customApi.UniqueName))) }
                    .Concat(GetNamedArguments(customApi))
                )
            )
        );
        editor.ReplaceNode(attributeNode, updatedAttributeSyntax);
    }

    private ClassDeclarationSyntax GenerateRequestClass(string className, IEnumerable<CustomApiRequestParameter> parameters, bool nullableEnabled)
    {
        return SyntaxFactory.ClassDeclaration($"{className}Request")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(parameters.Select(p => GenerateProperty(p.UniqueName!, p.Type, p.IsOptional, p.DisplayName, p.Description, nullableEnabled, "CustomApiRequestParameter", true)).ToArray());
    }

    private ClassDeclarationSyntax GenerateResponseClass(string className, IEnumerable<CustomApiResponseProperty> properties, bool nullableEnabled)
    {
        return SyntaxFactory.ClassDeclaration($"{className}Response")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(properties.Select(p => GenerateProperty(p.UniqueName!, p.Type, true, p.DisplayName, p.Description, nullableEnabled, "CustomApiResponseProperty", false)).ToArray());
    }

    private PropertyDeclarationSyntax GenerateProperty(string name, CustomApiFieldType type, bool isOptional, bool nullableEnabled)
    {
        var propertyType = MapType(type, nullableEnabled && isOptional);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(propertyType), name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        if (isOptional && !nullableEnabled)
        {
            propertyDeclaration = propertyDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("RequestParameter"),
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsOptional"), null,
                                        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))))))));
        }

        return propertyDeclaration;
    }

    private PropertyDeclarationSyntax GenerateProperty(string originalName, CustomApiFieldType type, bool isOptional, bool nullableEnabled, string attributeType)
    {
        var propertyName = char.IsLower(originalName[0]) ? char.ToUpper(originalName[0]) + originalName.Substring(1) : originalName;
        var propertyType = MapType(type, nullableEnabled && isOptional);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(propertyType), propertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        var needsUniqueNameAttribute = propertyName != originalName;

        if (isOptional && !nullableEnabled)
        {
            var args = new List<AttributeArgumentSyntax>
            {
                SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsOptional"), null, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
            };

            if (needsUniqueNameAttribute)
            {
                args.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("UniqueName"), null,
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(originalName))));
            }

            propertyDeclaration = propertyDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(args))))));
        }
        else if (needsUniqueNameAttribute)
        {
            var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName(attributeType),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("UniqueName"), null,
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(originalName))))));

            propertyDeclaration = propertyDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
        }

        return propertyDeclaration;
    }

    private PropertyDeclarationSyntax GenerateProperty(
        string originalName, 
        CustomApiFieldType type, 
        bool isOptional,
        string? displayName,
        string? description,
        bool nullableEnabled, 
        string attributeType, 
        bool canHaveIsOptional)
    {
        var propertyName = char.IsLower(originalName[0]) ? char.ToUpper(originalName[0]) + originalName.Substring(1) : originalName;
        var propertyType = MapType(type, nullableEnabled && isOptional);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(propertyType), propertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        var needsUniqueNameAttribute = propertyName != originalName;
        var attributeArguments = new List<AttributeArgumentSyntax>();

        if (needsUniqueNameAttribute)
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("UniqueName"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(originalName))));
        }

        if (canHaveIsOptional && isOptional && !nullableEnabled)
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsOptional"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        if (!string.IsNullOrWhiteSpace(displayName) && displayName != propertyName)
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("DisplayName"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(displayName!))));
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("Description"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(description!))));
        }

        if (attributeArguments.Any())
        {
            var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName(attributeType),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attributeArguments)));

            propertyDeclaration = propertyDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
        }

        return propertyDeclaration;
    }

    private IEnumerable<AttributeArgumentSyntax> GetNamedArguments(CustomApi api)
    {
        var args = new List<AttributeArgumentSyntax>();

        void AddArg(string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                args.Add(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals(name), null,
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value))));
            }
        }

        AddArg("Name", api.Name);
        AddArg("DisplayName", api.DisplayName);
        AddArg("Description", api.Description);

        if (api.StepType != CustomApi.ProcessingStepTypes.None)
        {
            args.Add(SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("StepType"), null,
                SyntaxFactory.ParseExpression($"{nameof(CustomApi.ProcessingStepTypes)}.{api.StepType}")));
        }

        if (api.BindingType != CustomApi.BindingTypes.Global)
        {
            args.Add(SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("BindingType"), null,
                SyntaxFactory.ParseExpression($"{nameof(CustomApi.BindingTypes)}.{api.BindingType}")));
        }

        AddArg("BoundEntityLogicalName", api.BoundEntityLogicalName);
        AddArg("ExecutePrivilegeName", api.ExecutePrivilegeName);

        args.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsFunction"), null,
            api.IsFunction ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));
        args.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsPrivate"), null,
            api.IsPrivate ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

        return args;
    }
}

#nullable restore