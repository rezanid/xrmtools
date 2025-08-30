namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum StringFormat
{
    Email,
    Text,
    TextArea,
    Url,
    TickerSymbol,
    PhoneticGuide,
    VersionNumber,
    Phone,
    Json,
    RichText
}