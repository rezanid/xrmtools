#nullable enable
namespace XrmTools.CodeRefactoringProviders;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using System.Collections.Generic;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(PluginRefactoringProvider)), Shared]
public class PluginRefactoringProvider : CodeRefactoringProvider
{
    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var node = root.FindNode(context.Span);

        var attribute = (node is AttributeListSyntax attrList && attrList.Attributes.Count == 1) ?
            attrList.Attributes[0] :
            node.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attribute == null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null) return;

        var symbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
        if (symbol?.ContainingType.Name != nameof(PluginAttribute)) return;

        var classNode = attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classNode == null) return;
        
        var className = semanticModel.GetDeclaredSymbol(classNode).ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);// classNode.Identifier.Text;

        // TODO: Possible error: "No environment selected." 
        var pluginType = await WebApiService.GetPluginDefinitionAsync(className);
        if (pluginType == null) return;

        var action = CodeAction.Create(
            "Expand [Plugin] attribute and add [Step] and [Image] attributes",
            ct => ApplyPluginFixAsync(context.Document, classNode, attribute, pluginType, ct),
            equivalenceKey: "FixPluginAttribute");

        context.RegisterRefactoring(action);
    }

    private async Task<Document> ApplyPluginFixAsync(
        Document document,
        ClassDeclarationSyntax classNode,
        AttributeSyntax attributeNode,
        PluginType pluginType,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var typeName = pluginType.TypeName ?? pluginType.Name ?? classNode.Identifier.Text;
        var name = pluginType.Name;
        var friendlyName = pluginType.FriendlyName;
        var description = pluginType.Description;

        var attributeArguments = new List<AttributeArgumentSyntax>();

        // Only add Name if different from typeName
        if (!string.IsNullOrWhiteSpace(name) && name != typeName)
        {
            attributeArguments.Add(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals("Name"), null,
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name))));
        }

        // Only add FriendlyName if different from typeName
        if (!string.IsNullOrWhiteSpace(friendlyName) && friendlyName != typeName)
        {
            attributeArguments.Add(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals("FriendlyName"), null,
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(friendlyName))));
        }

        // Only add Description if not empty
        if (!string.IsNullOrWhiteSpace(description))
        {
            attributeArguments.Add(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals("Description"), null,
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(description))));
        }

        var updatedPluginAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName(nameof(PluginAttribute)[..^9]),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SeparatedList(attributeArguments)
            )
        );
        editor.ReplaceNode(attributeNode, updatedPluginAttribute);

        // Add StepAttributes
        foreach (var step in pluginType.Steps)
        {
            // Get metadata for constructor selection
            var messageName = step.Message?.Name ?? step.Name ?? "";
            var primaryEntityName = step.SdkMessageFilter?.PrimaryObjectTypeCode;
            var filteringAttributes = step.FilteringAttributes;
            var stage = step.Stage ?? Stages.PreOperation; // Default to PreOperation if null
            var mode = (ExecutionMode)step.Mode; // Cast to enum

            AttributeArgumentSyntax[] positionalArgs;

            if (!string.IsNullOrWhiteSpace(primaryEntityName) && !string.IsNullOrWhiteSpace(filteringAttributes))
            {
                // Use StepAttribute(string messageName, string primaryEntityName, string filteringAttributes, Stages stage, ExecutionMode mode)
                positionalArgs = new[]
                {
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(messageName))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(primaryEntityName))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(filteringAttributes))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(Stages)}.{stage}")),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(ExecutionMode)}.{mode}"))
                };
            }
            else if (!string.IsNullOrWhiteSpace(primaryEntityName))
            {
                // Use StepAttribute(string messageName, string primaryEntityName, Stages stage, ExecutionMode mode)
                positionalArgs = new[]
                {
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(messageName))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(primaryEntityName))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(Stages)}.{stage}")),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(ExecutionMode)}.{mode}"))
                };
            }
            else
            {
                // Use StepAttribute(string messageName, Stages stage, ExecutionMode mode)
                positionalArgs = new[]
                {
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(messageName))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(Stages)}.{stage}")),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"{nameof(ExecutionMode)}.{mode}"))
                };
            }

            // Add named arguments as needed (e.g., Description, Configuration, etc.)
            var namedArgs = new List<AttributeArgumentSyntax>();
            if (!string.IsNullOrWhiteSpace(step.Description))
            {
                namedArgs.Add(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals("Description"), null,
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(step.Description))));
            }

            // Add other named arguments as needed...
            var stepAttribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName(nameof(StepAttribute)[..^9]),
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(positionalArgs.Concat(namedArgs))
                )
            );
            editor.AddAttribute(classNode, stepAttribute);

            // Add ImageAttributes for each step
            foreach (var image in step.Images)
            {
                var imageTypeValue = image.ImageType ?? 0;
                var imageType = (ImageTypes)imageTypeValue;
                var imageTypeName = imageType.ToString();
                var imgAttributes = image.Attributes;
                var imgEntityAlias = image.EntityAlias;
                var imgName = image.Name;
                var imgMessagePropertyName = image.MessagePropertyName;
                var imgDescription = image.Description;

                // Select best ImageAttribute constructor
                var imageArgs = new List<AttributeArgumentSyntax>
                {
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.ParseExpression($"{nameof(ImageTypes)}.{imageTypeName}"))
                };
                if (!string.IsNullOrWhiteSpace(imgAttributes))
                {
                    imageArgs.Add(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(imgAttributes))));
                }

                // Named arguments
                var imageNamedArgs = new List<AttributeArgumentSyntax>();
                if (!string.IsNullOrWhiteSpace(imgName) && imgName != imageTypeName)
                {
                    imageNamedArgs.Add(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("Name"), null,
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(imgName))));
                }
                if (!string.IsNullOrWhiteSpace(imgEntityAlias) && imgEntityAlias != imageTypeName)
                {
                    imageNamedArgs.Add(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("EntityAlias"), null,
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(imgEntityAlias))));
                }
                if (!string.IsNullOrWhiteSpace(imgMessagePropertyName) && imgMessagePropertyName != "Target")
                {
                    imageNamedArgs.Add(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("MessagePropertyName"), null,
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(imgMessagePropertyName))));
                }
                if (!string.IsNullOrWhiteSpace(imgDescription))
                {
                    imageNamedArgs.Add(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("Description"), null,
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(imgDescription))));
                }

                var imageAttribute = SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName(nameof(ImageAttribute)[..^9]),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(imageArgs.Concat(imageNamedArgs))
                    )
                );
                editor.AddAttribute(classNode, imageAttribute);
            }
        }

        return editor.GetChangedDocument();
    }
}
#nullable restore