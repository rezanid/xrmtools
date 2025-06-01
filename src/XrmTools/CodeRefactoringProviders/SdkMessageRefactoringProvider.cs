#nullable enable
namespace XrmTools.CodeRefactoringProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;
using XrmTools.Xrm;
using XrmTools.Xrm.Repositories;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SdkMessageRefactoringProvider)), Shared]
public class SdkMessageRefactoringProvider : CodeRefactoringProvider
{
    private const string DataContractNamespace = "http://schemas.microsoft.com/xrm/2011/new/";

    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; } = null!;

    private ISdkMessageRepository sdkMessageRepository = null!;

    private bool isNullableEnabled;
    private SemanticModel? semanticModel;

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
        var isRequestProxy = symbol?.ContainingType.Name == "RequestProxyAttribute";
        var isResponseProxy = symbol?.ContainingType.Name == "ResponseProxyAttribute";
        if (!isRequestProxy && !isResponseProxy) return;

        var classNode = attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classNode == null) return;
        if (classNode.Parent is ClassDeclarationSyntax) return;

        var firstArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument == null) return;

        var messageName = semanticModel.GetConstantValue(firstArgument.Expression).Value as string;
        if (string.IsNullOrWhiteSpace(messageName)) return;

        isNullableEnabled = semanticModel.GetNullableContext(classNode.SpanStart).AnnotationsEnabled();

        if (isRequestProxy)
        {
            var OrgRequestAction = CodeAction.Create(
                "Fix request based on Dataverse metadata",
                ct => ApplyRequestProxyFixAsync(context.Document, classNode, messageName!, ct),
                equivalenceKey: "FixRequestProxy");
            context.RegisterRefactoring(OrgRequestAction);
        }

        if (isResponseProxy)
        {
            var OrgResponseAction = CodeAction.Create(
                "Fix reponse based on Dataverse metadata",
                ct => ApplyResponseProxyFixAsync(context.Document, classNode, messageName!, ct),
                equivalenceKey: "FixResponseProxy");
            context.RegisterRefactoring(OrgResponseAction);
        }

        sdkMessageRepository = await RepositoryFactory.CreateRepositoryAsync<ISdkMessageRepository>().ConfigureAwait(false);
    }

    private async Task<Document> ApplyRequestProxyFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        string messageName,
        CancellationToken cancellationToken)
    {
        var message = await sdkMessageRepository.GetByNameWithDescendantsAsync(messageName, cancellationToken).ConfigureAwait(false);
        if (message is null || semanticModel is null) return document;

        var request = message.Pairs
            .SelectMany(pair => pair.Request)
            .FirstOrDefault(req => req.Name == messageName);
        if (request is null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var updatedClass = classNode;

        bool needsDataContract = !HasAttribute(semanticModel, classNode.AttributeLists, "System.Runtime.Serialization.DataContractAttribute", cancellationToken);
        bool needsInheritance = !InheritsFrom(classNode, semanticModel, semanticModel.Compilation, "Microsoft.Xrm.Sdk.OrganizationRequest", cancellationToken);

        var typeCache = new Dictionary<string, ITypeSymbol>();
        var existingProps = updatedClass.Members.OfType<PropertyDeclarationSyntax>().ToDictionary(p => p.Identifier.Text, StringComparer.OrdinalIgnoreCase);
        var newProps = new List<MemberDeclarationSyntax>();

        foreach (var field in request.Fields.OrderBy(f => f.Position))
        {
            if (string.IsNullOrWhiteSpace(field.Name) || string.IsNullOrWhiteSpace(field.ClrParser))
                continue;

            var propName = ToPascalCase(field.Name!);
            // Strip assembly from the name
            var clrTypeKey = field.ClrParser!.Split(',')[0].Trim();

            if (!typeCache.TryGetValue(clrTypeKey, out var typeSymbol))
            {
                typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(clrTypeKey);
                if (typeSymbol != null)
                    typeCache[clrTypeKey] = typeSymbol;
            }

            if (typeSymbol == null)
                continue;

            bool isReferenceType = typeSymbol.IsReferenceType;
            bool useNullable = field.Optional == true && (!isReferenceType || isNullableEnabled);
            var applyNullForgiving = !useNullable && isReferenceType;
            bool isRequired = isNullableEnabled && field.Optional != true && typeSymbol.IsReferenceType;

            var typeSyntax = CreateTypeSyntax(editor, typeSymbol, useNullable);

            if (!existingProps.TryGetValue(propName, out var existing))
            {
                newProps.Add(CreateRequestProperty(propName, typeSyntax, applyNullForgiving, isRequired));
            }
            else if (!PropertyMatches(existing, typeSymbol, semanticModel))
            {
                updatedClass = updatedClass.ReplaceNode(existing, CreateRequestProperty(propName, typeSyntax, applyNullForgiving, isRequired));
            }
        }

        if (newProps.Count > 0)
            updatedClass = InsertAfterLastProperty(updatedClass, newProps);

        if (needsInheritance)
        {
            updatedClass = AddBaseType(
                updatedClass,
                "Microsoft.Xrm.Sdk.OrganizationRequest",
                editor);
        }

        if (needsDataContract)
        {
            updatedClass = updatedClass.AddAttributeLists(CreateDataContractAttribute(semanticModel, editor.Generator));
        }

        editor.ReplaceNode(classNode, updatedClass);
        return editor.GetChangedDocument();
    }

    private static TypeSyntax CreateTypeSyntax(DocumentEditor editor, ITypeSymbol typeSymbol, bool useNullableAnnotation)
    {
        var typeSyntax = (TypeSyntax)editor.Generator.TypeExpression(typeSymbol);

        if (useNullableAnnotation && (typeSymbol.IsReferenceType || typeSymbol.IsValueType))
        {
            return SyntaxFactory.NullableType(typeSyntax);
        }

        return typeSyntax;
    }

    private static AttributeListSyntax CreateDataContractAttribute(SemanticModel semanticModel, SyntaxGenerator generator)
    {
        var dataContractSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.DataContractAttribute");
        return SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute((NameSyntax)generator.TypeExpression(dataContractSymbol))
                    .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("Namespace"),
                            null,
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(DataContractNamespace))
                        )
                    )))
            )
        );
    }

    private static ClassDeclarationSyntax InsertAfterLastProperty(
        ClassDeclarationSyntax classNode,
        IEnumerable<MemberDeclarationSyntax> newProperties)
    {
        var members = classNode.Members;
        var lastPropIndex = members
            .Select((member, index) => (member, index))
            .Where(tuple => tuple.member is PropertyDeclarationSyntax)
            .Select(tuple => tuple.index)
            .LastOrDefault();

        var updatedMembers = lastPropIndex > 0 ? members.InsertRange(lastPropIndex + 1, newProperties) : members.AddRange(newProperties);
        return classNode.WithMembers(updatedMembers);
    }

    private static PropertyDeclarationSyntax CreateRequestPropertyOld(string name, TypeSyntax typeSyntax, bool applyNullForgiving)
    {
        var variableName = "value_" + name.ToLowerInvariant();

        return SyntaxFactory.PropertyDeclaration(typeSyntax, name)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(
                SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.ConditionalExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Parameters"),
                                    SyntaxFactory.IdentifierName("TryGetValue")),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(name))),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.DeclarationExpression(
                                            typeSyntax,
                                            SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(variableName))))
                                            .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                                }))),
                            SyntaxFactory.IdentifierName(variableName),
                            applyNullForgiving
                                ? SyntaxFactory.PostfixUnaryExpression(
                                    SyntaxKind.SuppressNullableWarningExpression,
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword)))
                                : SyntaxFactory.LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                        )
                    ))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),

                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.ElementAccessExpression(
                                SyntaxFactory.IdentifierName("Parameters"),
                                SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(name)))
                                ))),
                            SyntaxFactory.IdentifierName("value"))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                })));
    }

    private static PropertyDeclarationSyntax CreateRequestProperty(string name, TypeSyntax typeSyntax, bool applyNullForgiving, bool isRequired)
    {
        var variableName = "value_" + name.ToLowerInvariant();

        var modifiers = new List<SyntaxToken> { SyntaxFactory.Token(SyntaxKind.PublicKeyword) };
        if (isRequired)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.RequiredKeyword));
        }

        return SyntaxFactory.PropertyDeclaration(typeSyntax, name)
            .WithModifiers(SyntaxFactory.TokenList(modifiers))
            .WithAccessorList(
                SyntaxFactory.AccessorList(SyntaxFactory.List(
                [
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.ConditionalExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Parameters"),
                                    SyntaxFactory.IdentifierName("TryGetValue")),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                                [
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(name))),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.DeclarationExpression(
                                            typeSyntax,
                                            SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(variableName))))
                                            .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                                ]))),
                            SyntaxFactory.IdentifierName(variableName),
                            applyNullForgiving
                                ? SyntaxFactory.PostfixUnaryExpression(
                                    SyntaxKind.SuppressNullableWarningExpression,
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword)))
                                : SyntaxFactory.LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                        )
                    ))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),

                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.ElementAccessExpression(
                                SyntaxFactory.IdentifierName("Parameters"),
                                SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(name)))
                                ))),
                            SyntaxFactory.IdentifierName("value"))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                ])));
    }

    private static bool PropertyMatches(
        PropertyDeclarationSyntax existingProperty,
        ITypeSymbol expectedSymbol,
        SemanticModel semanticModel)
    {
        var actualTypeSymbol = semanticModel.GetTypeInfo(existingProperty.Type).Type;
        return SymbolEqualityComparer.Default.Equals(expectedSymbol, actualTypeSymbol);
    }

    private static string ToPascalCase(string input)
    {
        return string.IsNullOrEmpty(input) ? input : char.ToUpperInvariant(input[0]) + input[1..];
    }

    private static bool HasAttribute(
        SemanticModel semanticModel,
        SyntaxList<AttributeListSyntax> attributeLists,
        string expectedAttributeMetadataName,
        CancellationToken cancellationToken)
    {
        var expectedAttrSymbol = semanticModel.Compilation.GetTypeByMetadataName(expectedAttributeMetadataName);
        if (expectedAttrSymbol == null)
            return false;

        return attributeLists
            .SelectMany(al => al.Attributes)
            .Select(attr => semanticModel.GetTypeInfo(attr, cancellationToken).Type)
            .Any(attrType => SymbolEqualityComparer.Default.Equals(attrType, expectedAttrSymbol));
    }

    private async Task<Document> ApplyResponseProxyFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        string messageName,
        CancellationToken cancellationToken)
    {
        var message = await sdkMessageRepository.GetByNameWithDescendantsAsync(messageName, cancellationToken);
        if (message is null || semanticModel is null) return document;

        var response = message.Pairs
            .SelectMany(pair => pair.Request)
            .FirstOrDefault(req => req.Name == messageName)?
            .Response
            .FirstOrDefault();

        if (response is null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var updatedClass = classNode;

        bool needsDataContract = !HasAttribute(semanticModel, classNode.AttributeLists, "System.Runtime.Serialization.DataContractAttribute", cancellationToken);
        bool needsInheritance = !InheritsFrom(classNode, semanticModel, semanticModel.Compilation, "Microsoft.Xrm.Sdk.OrganizationResponse", cancellationToken);

        var typeCache = new Dictionary<string, ITypeSymbol>();
        var existingProps = updatedClass.Members.OfType<PropertyDeclarationSyntax>().ToDictionary(p => p.Identifier.Text, StringComparer.OrdinalIgnoreCase);
        var newProps = new List<MemberDeclarationSyntax>();

        foreach (var field in response.Fields.OrderBy(f => f.Position))
        {
            if (string.IsNullOrWhiteSpace(field.Name) || string.IsNullOrWhiteSpace(field.ClrFormatter))
                continue;

            var propName = ToPascalCase(field.Name!);
            // Strip assembly from the name
            var clrTypeKey = field.ClrFormatter!.Split(',')[0].Trim();

            if (!typeCache.TryGetValue(clrTypeKey, out var typeSymbol))
            {
                typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(clrTypeKey);
                if (typeSymbol != null)
                    typeCache[clrTypeKey] = typeSymbol;
            }

            if (typeSymbol == null)
                continue;

            bool useNullable = !typeSymbol.IsReferenceType || isNullableEnabled;

            var typeSyntax = CreateTypeSyntax(editor, typeSymbol, useNullable);

            if (!existingProps.TryGetValue(propName, out var existing))
            {
                newProps.Add(CreateResponseProperty(propName, typeSyntax));
            }
            else if (!PropertyMatches(existing, typeSymbol, semanticModel))
            {
                updatedClass = updatedClass.ReplaceNode(existing, CreateResponseProperty(propName, typeSyntax));
            }
        }

        if (newProps.Count > 0)
            updatedClass = InsertAfterLastProperty(updatedClass, newProps);

        if (needsInheritance)
        {
            updatedClass = AddBaseType(
                updatedClass,
                "Microsoft.Xrm.Sdk.OrganizationResponse",
                editor);
        }

        if (needsDataContract)
        {
            updatedClass = updatedClass.AddAttributeLists(CreateDataContractAttribute(semanticModel, editor.Generator));
        }

        editor.ReplaceNode(classNode, updatedClass);
        return editor.GetChangedDocument();
    }

    private static ClassDeclarationSyntax AddBaseType(
        ClassDeclarationSyntax classNode,
        string baseTypeMetadataName,
        DocumentEditor editor)
    {
        var compilation = editor.SemanticModel.Compilation;
        var baseTypeSymbol = compilation.GetTypeByMetadataName(baseTypeMetadataName);
        if (baseTypeSymbol == null)
            return classNode;

        var typeSyntax = (TypeSyntax)editor.Generator.TypeExpression(baseTypeSymbol);
        var newBase = SyntaxFactory.SimpleBaseType(typeSyntax);

        var updatedBaseList = classNode.BaseList is null
            ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(newBase))
            : classNode.BaseList.AddTypes(newBase);

        return classNode.WithBaseList(updatedBaseList);
    }

    private static bool InheritsFrom(
        ClassDeclarationSyntax classNode,
        SemanticModel semanticModel,
        Compilation compilation,
        string expectedBaseTypeMetadataName,
        CancellationToken cancellationToken)
    {
        var expectedBaseTypeSymbol = compilation.GetTypeByMetadataName(expectedBaseTypeMetadataName);
        if (expectedBaseTypeSymbol == null)
            return false;

        return classNode.BaseList?.Types
            .Select(bt => semanticModel.GetTypeInfo(bt.Type, cancellationToken).Type)
            .Any(t => SymbolEqualityComparer.Default.Equals(t, expectedBaseTypeSymbol)) == true;
    }

    private static PropertyDeclarationSyntax CreateResponseProperty(string name, TypeSyntax typeSyntax)
    {
        var variableName = "value_" + name.ToLowerInvariant();

        return SyntaxFactory.PropertyDeclaration(typeSyntax, name)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                SyntaxFactory.ConditionalExpression(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Results"),
                            SyntaxFactory.IdentifierName("TryGetValue")),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name))),
                        SyntaxFactory.Argument(SyntaxFactory.DeclarationExpression(
                            typeSyntax,
                            SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(variableName))))
                            .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                        }))),
                    SyntaxFactory.IdentifierName(variableName),
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}
#nullable restore