using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmTraditionalPlugins
{
    public abstract class PluginBase<TPlugin> where TPlugin : IPlugin
    {
        [Dependency]
        protected IOrganizationServiceFactory ServiceFactory => Require<IOrganizationServiceFactory>();

        [DependencyProvider]
        protected IOrganizationService CurrentUserService 
            => TryGet<IOrganizationService>(out var service)
            ? service
            : Set(ServiceFactory.CreateOrganizationService(Context.UserId));

        [Dependency]
        protected ITracingService Tracer => Require<ITracingService>();

        [Dependency]
        protected static ILogger Logger => Require<ILogger>();

        [Dependency]
        protected IExecutionContext Context => Require<IExecutionContext>();

        protected static T Require<T>() => DependencyScope<TPlugin>.Current.Require<T>();
        protected static T Require<T>(string name) => DependencyScope<TPlugin>.Current.Require<T>(name);
        protected static bool TryGet<T>(out T instance) => DependencyScope<TPlugin>.Current.TryGet(out instance);
        protected static bool TryGet<T>(string name, out T instance) => DependencyScope<TPlugin>.Current.TryGet(name, out instance);
        protected static T Set<T>(T instance) => DependencyScope<TPlugin>.Current.Set(instance);
    }
}