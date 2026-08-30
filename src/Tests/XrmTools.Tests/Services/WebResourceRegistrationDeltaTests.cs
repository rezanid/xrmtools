namespace XrmTools.Tests.Services;

using FluentAssertions;
using System;
using XrmTools.Services;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Xunit;

public class WebResourceRegistrationDeltaTests
{
    [Fact]
    public void CalculatesCreatesUpdatesDeletesAndUnchangedResources()
    {
        var unchangedId = Guid.NewGuid();
        var updateId = Guid.NewGuid();
        var removedId = Guid.NewGuid();
        var desired = new[]
        {
            Desired("contoso_/new.js", "bmV3"),
            Desired("contoso_/changed.js", "bmV3IGNvbnRlbnQ="),
            Desired("contoso_/same.js", "c2FtZQ==")
        };
        var existing = new[]
        {
            Existing(updateId, "contoso_/changed.js", "b2xk"),
            Existing(unchangedId, "contoso_/same.js", "c2FtZQ=="),
            Existing(removedId, "contoso_/removed.js", "cmVtb3ZlZA==")
        };

        var delta = WebResourceRegistrationService.CalculateDelta(desired, existing);

        delta.Creates.Should().ContainSingle(resource => resource.Name == "contoso_/new.js");
        delta.Updates.Should().ContainSingle(update => update.Desired.Name == "contoso_/changed.js");
        delta.Updates[0].Desired.Id.Should().Be(updateId);
        delta.Deletes.Should().ContainSingle(resource => resource.Id == removedId);
        delta.UnchangedCount.Should().Be(1);
        desired[2].Id.Should().Be(unchangedId);
    }

    [Theory]
    [InlineData(".html", WebResourceType.Webpage)]
    [InlineData(".css", WebResourceType.StyleSheet)]
    [InlineData(".js", WebResourceType.Script)]
    [InlineData(".png", WebResourceType.Png)]
    [InlineData(".svg", WebResourceType.Svg)]
    [InlineData(".resx", WebResourceType.Resx)]
    public void MapsSupportedExtensions(string extension, WebResourceType expectedType)
        => WebResourceTypes.FromExtension(extension).Should().Be(expectedType);

    private static DesiredWebResource Desired(string name, string content)
        => new(Guid.NewGuid(), name, name, null, WebResourceType.Script, content);

    private static WebResource Existing(Guid id, string name, string content)
        => new()
        {
            Id = id,
            Name = name,
            DisplayName = name,
            WebResourceType = WebResourceType.Script,
            Content = content
        };
}
