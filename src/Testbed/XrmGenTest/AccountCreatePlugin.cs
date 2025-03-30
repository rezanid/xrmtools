using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

namespace XrmGenTest;

public abstract class PluginBase : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        Initialize(serviceProvider);
    }

    internal virtual void Initialize(IServiceProvider serviceProvider) { }
}

[Plugin("AccountPostCreate")]
[Step("account", "Create", "name,accountnumber,createdon", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "Target", "name,accountnumber,createdon")]
public partial class AccountCreatePlugin : PluginBase
{
    public void ExecuteLocal(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        var organizationService = serviceProvider.GetOrganizationService(executionContext.UserId);
        var tracing = serviceProvider.Get<ITracingService>();
        //Initialize(serviceProvider);
    }
}