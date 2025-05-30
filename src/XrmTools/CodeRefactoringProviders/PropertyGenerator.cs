namespace XrmTools.CodeRefactoringProviders;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

public static class PropertyGenerator
{
    public static PropertyDeclarationSyntax GenerateProperty(string propertyName, string typeName)
        => GenerateProperty(propertyName, typeName, false);

    public static PropertyDeclarationSyntax GenerateProperty(string propertyName, string typeName, bool isNullable)
    {
        var parsedType = TypeNameParser.Parse(typeName);
        var typeSyntax = CreateTypeSyntax(parsedType, isNullable);

        return SyntaxFactory.PropertyDeclaration(typeSyntax, propertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
    }

    private static TypeSyntax CreateTypeSyntax(ParsedType type)
    {
        if (type.GenericArguments.Count > 0)
        {
            var genericName = SyntaxFactory.GenericName(SyntaxFactory.Identifier(type.Name))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(
                            type.GenericArguments.Select(CreateTypeSyntax))));

            return SyntaxFactory.QualifiedName(
                SyntaxFactory.ParseName(type.Namespace),
                genericName);
        }

        return SyntaxFactory.ParseTypeName($"{type.Namespace}.{type.Name}");
    }

    private static TypeSyntax CreateTypeSyntax(ParsedType type, bool isNullable)
    {
        TypeSyntax baseType;

        if (type.GenericArguments.Count > 0)
        {
            var genericName = SyntaxFactory.GenericName(SyntaxFactory.Identifier(type.Name))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(
                            type.GenericArguments.Select(arg => CreateTypeSyntax(arg, false)))));

            baseType = SyntaxFactory.QualifiedName(
                SyntaxFactory.ParseName(type.Namespace),
                genericName);
        }
        else
        {
            baseType = SyntaxFactory.ParseTypeName($"{type.Namespace}.{type.Name}");
        }

        if (isNullable)
        {
            return SyntaxFactory.NullableType(baseType);
        }

        return baseType;
    }
}