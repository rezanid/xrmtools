namespace XrmTools.Tests.Xrm.Generators;

using System.IO;
using FluentAssertions;
using Scriban;
using Scriban.Runtime;
using XrmTools.Xrm.Generators;
using Xunit;

public class EntityReferenceLookupTemplateTests
{
    [Theory]
    [InlineData("Lookup.sbncs")]
    [InlineData("Customer.sbncs")]
    public void Render_AllowsNullAndRetainsTargetValidation(string templateName)
    {
        var templatePath = GetTemplatePath(templateName);
        var attribute = new ScriptObject
        {
            ["schema_name"] = "LookupId",
            ["logical_name"] = "lookupid",
            ["is_valid_for_create"] = true,
            ["is_valid_for_update"] = true,
            ["is_valid_for_read"] = true,
            ["targets"] = new[] { "account", "contact" }
        };
        var globals = new ScriptObject
        {
            ["attribute"] = attribute,
            ["parent_type_name"] = "SampleEntity",
            ["force_readonly"] = false
        };
        var context = new TemplateContext
        {
            TemplateLoader = new FileTemplateLoader(Path.GetDirectoryName(templatePath))
        };
        context.PushGlobal(globals);

        var template = Template.Parse(File.ReadAllText(templatePath), templatePath);
        template.HasErrors.Should().BeFalse(string.Join("\n", template.Messages));
        var output = template.Render(context);

        output.Should().Contain("if (value != null &&");
        output.Should().Contain("!SampleEntity.Meta.Fields.LookupIdTargets.Contains(value.LogicalName)");
        output.Should().Contain("throw new InvalidPluginExecutionException");
        output.Should().Contain("this[\"lookupid\"] = value;");
    }

    private static string GetTemplatePath(string fileName)
    {
        for (var directory = new DirectoryInfo(System.AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "XrmTools", "CodeGenTemplates", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException($"Could not locate {fileName} from the test output directory.");
    }
}
