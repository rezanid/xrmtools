#nullable enable
namespace XrmTools.CodeRefactoringProviders;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;
using XrmTools.WebApi;
using Microsoft.CodeAnalysis.CSharp;
using XrmTools.WebApi.Entities;
using System.Collections.Generic;
using CustomApiFieldType = WebApi.Types.CustomApiFieldType;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CustomApiRefactoringProvider)), Shared]
public class CustomApiRefactoringProvider : CodeRefactoringProvider
{
    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    public bool IsNullableEnabled { get; set; }

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var node = root.FindNode(context.Span);

        var attribute = node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attribute == null) return;
        if (attribute.ArgumentList == null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null) return;

        var symbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
        if (symbol?.ContainingType.Name != nameof(CustomApiAttribute)) return;

        var classNode = attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classNode == null) return;

        var firstArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument == null) return;

        var customApiName = semanticModel.GetConstantValue(firstArgument.Expression).Value as string;
        if (string.IsNullOrWhiteSpace(customApiName)) return;

        IsNullableEnabled = semanticModel.GetNullableContext(classNode.SpanStart).AnnotationsEnabled();

        // Register basic action
        var basicAction = CodeAction.Create(
            "Expand [CustomApi] attribute",
            ct => ApplyBasicFixAsync(context.Document, classNode, attribute, customApiName!, ct),
            equivalenceKey: "FixCustomApi");

        // Register full expansion action
        var fullAction = CodeAction.Create(
            "Expand [CustomApi] and add Request/Response nested classes",
            ct => ApplyFullFixAsync(context.Document, classNode, attribute, customApiName!, ct),
            equivalenceKey: "FixCustomApiWithRequestResponse");

        context.RegisterRefactoring(basicAction);
        context.RegisterRefactoring(fullAction);
    }

    private async Task<Document> ApplyBasicFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        AttributeSyntax attributeNode,
        string customApiName,
        CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        UpdateAttribute(editor, classNode, attributeNode, customApi);
        return editor.GetChangedDocument();
    }

    private async Task<Document> ApplyFullFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        AttributeSyntax attributeNode,
        string customApiName,
        CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        UpdateAttribute(editor, classNode, attributeNode, customApi);
        editor.AddMember(classNode, GenerateRequestClass(classNode.Identifier.Text, customApi.RequestParameters));
        editor.AddMember(classNode, GenerateResponseClass(classNode.Identifier.Text, customApi.ResponseProperties));

        return editor.GetChangedDocument();
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

    private ClassDeclarationSyntax GenerateRequestClass(string className, IEnumerable<CustomApiRequestParameter> parameters)
        => SyntaxFactory.ClassDeclaration($"{className}Request")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(nameof(CustomApiRequestAttribute)))
                    )
                )
            )
            .AddMembers([.. parameters.Select(GenerateProperty)]);

    private ClassDeclarationSyntax GenerateResponseClass(string className, IEnumerable<CustomApiResponseProperty> properties)
        => SyntaxFactory.ClassDeclaration($"{className}Response")
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddAttributeLists(
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(nameof(CustomApiResponseAttribute)))
                )
            )
        )
        .AddMembers([.. properties.Select(GenerateProperty)]);

    private PropertyDeclarationSyntax GenerateProperty(
        CustomApiRequestParameter parameter) => GenerateProperty(parameter.UniqueName!, parameter.Type, parameter.LogicalEntityName, parameter.IsOptional, parameter.DisplayName, parameter.Description, nameof(CustomApiRequestParameter), true);

    private PropertyDeclarationSyntax GenerateProperty(
        CustomApiResponseProperty property) => GenerateProperty(property.UniqueName!, property.Type, property.LogicalEntityName, true, property.DisplayName, property.Description, nameof(CustomApiResponseProperty), false);

    private PropertyDeclarationSyntax GenerateProperty(
        string originalName,
        CustomApiFieldType type,
        string? entityLogicalName,
        bool isOptional,
        string? displayName,
        string? description,
        string attributeType,
        bool canHaveIsOptional)
    {
        var propertyName = char.IsLower(originalName[0]) ? char.ToUpper(originalName[0]) + originalName.Substring(1) : originalName;
        var propertyType = MapType(type, IsNullableEnabled && isOptional);

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

        if (canHaveIsOptional && isOptional && !IsNullableEnabled)
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("IsOptional"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        if (!string.IsNullOrWhiteSpace(entityLogicalName))
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("LogicalEntityName"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(entityLogicalName!))));
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
}
#nullable restore