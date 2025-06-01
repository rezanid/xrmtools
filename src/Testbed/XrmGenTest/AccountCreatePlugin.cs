using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

public abstract class PluginBase : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        var organizationService = serviceProvider.GetOrganizationService(executionContext.UserId);
        var tracing = serviceProvider.Get<ITracingService>();
        Initialize(serviceProvider);
    }

    internal virtual void Initialize(IServiceProvider serviceProvider) { }
}

[Plugin]
[Step("Create", "account", "accountnumber,accountcategorycode,accountclassificationcode", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "accountnumber,accountcategorycode,address1_freighttermscode,accountid,address1_addresstypecode")]
public partial class AccountCreatePlugin : PluginBase, IPlugin
{
    public void ExecuteLocal(IServiceProvider serviceProvider)
    {
        // It's just business logic!
    }
}