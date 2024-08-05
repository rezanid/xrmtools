#nullable enable
namespace XrmGen.Xrm.Model;

using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;


public class PluginAssemblyInfo
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int IsolationMode { get; set; }
    public int SourceType { get; set; }
    public string? Version { get; set; }
    public required IEnumerable<Plugin> PluginTypes { get; set; }
    public IEnumerable<OptionSetMetadata>? OptionMetadatas { get; set; }

}
#nullable restore