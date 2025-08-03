namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum EndpointType
{
    OrganizationService,
    OrganizationDataService,
    WebApplication
}
