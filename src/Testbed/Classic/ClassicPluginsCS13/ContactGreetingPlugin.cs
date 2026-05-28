using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Create", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
[Step("Update", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PreImage, "firstname,lastname,description,accountrolecode", Name = "ContactPreImage", EntityAlias = "ContactPreImage")]
public partial class ContactGreetingPlugin : PluginBase<ContactGreetingPlugin>, IPlugin
{
    [Dependency]
    public IContactPersister ContactPersister => Require<IContactPersister>();

    [Dependency]
    public IContactOrchestrator ContactOrchestrator => Require<IContactOrchestrator>();

    [Dependency]
    public IValidationService ValidationService => Require<IValidationService>();

    [Dependency]
    public IOrganizationService OrganizationService => Require<IOrganizationService>();

    override protected void ExecuteInternal(IServiceProvider serviceProvider)
    {
        using var scope = CreateScope(serviceProvider);
        var createTarget = CreateInputParameters.Target;
        createTarget.Description = $"Hello {createTarget.FirstName} {createTarget.LastName}!";
    }
}