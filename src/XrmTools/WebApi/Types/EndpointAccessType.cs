﻿namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum EndpointAccessType
{
    Default,
    Internet,
    Intranet
}
