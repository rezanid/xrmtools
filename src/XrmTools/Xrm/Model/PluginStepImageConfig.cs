#nullable enable
namespace XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public enum ImageTypes
{
    PreImage = 0,
    PostImage = 1,
    Both = 2
}

public interface IMessageProcessingStepImageEntity
{
    Guid? PluginStepImageId { get; set; }
    string? EntityAlias { get; set; }
    string? ImageAttributes { get; set; }
    ImageTypes? ImageType { get; set; }
    string? MessagePropertyName { get; set; }
    string? Name { get; set; }
}

public interface IMessageProcessingStepImageConfig : IMessageProcessingStepImageEntity
{
    EntityMetadata? MessagePropertyDefinition { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginStepImageConfig : TypedEntity<PluginStepImageConfig>, IMessageProcessingStepImageConfig
{
    public const string EntityLogicalName = "sdkmessageprocessingstepimage";

    #region IMessageProcessingStepImageConfig-only Properties
    public EntityMetadata? MessagePropertyDefinition { get; set; }
    #endregion

    #region IMessageProcessingStepImageEntity Properties
    [AttributeLogicalName("sdkmessageprocessingstepimageid")]
    [JsonPropertyName("Id")]
    [JsonProperty("Id")]
    public Guid? PluginStepImageId
    {
        get => TryGetAttributeValue("sdkmessageprocessingstepimageid", out Guid value) ? value : null;
        set => this["sdkmessageprocessingstepimageid"] = value;
    }
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string value) ? value : null;
        set => this["name"] = value; 
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public new AttributeCollection Attributes
    {
        get { return base.Attributes; }
    }

    [AttributeLogicalName("attributes")]
    [JsonPropertyName("attributes")]
    public string? ImageAttributes 
    { 
        get => TryGetAttributeValue("attributes", out string value) ? value : null; 
        set => this["attributes"] = value; 
    }
    [AttributeLogicalName("entityalias")]
    public string? EntityAlias 
    { 
        get => TryGetAttributeValue("entityalias", out string value) ? value : null;
        set => this["entityalias"] = value;
    }
    [AttributeLogicalName("imagetype")]
    public ImageTypes? ImageType 
    {
        get => TryGetAttributeValue("imagetype", out OptionSetValue option) ? (ImageTypes)option.Value : null;
        set => this["imagetype"] = value == null ? null : new OptionSetValue((int)value);
    }
    [AttributeLogicalName("messagepropertyname")]
    public string? MessagePropertyName 
    { 
        get => TryGetAttributeValue("messagepropertyname", out string value) ? value : null;
        set => this["messagepropertyname"] = value;
    }
    #endregion

    public PluginStepImageConfig() : base(EntityLogicalName) { }
}

public class MessageProcessingStepImageConverter : System.Text.Json.Serialization.JsonConverter<PluginStepImageConfig>
{
    public override PluginStepImageConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter writer, PluginStepImageConfig value, JsonSerializerOptions options) => throw new NotImplementedException();
}
#nullable restore