using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace $rootnamespace$
{
    /// <summary>
    /// This is a sample plugin with registration attributes (i.e. [Plugin], [Step]) and code generation. Learn
    /// more about writing [Power Platform plugins using Xrm Tools](https://github.com/rezanid/xrmtools/wiki/Writing-a-Plugin)
    /// If you see errors in the code, just save this file (Ctrl+S) to trigger code generation - all the errors will disappear.
    /// </summary>
    [Plugin]
    [Step("Create", "account", "name,description,industrycode", Stages.PreOperation, ExecutionMode.Synchronous)]
    public partial class $safeitemname$ : IPlugin
    {
        readonly string _config;
        readonly string _secureConfig;

        public $safeitemname$(string config, string secureConfig)
        {
            _config = config;
            _secureConfig = secureConfig;
        }

        public string Config => _config;
        public string SecureConfig => _secureConfig;

        /// <summary>
        /// Keep your plugins lean and more maintainable using [Dependency Injection](https://github.com/rezanid/xrmtools/wiki/Dependency-Injection)
        /// </summary>
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
                Tracing.Trace("$safeitemname$: Execute started.");
                Target.Description = $"Hello {Target.Name}!";
            }
        }
    }
}
