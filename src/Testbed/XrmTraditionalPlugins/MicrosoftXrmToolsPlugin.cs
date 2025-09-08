using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Net.Http;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmTraditionalPlugins
{
  public abstract class PluginBase<TPlugin> where TPlugin : IPlugin
  {
    protected static T Require<T>()
      => DependencyScope<TPlugin>.Current.Require<T>();
    protected static T Require<T>(string name)
      => DependencyScope<TPlugin>.Current.Require<T>(name);
    protected static bool TryGet<T>(out T instance)
      => DependencyScope<TPlugin>.Current.TryGet(out instance);
    protected static bool TryGet<T>(string name, out T instance)
      => DependencyScope<TPlugin>.Current.TryGet(name, out instance);
    protected static T Set<T>(T instance)
      => DependencyScope<TPlugin>.Current.Set(instance);
  }

  [Plugin]
  [Step("Create", "account", "name,description", Stages.PostOperation, ExecutionMode.Asynchronous)]
  public partial class CleanAccountPostOperation : PluginBase<CleanAccountPostOperation>, IPlugin
  {    
    [Dependency]
    IOrganizationServiceFactory ServiceFactory { get => Require<IOrganizationServiceFactory>(); }
    
    [DependencyProvider]
    IOrganizationService CurrentUserService 
    { 
      get => TryGet<IOrganizationService>(out var service) ? service : Set(ServiceFactory.CreateOrganizationService(Context.UserId));
    }

    [Dependency]
    ITracingService Tracer { get => Require<ITracingService>();  }
    
    [Dependency]
    static ILogger Logger { get => Require<ILogger>(); }
    
    [Dependency]
    IExecutionContext Context { get => Require<IExecutionContext>(); }
    
    [Dependency]
    IBusinessService Business { get => Require<IBusinessService>(); }

    //[DependencyProvider("Config")]
    public string Config { get; set; } = "https://echo.free.beeceptor.com/sample-request?author=XrmTools";

    public CleanAccountPostOperation(string config) { Config = config; }

    public void Execute(IServiceProvider serviceProvider)
    {
      try
      {
        string startExecMsg = "Start execution of AccountPostOperation";
        Logger.LogInformation(startExecMsg);
        Tracer.Trace(startExecMsg);

        if (Target.LogicalName != "account")
        {
          string wrongEntityMsg = "Plug-in registered for wrong entity {0}";
          Logger.LogWarning(wrongEntityMsg, Target.LogicalName);
          Tracer.Trace(wrongEntityMsg, Target.LogicalName);
          return;
        }

        string activityMsg = "Callback";

        using (Logger.BeginScope(activityMsg))
        {
          Tracer.Trace(activityMsg);

          string startTaskMsg = "Start Task Creation";
          Logger.LogInformation(startTaskMsg);
          Tracer.Trace(startTaskMsg);

          var followup = new Task
          {
            Subject = "Send e-mail to the new customer.",
            Description = "Follow up with the customer. Check if there are any new issues that need resolution.",
            ScheduledStart = DateTime.Now.AddDays(7),
            ScheduledEnd = DateTime.Now.AddDays(7),
            Category = Context.PrimaryEntityName
          };

          // Refer to the account in the task activity.
          if (Context.OutputParameters.Contains("id"))
          {
            followup.RegardingObjectId = Target.ToEntityReference();
          }

          //Create the task
          CurrentUserService.Create(followup);

          string endTaskMsg = "Task creation completed";
          Logger.LogInformation(endTaskMsg);
          Tracer.Trace(endTaskMsg);
        }

        string outBoundScope = "OutboundCall";

        using (Logger.BeginScope(outBoundScope))
        {
          string outboundStartMsg = "Outbound call started";
          Logger.LogInformation(outboundStartMsg);
          Tracer.Trace(outboundStartMsg);

          using (HttpClient client = new HttpClient())
          {
            client.Timeout = TimeSpan.FromMilliseconds(15000); //15 seconds
            client.DefaultRequestHeaders.ConnectionClose = true; //Set KeepAlive to false

            HttpResponseMessage response = client
              .GetAsync(requestUri: Config)
              .GetAwaiter()
              .GetResult(); //Make sure it is synchronous

            response.EnsureSuccessStatusCode();

            string responseText = response.Content
              .ReadAsStringAsync()
              .GetAwaiter()
              .GetResult(); //Make sure it is synchronous

            string shortResponseText = responseText.Substring(0, 20);

            Logger.LogInformation(shortResponseText);
            Tracer.Trace(shortResponseText);

            string outboundEndMsg = "Outbound call ended successfully";

            Logger.LogInformation(outboundEndMsg);
            Tracer.Trace(outboundEndMsg);
            
          }
        }
      }
      catch (Exception e)
      {
        string errMsg = "Plugin failed";
        Logger.LogError(e, errMsg);
        Tracer.Trace($"{errMsg}:{e.Message}");
        throw new InvalidPluginExecutionException(e.Message, e);
      }
    }
  }
  
  public interface IBusinessService 
  {
    void DoWork(Entity entity);  
  }

  public class BusinessService : IBusinessService
  {
    //[Dependency]
    //public string Config { get; set; }
    
    public BusinessService(ILogger logger, string config)
    {
            
    }
    public void DoWork(Entity entity) { throw new NotImplementedException(); }
  }
}