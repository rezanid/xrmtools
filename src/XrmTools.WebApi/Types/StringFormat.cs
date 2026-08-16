namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System;

[Serializable]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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