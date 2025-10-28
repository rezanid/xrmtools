using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Create", "account", "name, description, accountnumber", Stages.PostOperation, ExecutionMode.Synchronous)]
public partial class AccountGreetingPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        // Without Dependency Injection:
        var executionContext = serviceProvider.Get<IPluginExecutionContext>();
        var target = executionContext.InputParameters["Target"] as Entity;
        target["description"] = $"Welcome to Power Platform, {target["name"]}!";

        // With Dependency Injection:
        using var scope = CreateScope(serviceProvider);
        Target.Description = $"Welcome to Power Platform, {Target.Name}!";
    }
}