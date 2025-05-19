using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Update", "annotation", "owningteam", Stages.PostOperation, ExecutionMode.Synchronous)]
public partial class LeadCreatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        Initialize(serviceProvider);
    }
}
