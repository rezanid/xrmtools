using System;

namespace XrmGen.Xrm.Model;

public class PluginStepImageRegistration
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Attributes { get; set; }
    public string EntityAlias { get; set; }
    public string MessagePropertyName { get; set; }
    public int ImageType { get; set; }
}
