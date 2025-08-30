namespace XrmGenTestClassic
{
    using Microsoft.Xrm.Sdk;
    using System;
    using XrmTools.Meta.Attributes;

    [Plugin]
    [Step("Create", "account", "name, description, accountnumber", Stages.PreOperation, ExecutionMode.Synchronous)]
    public partial class AccountCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            using (var scope = CreateScope(serviceProvider))
            {
                Target.Description = $"Welcome to Dynamics 365, {Target.Name}!";
            }
        }
    }
}
