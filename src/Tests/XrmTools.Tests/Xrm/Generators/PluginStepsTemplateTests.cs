namespace XrmTools.Tests.Xrm.Generators;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Scriban;
using Scriban.Runtime;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Model.Configuration;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using XrmTools.Xrm.Generators;
using Xunit;

public class PluginStepsTemplateTests
{
    [Fact]
    public void Render_TwoStepsSameMessage_ProducesTwoFilteredEntitiesAndTwoTargets()
    {
        var output = RenderPlugin(BuildModelWithTwoUpdateSteps());

        // Two distinct input-parameter wrappers (disambiguated by stage).
        output.Should().Contain("class UpdatePreOperationInputParameters");
        output.Should().Contain("class UpdatePostOperationInputParameters");

        // Two distinct entity backing types.
        output.Should().Contain("class UpdatePreOperationMy_entityEntity");
        output.Should().Contain("class UpdatePostOperationMy_entityEntity");

        // Two Target properties, one per step, each typed to its own entity.
        output.Should().Contain("public static UpdatePreOperationMy_entityEntity Target");
        output.Should().Contain("public static UpdatePostOperationMy_entityEntity Target");

        // Per-step attribute filtering: the pre-operation step requested only "name",
        // the post-operation step requested only "email". Each field constant must appear
        // exactly once (i.e. in a single entity type), proving attributes are not leaking
        // across steps.
        Regex.Matches(output, @"public const string Name = ""my_name"";").Count.Should().Be(1);
        Regex.Matches(output, @"public const string Email = ""my_email"";").Count.Should().Be(1);
    }

    [Fact]
    public void Render_TargetWithSharedGlobalOptionSet_ProducesOneNestedEnum()
    {
        var model = BuildModelWithTwoUpdateSteps();
        model.PluginTypes.First().Steps.First().PrimaryEntityDefinition = BuildEntityWithSharedGlobalOptionSet();

        var output = RenderPlugin(model);

        Regex.Matches(output, @"public enum SharedStatus").Count.Should().Be(1);
        output.Should().Contain("public UpdatePreOperationMy_entityEntity.Meta.OptionSets.SharedStatus? StatusOne");
        output.Should().Contain("public UpdatePreOperationMy_entityEntity.Meta.OptionSets.SharedStatus? StatusTwo");
    }

    [Fact]
    public void Render_ImageWithSharedGlobalOptionSet_ProducesOneNestedEnum()
    {
        var model = BuildModelWithTwoUpdateSteps();
        var step = model.PluginTypes.First().Steps.First();
        step.Images =
        [
            new()
            {
                ImageType = ImageTypes.PreImage,
                Name = "PreImage",
                MessagePropertyDefinition = BuildEntityWithSharedGlobalOptionSet()
            }
        ];

        var output = RenderPlugin(model);

        output.Should().Contain("class UpdatePreOperationPreImageMy_entity");
        Regex.Matches(output, @"public enum SharedStatus").Count.Should().Be(1);
        output.Should().Contain("public UpdatePreOperationPreImageMy_entity.Meta.OptionSets.SharedStatus? StatusOne");
        output.Should().Contain("public UpdatePreOperationPreImageMy_entity.Meta.OptionSets.SharedStatus? StatusTwo");
    }

    private static PluginAssemblyConfig BuildModelWithTwoUpdateSteps()
    {
        var message = new SdkMessage
        {
            Name = "Update",
            Pairs =
            [
                new SdkMessagePair
                {
                    Request =
                    [
                        new SdkMessageRequest
                        {
                            Fields =
                            [
                                new SdkMessageRequestField
                                {
                                    Name = "Target",
                                    Parser = "Microsoft.Xrm.Sdk.Entity, Microsoft.Xrm.Sdk",
                                    Optional = false
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var preStep = new PluginStepConfig
        {
            MessageName = "Update",
            Stage = Stages.PreOperation,
            PrimaryEntityName = "my_entity",
            FilteringAttributes = "my_name",
            Message = message,
            PrimaryEntityDefinition = BuildEntity(("Name", "my_name"))
        };

        var postStep = new PluginStepConfig
        {
            MessageName = "Update",
            Stage = Stages.PostOperation,
            PrimaryEntityName = "my_entity",
            FilteringAttributes = "my_email",
            Message = message,
            PrimaryEntityDefinition = BuildEntity(("Email", "my_email"))
        };

        var pluginType = new PluginTypeConfig
        {
            TypeName = "My.MyPlugin",
            Namespace = "My",
            IsNullableEnabled = false,
            Steps = [preStep, postStep]
        };

        return new PluginAssemblyConfig { PluginTypes = [pluginType] };
    }

    private static EntityMetadata BuildEntity(params (string Schema, string Logical)[] attributes)
    {
        var attributeMetadata = new List<AttributeMetadata>();
        foreach (var (schema, logical) in attributes)
        {
            attributeMetadata.Add(new AttributeMetadata
            {
                SchemaName = schema,
                LogicalName = logical,
                AttributeType = AttributeTypeCode.String,
                IsPrimaryId = false,
                IsLogical = false,
                IsValidForCreate = true,
                IsValidForRead = true,
                IsValidForUpdate = true,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None)
            });
        }

        return new EntityMetadata("my_entity", "my_entities")
        {
            SchemaName = "My_entity",
            PrimaryIdAttribute = "my_entityid",
            LogicalCollectionName = "my_entities",
            Attributes = [.. attributeMetadata]
        };
    }

    private static EntityMetadata BuildEntityWithSharedGlobalOptionSet()
    {
        var optionSet = new OptionSetMetadata
        {
            Name = "my_shared_status",
            DisplayName = Label("Shared Status"),
            Description = Label("Shared status values."),
            IsCustomizable = new ManagedBooleanProperty(true),
            ExternalTypeName = string.Empty,
            IntroducedVersion = string.Empty,
            IsGlobal = true,
            OptionSetType = OptionSetType.Picklist,
            Options = [new OptionMetadata(Label("Active"), 1)]
        };

        return new EntityMetadata("my_entity", "my_entities")
        {
            SchemaName = "My_entity",
            PrimaryIdAttribute = "my_entityid",
            LogicalCollectionName = "my_entities",
            Attributes =
            [
                Picklist("StatusOne", "my_statusone", optionSet),
                Picklist("StatusTwo", "my_statustwo", optionSet)
            ]
        };
    }

    private static PicklistAttributeMetadata Picklist(string schemaName, string logicalName, OptionSetMetadata optionSet) =>
        new(schemaName)
        {
            LogicalName = logicalName,
            SchemaName = schemaName,
            IsPrimaryId = false,
            IsLogical = false,
            IsValidForCreate = true,
            IsValidForRead = true,
            IsValidForUpdate = true,
            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
            OptionSet = optionSet
        };

    private static Label Label(string value) => new()
    {
        LocalizedLabels = [new LocalizedLabel { LanguageCode = 1033, Label = value }]
    };

    private static string RenderPlugin(PluginAssemblyConfig model)
    {
        var templatePath = GetTemplatePath("Plugin.sbncs");

        var config = new XrmCodeGenConfig
        {
            DefaultNamespace = "My",
            TemplateFilePath = templatePath,
            InputFileName = "MyPlugin.cs"
        };

        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add(
            ScribanExtensionCache.KnownAssemblies.Humanizr.ToString().ToLowerInvariant(),
            ScribanExtensionCache.GetHumanizrMethods());
        scriptObject.Add("config", config);
        scriptObject.Add("model", model);
        scriptObject.Add("codegen_attribute", "[GeneratedCode(\"Test\", \"1.0\")]");

        var context = new TemplateContext
        {
            LoopLimit = 5000,
            TemplateLoader = new FileTemplateLoader(Path.GetDirectoryName(templatePath))
        };
        context.PushGlobal(scriptObject);

        var template = Template.Parse(File.ReadAllText(templatePath), templatePath);
        template.HasErrors.Should().BeFalse(string.Join("\n", template.Messages));

        return template.Render(context);
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
