using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Create", "contact", "firstname,lastname,description", Stages.PreOperation, ExecutionMode.Synchronous)]
public partial class ContactCreatePlugin : IPlugin
{
    public ITracingService MyProperty { get; set; }
    public void Execute(IServiceProvider serviceProvider)
    {
        Target.Description = $"Hello {Target.FirstName} {Target.LastName}!";
    }
}
