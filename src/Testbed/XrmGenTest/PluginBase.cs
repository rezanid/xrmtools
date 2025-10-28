namespace XrmGenTest;

using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using XrmTools;
using XrmTools.Meta.Attributes;

public abstract class PluginBase<TPlugin> : IPlugin where TPlugin : IPlugin
{
    [Dependency]
    protected ITracingService Tracing { get => Require<ITracingService>(); }

    [Dependency]
    protected ILoggingService Logging { get => Require<ILoggingService>(); }

    [DependencyProvider("User")]
    protected IOrganizationService UserOrgService
    {
        get => TryGet<IOrganizationService>("User", out var service)
            ? service
            : Set("User", OrgServiceFactory.CreateOrganizationService(Context.UserId));
    }

    [Dependency]
    protected IOrganizationServiceFactory OrgServiceFactory { get => Require<IOrganizationServiceFactory>(); }

    [Dependency]
    protected IPluginExecutionContext Context { get => Require<IPluginExecutionContext>(); }

    public void Execute(IServiceProvider serviceProvider)
    {
        using (var scope = CreateScope(serviceProvider))
        {
            try
            {
                ExecuteInternal(serviceProvider);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Tracing.Trace($"[e] {ex}");
                throw new InvalidPluginExecutionException($"OrganizationServiceFault: {ex.Message}", ex);
            }
            catch (InvalidCastException ex)
            {
                Tracing.Trace($"[e] {ex}");
                throw new InvalidPluginExecutionException(
                    "Request cannot be processed, potentially due to invalid data type assigned to a parameter.",
                    PluginHttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Tracing.Trace($"[e] {ex}");
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }
    }

    protected abstract IDisposable CreateScope(IServiceProvider serviceProvider);

    protected abstract void ExecuteInternal(IServiceProvider serviceProvider);

    protected static T Require<T>() => DependencyScope<TPlugin>.Current.Require<T>();
    protected static T Require<T>(string name) => DependencyScope<TPlugin>.Current.Require<T>(name);

    protected static bool TryGet<T>(out T instance) => DependencyScope<TPlugin>.Current.TryGet(out instance);
    protected static bool TryGet<T>(string name, out T instance) => DependencyScope<TPlugin>.Current.TryGet(name, out instance);

    protected static T Set<T>(T instance) => DependencyScope<TPlugin>.Current.Set(instance);
    protected static T Set<T>(string name, T instance) => DependencyScope<TPlugin>.Current.Set(name, instance);
    protected static T SetAndTrack<T>(T instance) where T : IDisposable => DependencyScope<TPlugin>.Current.SetAndTrack(instance);
    protected static T SetAndTrack<T>(string name, T instance) where T : IDisposable => DependencyScope<TPlugin>.Current.SetAndTrack(name, instance);
}
