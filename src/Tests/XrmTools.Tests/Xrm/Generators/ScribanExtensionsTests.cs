namespace XrmTools.Tests.Xrm.Generators;

using FluentAssertions;
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
}
