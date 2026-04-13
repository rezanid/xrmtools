namespace $safeprojectname$
{
    using Microsoft.Xrm.Sdk;
    using System;
    using XrmTools.Meta.Attributes;

    [Plugin]
    [Step("Create", "account", "name,description,industrycode", Stages.PreOperation, ExecutionMode.Synchronous)]
    public partial class HelloWorldPlugin : IPlugin
    {
        readonly string _config;
        readonly string _secureConfig;

        public HelloWorldPlugin(string config, string secureConfig)
        {
            _config = config;
            _secureConfig = secureConfig;
        }

        public string Config => _config;
        public string SecureConfig => _secureConfig;

        [Dependency]
        IPluginExecutionContext Context => Require<IPluginExecutionContext>();

        [Dependency]
        IOrganizationServiceFactory ServiceFactory => Require<IOrganizationServiceFactory>();

        [Dependency]
        ITracingService Tracing => Require<ITracingService>();

        [DependencyProvider]
        IOrganizationService OrganizationService => TryGet<IOrganizationService>(out var instance)
                ? instance
                : Set(ServiceFactory.CreateOrganizationService(null));

        [DependencyProvider("User")]
        IOrganizationService OrganizationUserService => TryGet<IOrganizationService>("User", out var instance)
                ? instance
                : Set("User", ServiceFactory.CreateOrganizationService(Context.UserId));

        public void Execute(IServiceProvider serviceProvider)
        {
            using (CreateScope(serviceProvider))
            {
                Tracing.Trace("HelloWorldPlugin: Execute started.");
                Target.Description = $"Hello {Target.Name}!";
            }
        }
    }
}
