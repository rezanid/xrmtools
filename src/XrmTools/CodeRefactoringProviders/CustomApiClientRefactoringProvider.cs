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
using XrmTools.Extensions;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CustomApiRefactoringProvider)), Shared]
public class CustomApiClientRefactoringProvider : CodeRefactoringProvider
{
    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    private bool isNullableEnabled;
    private SemanticModel? semanticModel;
    private static readonly string[] parameterAttributeNames = [nameof(CustomApiRequestParameterAttribute), nameof(CustomApiResponsePropertyAttribute), nameof(CustomApiRequestParameter), nameof(CustomApiResponseProperty)];

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var node = root.FindNode(context.Span);

        var attribute = node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attribute?.ArgumentList == null) return;

        semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null) return;

        var symbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
        var isCustomApiRequest = symbol?.ContainingType.Name == nameof(CustomApiRequestAttribute);
        var isCustomApiResponse = symbol?.ContainingType.Name == nameof(CustomApiResponseAttribute);
        if (!isCustomApiRequest && !isCustomApiResponse) return;

        var classNode = attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classNode == null) return;
        if (classNode.Parent is ClassDeclarationSyntax) return;

        var firstArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument == null) return;

        var customApiName = semanticModel.GetConstantValue(firstArgument.Expression).Value as string;
        if (string.IsNullOrWhiteSpace(customApiName)) return;

        isNullableEnabled = semanticModel.GetNullableContext(classNode.SpanStart).AnnotationsEnabled();

        // Register CustomApiRequest fix action
        if (isCustomApiRequest)
        {
            var customApiRequestAction = CodeAction.Create(
                "Add missing request parameters",
                ct => ApplyCustomApiRequestFixAsync(context.Document, classNode, customApiName!, ct),
                equivalenceKey: "FixCustomApiRequestClient");
            context.RegisterRefactoring(customApiRequestAction);
        }

        // Register CustomApiResponse fix action
        if (isCustomApiResponse)
        {
            var customApiResponseAction = CodeAction.Create(
                "Add missing response properties",
                ct => ApplyCustomApiResponseFixAsync(context.Document, classNode, customApiName!, ct),
                equivalenceKey: "FixCustomApiResponseClient");
            context.RegisterRefactoring(customApiResponseAction);
        }
    }

    private async Task<Document> OriginalApplyCustomApiRequestFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        string customApiName,
        CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        foreach (var parameter in customApi.RequestParameters)
        {
            if (classNode.Members.FirstOrDefault(m => m is PropertyDeclarationSyntax p && p.Identifier.Text == parameter.UniqueName) is PropertyDeclarationSyntax propertyNode) continue;
            editor.AddMember(classNode, GenerateProperty(parameter));
        }

        return editor.GetChangedDocument();
    }

    private async Task<Document> OriginalApplyCustomApiResponseFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        string customApiName,
        CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        foreach (var parameter in customApi.ResponseProperties)
        {
            if (classNode.Members.FirstOrDefault(m => m is PropertyDeclarationSyntax p && p.Identifier.Text == parameter.UniqueName) is PropertyDeclarationSyntax propertyNode) continue;
            editor.AddMember(classNode, GenerateProperty(parameter));
        }

        return editor.GetChangedDocument();
    }

    private async Task<Document> ApplyCustomApiRequestFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        string customApiName,
        CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null || semanticModel == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        if (!ImplementsInterface(classNode, nameof(ITypedOrganizationRequest)))
        {
            editor.AddBaseType(classNode, SyntaxFactory.ParseTypeName(nameof(ITypedOrganizationRequest)));
        }

        if (!classNode.Members.OfType<PropertyDeclarationSyntax>().Any(p => p.Identifier.Text == "RequestName"))
        {
            var requestNameProperty = GenerateRequestNameProperty(customApiName);
            editor.AddMember(classNode, requestNameProperty);
        }

        var members = classNode.Members.OfType<PropertyDeclarationSyntax>().ToList();
        
        foreach (var parameter in customApi.RequestParameters)
        {
            var existing = members.FirstOrDefault(p =>
                p.Identifier.Text == parameter.UniqueName ||
                GetUniqueNameFromProperty(p) == parameter.UniqueName);

            var expectedType = MapType(parameter.Type, isNullableEnabled && parameter.IsOptional);

            if (existing != null)
            {
                //var currentType = semanticModel.GetTypeInfo(existing.Type, cancellationToken).Type?.ToDisplayString();
                var currentType = existing.Type.ToString();
                if (currentType != expectedType)
                {
                    var newProp = GenerateProperty(parameter);
                    editor.ReplaceNode(existing, newProp);
                    //editor.ReplaceNode(existing.Type, SyntaxFactory.ParseTypeName(expectedType));
                }
            }
            else
            {
                var newProp = GenerateProperty(parameter);
                var lastProp = members.LastOrDefault();
                if (lastProp != null)
                    editor.InsertAfter(lastProp, newProp);
                else
                    editor.AddMember(classNode, newProp);
            }
        }

        var intermediateDocument = editor.GetChangedDocument();
        var intermediateRoot = await intermediateDocument.GetSyntaxRootAsync(cancellationToken);
        var updatedClassNode = intermediateRoot?.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == classNode.Identifier.Text);
        if (updatedClassNode == null) return intermediateDocument;

        var method = GenerateToOrganizationRequestMethod(updatedClassNode);
        var existingMethod = FindMethod(classNode, "ToOrganizationRequest");

        var finalRoot = existingMethod != null
            ? intermediateRoot.ReplaceNode(existingMethod, method)
            : intermediateRoot.InsertNodesAfter(updatedClassNode.Members.Last(), [method]);

        return intermediateDocument.WithSyntaxRoot(finalRoot);

        //var method = GenerateToOrganizationRequestMethod(customApi.RequestParameters);
        //var method = GenerateToOrganizationRequestMethod(classNode);
        //editor.AddMember(classNode, SyntaxFactory.ParseMemberDeclaration(method)!);
        //editor.AddAttribute(classNode, GenerateToOrganizationRequestMethod(classNode));

        // ALL IN ONE CHANGE
        //var method = GenerateToOrganizationRequestMethod(classNode);
        //var existingMethod = FindMethod(classNode, "ToOrganizationRequest");
        //if (existingMethod != null)
        //    editor.ReplaceNode(existingMethod, method);
        //else
        //    editor.AddMember(classNode, method);

        //return editor.GetChangedDocument();
    }

    private async Task<Document> ApplyCustomApiResponseFixAsync(Document document, ClassDeclarationSyntax classNode, string customApiName, CancellationToken cancellationToken)
    {
        var customApi = await WebApiService.GetCustomApiDefinitionAsync(customApiName);
        if (customApi == null || semanticModel == null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        if (!ImplementsInterface(classNode, nameof(ITypedOrganizationResponse)))
        {
            editor.AddBaseType(classNode, SyntaxFactory.ParseTypeName(nameof(ITypedOrganizationResponse)));
        }

        var members = classNode.Members.OfType<PropertyDeclarationSyntax>().ToList();

        foreach (var property in customApi.ResponseProperties)
        {
            var existing = members.FirstOrDefault(p =>
                p.Identifier.Text == property.UniqueName ||
                GetUniqueNameFromProperty(p) == property.UniqueName);

            var expectedType = MapType(property.Type, isNullableEnabled);

            if (existing != null)
            {
                //var currentType = semanticModel.GetTypeInfo(existing.Type, cancellationToken).Type?.ToDisplayString();
                var currentType = existing.Type.ToString();
                if (currentType != expectedType)
                {
                    var newProp = GenerateProperty(property);
                    editor.ReplaceNode(existing, newProp);
                    //editor.ReplaceNode(existing.Type, SyntaxFactory.ParseTypeName(expectedType));
                }
            }
            else
            {
                var newProp = GenerateProperty(property);
                var lastProp = members.LastOrDefault();
                if (lastProp != null)
                    editor.InsertAfter(lastProp, newProp);
                else
                    editor.AddMember(classNode, newProp);
            }
        }

        var intermediateDocument = editor.GetChangedDocument();
        if (intermediateDocument == null) return document;

        var intermediateRoot = await intermediateDocument.GetSyntaxRootAsync(cancellationToken);
        if (intermediateRoot == null) return intermediateDocument;

        var updatedClassNode = intermediateRoot?.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == classNode.Identifier.Text);
        if (updatedClassNode == null) return intermediateDocument;

        var method = GenerateLoadFromResponseMethod(updatedClassNode);
        var existingMethod = FindMethod(classNode, nameof(ITypedOrganizationResponse.LoadFromOrganizationResponse), "OrganizationResponse");

        var finalRoot = existingMethod != null
            ? intermediateRoot!.ReplaceNode(existingMethod, method)
            : intermediateRoot!.InsertNodesAfter(updatedClassNode.Members.Last(), [method]);

        return intermediateDocument.WithSyntaxRoot(finalRoot!);

        //if (existingMethod != null)
        //    editor.ReplaceNode(existingMethod, method);
        //else
        //    editor.AddMember(classNode, method);

        //return editor.GetChangedDocument();
    }

    private PropertyDeclarationSyntax GenerateProperty(
        CustomApiRequestParameter parameter) => GenerateProperty(parameter.UniqueName!, parameter.Type, parameter.IsOptional, parameter.DisplayName, parameter.Description, nameof(CustomApiRequestParameter), false);

    private PropertyDeclarationSyntax GenerateProperty(
        CustomApiResponseProperty property) => GenerateProperty(property.UniqueName!, property.Type, true, property.DisplayName, property.Description, nameof(CustomApiResponseProperty), false);

    private PropertyDeclarationSyntax GenerateProperty(
        string originalName,
        CustomApiFieldType type,
        bool isOptional,
        string? displayName,
        string? description,
        string attributeType,
        bool canHaveIsOptional)
    {
        var propertyName = char.IsLower(originalName[0]) ? char.ToUpper(originalName[0]) + originalName.Substring(1) : originalName;
        var propertyType = MapType(type, isNullableEnabled && isOptional);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(propertyType), propertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        if (!isOptional && isNullableEnabled)
        {
            propertyDeclaration = propertyDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.RequiredKeyword));
        }

        var needsUniqueNameAttribute = propertyName != originalName;
        var attributeArguments = new List<AttributeArgumentSyntax>();

        if (needsUniqueNameAttribute)
        {
            attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("UniqueName"), null,
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(originalName))));
        }

        if (canHaveIsOptional && isOptional && !isNullableEnabled)
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

    //TODO: test if this works for constants too.
    private static string? GetUniqueNameFromProperty(PropertyDeclarationSyntax property)
    { 

        return property.AttributeLists
            .SelectMany(al => al.Attributes)
            .Where(a => parameterAttributeNames.Contains(a.Name.ToString()))
            .SelectMany(a => a.ArgumentList?.Arguments ?? default)
            .Where(arg => arg.NameEquals?.Name.Identifier.Text == nameof(CustomApiRequestParameterAttribute.UniqueName))
            .Select(arg => arg.Expression)
            .OfType<LiteralExpressionSyntax>()
            .Select(e => e.Token.ValueText)
            .FirstOrDefault();
    }

    //TODO: Move this to a utility class
    private static bool ImplementsInterface(ClassDeclarationSyntax classNode, string interfaceName)
        => classNode.BaseList?.Types.Any(t => t.Type.ToString() == interfaceName) == true;

    private static PropertyDeclarationSyntax GenerateRequestNameProperty(string customApiName)
        => SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                SyntaxFactory.Identifier("RequestName"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(customApiName))))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    //TODO: Another candidate to move to a utility class
    private static MethodDeclarationSyntax? FindMethod(
        ClassDeclarationSyntax classNode,
        string methodName,
        params string[] parameterTypeNames)
    {
        return classNode.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m =>
                m.Identifier.Text == methodName &&
                m.ParameterList.Parameters.Count == parameterTypeNames.Length &&
                m.ParameterList.Parameters.Select(p => p.Type?.ToString()).SequenceEqual(parameterTypeNames));
    }

    private static MethodDeclarationSyntax GenerateToOrganizationRequestMethod(ClassDeclarationSyntax classNode)
    {
        var mappings = ExtractRequestPropertyMappings(classNode);

        var statements = new List<StatementSyntax>
        {
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var "))
                .AddVariables(SyntaxFactory.VariableDeclarator("request")
                    .WithInitializer(SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("OrganizationRequest"))
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("RequestName")))))))))
        };

        statements.AddRange(mappings.Select(mapping =>
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("request"),
                            SyntaxFactory.IdentifierName("Parameters")),
                        SyntaxFactory.IdentifierName("AddOrUpdateIfNotNull")))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                    [
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(mapping.MetadataName))),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(mapping.PropertyName))
                    ]))))));

        statements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("request")));

        return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.IdentifierName("OrganizationRequest"),
                SyntaxFactory.Identifier("ToOrganizationRequest"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(statements));
    }

    private static MethodDeclarationSyntax GenerateLoadFromResponseMethod(ClassDeclarationSyntax classNode)
    {
        var mappings = ExtractRequestPropertyMappings(classNode);
        var statements = new List<StatementSyntax>();

        foreach (var (propertyName, metadataName, propTypeName) in mappings)
        {
            var valueVar = SyntaxFactory.IdentifierName("value");

            var assignment = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ConditionalExpression(
                        //SyntaxFactory.BinaryExpression(
                        //    SyntaxKind.LogicalAndExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("response"),
                                        SyntaxFactory.IdentifierName("Results")),
                                    SyntaxFactory.IdentifierName("TryGetValue")))
                            .WithArgumentList(SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(
                                [
                                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(metadataName))),
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"out {propTypeName} {ToCamel(metadataName)}"))
                                ]))),
                            //SyntaxFactory.IsPatternExpression(
                            //    valueVar,
                            //    SyntaxFactory.DeclarationPattern(
                            //        SyntaxFactory.IdentifierName("var"),
                            //        SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("result"))))
                            //),
                        //SyntaxFactory.CastExpression(
                        //    SyntaxFactory.IdentifierName(propertyName),
                            SyntaxFactory.IdentifierName(ToCamel(metadataName)),
                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))));

            statements.Add(assignment);
        }

        //statements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ThisExpression()));

        return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.IdentifierName("void"),
                SyntaxFactory.Identifier(nameof(ITypedOrganizationResponse.LoadFromOrganizationResponse)))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithParameterList(SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("response"))
                        .WithType(SyntaxFactory.IdentifierName("OrganizationResponse")))))
            .WithBody(SyntaxFactory.Block(statements));
    }

    private static IEnumerable<(string PropertyName, string MetadataName, string propertyTypeName)> ExtractRequestPropertyMappings(ClassDeclarationSyntax classNode)
    {
        foreach (var property in classNode.Members.OfType<PropertyDeclarationSyntax>())
        {
            var metadataName = GetUniqueNameFromProperty(property) ?? property.Identifier.Text;
            yield return (property.Identifier.Text, metadataName, property.Type.ToString());
        }
    }

    private static string ToPascal(string name) => string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name[1..];
    private static string ToCamel(string name) => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

}
#nullable restore