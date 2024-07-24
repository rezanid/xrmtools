using System;
using System.Collections.Generic;

namespace XrmGen.Xrm.Model;

public class PluginAssembly
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int IsolationMode { get; set; }
    public int SourceType { get; set; }
    public string Version { get; set; }
    public IEnumerable<Plugin> PluginTypes { get; set; }
}