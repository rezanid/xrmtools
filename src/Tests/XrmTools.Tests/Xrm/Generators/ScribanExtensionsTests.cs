namespace XrmTools.Tests.Xrm.Generators;

using FluentAssertions;
using System.Collections.Generic;
using Scriban;
using Scriban.Runtime;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model.Configuration;
using XrmTools.WebApi.Types;
using XrmTools.Xrm.Generators;
using Xunit;

public class ScribanExtensionsTests
{
    [Fact]
    public void GetLabel_ReturnsRequestedLanguage_WhenPresent()
    {
        var label = new Label
        {
            LocalizedLabels =
            [
                new LocalizedLabel { LanguageCode = 1036, Label = "Statut" },
                new LocalizedLabel { LanguageCode = 1033, Label = "Status" },
            ]
        };

        ScribanExtensions.GetLabel(label, 1033).Should().Be("Status");
    }

    [Fact]
    public void GetLabel_FallsBackToUserLocalizedLabel_WhenRequestedLanguageMissing()
    {
        var label = new Label
        {
            UserLocalizedLabel = new LocalizedLabel { LanguageCode = 1036, Label = "Statut" },
            LocalizedLabels =
            [
                new LocalizedLabel { LanguageCode = 1036, Label = "Statut" },
            ]
        };

        ScribanExtensions.GetLabel(label, 1033).Should().Be("Statut");
    }

    [Fact]
    public void GetLabel_FallsBackToFirstNonEmptyLabel_WhenRequestedAndUserLabelMissing()
    {
        var label = new Label
        {
            UserLocalizedLabel = new LocalizedLabel { LanguageCode = 1036, Label = string.Empty },
            LocalizedLabels =
            [
                new LocalizedLabel { LanguageCode = 1036, Label = "Statut" },
            ]
        };

        ScribanExtensions.GetLabel(label, 1033).Should().Be("Statut");
    }

    [Fact]
    public void GetLabel_ReturnsNull_WhenLabelIsNull()
    {
        ScribanExtensions.GetLabel(null, 1033).Should().BeNull();
    }

    [Fact]
    public void GetLabel_ReturnsNull_WhenNoLabelsAvailable()
    {
        var label = new Label
        {
            UserLocalizedLabel = new LocalizedLabel { Label = string.Empty },
            LocalizedLabels = []
        };

        ScribanExtensions.GetLabel(label, 1033).Should().BeNull();
    }

    private static PluginStepConfig Step(string message, Stages? stage) =>
        new() { MessageName = message, Stage = stage };

    [Fact]
    public void StepTypeDiscriminator_ReturnsEmpty_ForSingleStep()
    {
        var step = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { step };

        ScribanExtensions.StepTypeDiscriminator(steps, step).Should().BeEmpty();
    }

    [Fact]
    public void StepTypeDiscriminator_ReturnsEmpty_WhenMessagesAreDistinct()
    {
        var create = Step("Create", Stages.PostOperation);
        var update = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { create, update };

        ScribanExtensions.StepTypeDiscriminator(steps, create).Should().BeEmpty();
        ScribanExtensions.StepTypeDiscriminator(steps, update).Should().BeEmpty();
    }

    [Fact]
    public void StepTypeDiscriminator_UsesStageName_WhenMessageSharedWithDifferentStages()
    {
        var pre = Step("Update", Stages.PreOperation);
        var post = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { pre, post };

        ScribanExtensions.StepTypeDiscriminator(steps, pre).Should().Be("PreOperation");
        ScribanExtensions.StepTypeDiscriminator(steps, post).Should().Be("PostOperation");
    }

    [Fact]
    public void StepTypeDiscriminator_AppendsOrdinal_WhenMessageAndStageShared()
    {
        var first = Step("Update", Stages.PostOperation);
        var second = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { first, second };

        ScribanExtensions.StepTypeDiscriminator(steps, first).Should().Be("PostOperation1");
        ScribanExtensions.StepTypeDiscriminator(steps, second).Should().Be("PostOperation2");
    }

    [Fact]
    public void StepTypeDiscriminator_OnlyDisambiguatesCollidingMessages()
    {
        var createPost = Step("Create", Stages.PostOperation);
        var updatePre = Step("Update", Stages.PreOperation);
        var updatePost = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { createPost, updatePre, updatePost };

        ScribanExtensions.StepTypeDiscriminator(steps, createPost).Should().BeEmpty();
        ScribanExtensions.StepTypeDiscriminator(steps, updatePre).Should().Be("PreOperation");
        ScribanExtensions.StepTypeDiscriminator(steps, updatePost).Should().Be("PostOperation");
    }

    [Fact]
    public void StepTypeDiscriminator_ReturnsEmpty_WhenArgumentsAreNull()
    {
        ScribanExtensions.StepTypeDiscriminator(null, Step("Update", Stages.PostOperation)).Should().BeEmpty();
        ScribanExtensions.StepTypeDiscriminator(new List<PluginStepConfig>(), null).Should().BeEmpty();
    }

    [Fact]
    public void StepTypeDiscriminator_IsCallableFromScriban_ProducingUniqueTypeNames()
    {
        var pre = Step("Update", Stages.PreOperation);
        var post = Step("Update", Stages.PostOperation);
        var steps = new List<PluginStepConfig> { pre, post };

        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add("steps", steps);

        var template = Template.Parse(
            "{{~for step in steps~}}{{steps | step_type_discriminator step}}Update|{{~end~}}");
        template.HasErrors.Should().BeFalse(string.Join("\n", template.Messages));

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        template.Render(context).Should().Be("PreOperationUpdate|PostOperationUpdate|");
    }
}
