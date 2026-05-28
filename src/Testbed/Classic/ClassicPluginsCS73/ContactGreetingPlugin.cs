namespace XrmGenTestClassic
{
    using Microsoft.Xrm.Sdk;
    using System;
    using XrmTools.Meta.Attributes;

    [Plugin]
    [Step("Create", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
    public partial class ContactGreetingPlugin : PluginBase<ContactGreetingPlugin>, IPlugin
    {
        [Dependency]
        public IContactPersister ContactPersister { get => Require<IContactPersister>(); }

        [Dependency]
        public IContactOrchestrator ContactOrchestrator { get => Require<IContactOrchestrator>(); }

        [Dependency]
        public IValidationService ValidationService { get => Require<IValidationService>(); }

        override protected void ExecuteInternal(IServiceProvider serviceProvider)
        {
            Target.Description = $"Hello {Target.FirstName} {Target.LastName}!";
        }
    }
}