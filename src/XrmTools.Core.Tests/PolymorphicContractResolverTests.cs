namespace XrmTools.Core.Tests;
using Newtonsoft.Json;
using System.Linq;
using Xunit;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using XrmTools.Serialization;
using FluentAssertions;

public class PolymorphicContractResolverTests
{
    [Fact]
    public void Deserialize_Should_Detect_Polymorphic_Properties_In_RetrieveEntityResponse()
    {
        // Arrange
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new PolymorphicContractResolver()
        };
        var json = File.ReadAllText("RetrieveEntity_beoneRequest.json");
        var jobj = JObject.Parse(json);

        // Act
        var result = JsonConvert.DeserializeObject<EntityMetadata>(jobj.GetValue("EntityMetadata").ToString(), settings);
        var response = new RetrieveEntityResponse();
        response.Results.Add("EntityMetadata", result);

        // Assert
        Assert.NotNull(result);
        var jattributes = jobj["EntityMetadata"]["Attributes"];
        jattributes.Should().NotBeNull();
        result.Attributes.Should().HaveCount(jattributes.Count());
        var index = 0;
        foreach (var jtoken in jattributes)
        {
            var jItem = jtoken as JObject;
            var item = response.EntityMetadata.Attributes[index++];
            if (jItem.TryGetValue("@odata.type", out var typeAnnotation))
            {
                typeAnnotation.Value<string>().TrimStart('#').Should().EndWith(item.GetType().Name);
            }
            else
            {
                item.GetType().Name.Should().Be(typeof(AttributeMetadata).Name);
            }
        }
    }
}