using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Update", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PreImage, "firstname,lastname,description,accountrolecode", Name = "ContactPreImage", EntityAlias = "ContactPreImage")]
[Step("Create", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
public partial class ContactGreetingPlugin : PluginBase<ContactGreetingPlugin>, IPlugin
{
    [Dependency]
    public IContactPersister ContactPersister => Require<IContactPersister>();

    [Dependency]
    public IContactOrchestrator ContactOrchestrator => Require<IContactOrchestrator>();

    [Dependency]
    public IValidationService ValidationService => Require<IValidationService>();

    public ITracingService MyProperty { get; set; }
    override protected void ExecuteInternal(IServiceProvider serviceProvider)
    {
        CreateTarget.Description = $"Hello {CreateTarget.FirstName} {CreateTarget.LastName}!";
    }
}