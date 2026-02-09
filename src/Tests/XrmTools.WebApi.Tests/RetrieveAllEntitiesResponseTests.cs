using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using XrmTools.Meta.Model;
using Xunit;

namespace XrmTools.WebApi.Tests;

public class RetrieveAllEntitiesResponseTests
{
    //    [Fact]
    //    public void CanDeserializeSampleJson()
    //    {
    //        // Arrange
    //        var filePath = Path.Combine("Samples", "RetrieveAllEntitiesResponse.txt");
    //        using var stream = File.OpenRead(filePath);

    //        // Act
    //        var result = JsonSerializer.Deserialize<RetrieveAllEntitiesResponse>(stream);

    //        // Assert
    //        result.Should().NotBeNull();
    //        result.EntityMetadata.Should().NotBeNull();
    //        result.EntityMetadata.Count.Should().BeGreaterThan(0);
    //    }
    //}

    //[Fact]
    //public void NewtonsoftDeserializeWithCustomSettings()
    //{
    //    var filePath = Path.Combine("Samples", "RetrieveAllEntitiesResponse.txt");
    //    using var stream = File.OpenRead(filePath);
    //    using var sr = new StreamReader(stream);
    //    using var jr = new JsonTextReader(sr);
    //    var settings = new JsonSerializerSettings()
    //    {
    //        TypeNameHandling = TypeNameHandling.Auto,
    //        NullValueHandling = NullValueHandling.Ignore,
    //        ContractResolver = new IgnoreEntityPropertiesResolver()
    //    };
    //    var serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
    //    var result = serializer.Deserialize<RetrieveAllEntitiesResponse>(jr);

    //    // Assert
    //    result.Should().NotBeNull();
    //}
}