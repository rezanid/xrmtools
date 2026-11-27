namespace XrmTools.Tests.OData;

using System;
using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using XrmTools.OData;
using Xunit;

public class ODataQuickInfoContentTests
{
    [Fact]
    public void UsesNativeIdentifierStringAndNaturalLanguageRunsWithoutCustomFonts()
    {
        var content = ODataQuickInfoSource.CreateContent(new ODataInspection("Document variable id", "{{$guid}}")
        {
            Description = "Depends on $guid; evaluated when sending."
        });
        Assert.Equal(ContainerElementStyle.Stacked, content.Style);
        var rows = content.Elements.Cast<ClassifiedTextElement>().ToArray();
        Assert.Equal(3, rows.Length);
        var label = Assert.Single(rows[0].Runs);
        Assert.Equal(PredefinedClassificationTypeNames.Identifier, label.ClassificationTypeName);
        Assert.Equal(ClassifiedTextRunStyle.Bold, label.Style);
        Assert.Equal("Document variable id", label.Text);
        var value = Assert.Single(rows[1].Runs);
        Assert.Equal(PredefinedClassificationTypeNames.String, value.ClassificationTypeName);
        Assert.Equal(ClassifiedTextRunStyle.Plain, value.Style);
        Assert.Equal("{{$guid}}", value.Text);
        var description = Assert.Single(rows[2].Runs);
        Assert.Equal(PredefinedClassificationTypeNames.NaturalLanguage, description.ClassificationTypeName);
        Assert.Equal(ClassifiedTextRunStyle.Plain, description.Style);
        Assert.Equal(Environment.NewLine + "Depends on $guid; evaluated when sending.", description.Text);
    }

    [Fact]
    public void RendersUnavailableValuesAsProseWithoutAnEmptyDescriptionRow()
    {
        var content = ODataQuickInfoSource.CreateContent(new ODataInspection("Named request response r", "No valid capture.") { IsValue = false });
        var rows = content.Elements.Cast<ClassifiedTextElement>().ToArray();
        Assert.Equal(2, rows.Length);
        var notice = Assert.Single(rows[1].Runs);
        Assert.Equal(PredefinedClassificationTypeNames.NaturalLanguage, notice.ClassificationTypeName);
        Assert.Equal("No valid capture.", notice.Text);
    }
}
