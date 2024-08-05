using System.Collections.Generic;

namespace XrmGen.Xrm.Model;

public class Plugin
{
    public string Id { get; set; }
    public string Name { get; set; }
    public object Description { get; set; }
    public string FriendlyName { get; set; }
    public string TypeName { get; set; }
    public object WorkflowActivityGroupName { get; set; }
    public IEnumerable<MessageProcessingStep> Steps { get; set; }
}
