using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Net.Http;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmTraditionalPlugins
{
  [Plugin]
  [Step("Create", "account", "description,name,accountcategorycode",Stages.PostOperation, ExecutionMode.Asynchronous)]
  public partial class CleanAccountPostOperation : IPlugin
  {
    [Dependency]
    IOrganizationServiceFactory ServiceFactory { get => DependencyScope<CleanAccountPostOperation>.Current.Require<IOrganizationServiceFactory>(); }

    private readonly string webAddress;
    public CleanAccountPostOperation(string config, string SecureConfig)
    {
      webAddress = string.IsNullOrEmpty(config) ?
        "https://echo.free.beeceptor.com/sample-request?author=XrmTools"
        : config;
    }

    public void Execute(IServiceProvider serviceProvider)
    {
      ITracingService tracingService =
        (ITracingService)serviceProvider.GetService(typeof(ITracingService));

      ILogger logger = (ILogger)serviceProvider.GetService(typeof(ILogger));

      IPluginExecutionContext context = (IPluginExecutionContext)
        serviceProvider.GetService(typeof(IPluginExecutionContext));

      try
      {
        string startExecMsg = "Start execution of AccountPostOperation";
        logger.LogInformation(startExecMsg);
        tracingService.Trace(startExecMsg);

        if (Target.LogicalName != "account")
        {
          string wrongEntityMsg = "Plug-in registered for wrong entity {0}";
          logger.LogWarning(wrongEntityMsg, Target.LogicalName);
          tracingService.Trace(wrongEntityMsg, Target.LogicalName);
          return;
        }

        string activityMsg = "Callback";

        using (logger.BeginScope(activityMsg))
        {
          tracingService.Trace(activityMsg);

          string startTaskMsg = "Start Task Creation";
          logger.LogInformation(startTaskMsg);
          tracingService.Trace(startTaskMsg);

          Entity followup = new Entity("task");
          followup["subject"] = "Send e-mail to the new customer.";
          followup["description"] =
            "Follow up with the customer. Check if there are any new issues that need resolution.";
          followup["scheduledstart"] = DateTime.Now.AddDays(7);
          followup["scheduledend"] = DateTime.Now.AddDays(7);
          followup["category"] = context.PrimaryEntityName;

          // Refer to the account in the task activity.
          if (context.OutputParameters.Contains("id"))
          {
            Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
            string regardingobjectidType = "account";
            
            followup["regardingobjectid"] =
              new EntityReference(regardingobjectidType, regardingobjectid);
          }

          // Obtain the IOrganizationService reference.
          IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider
            .GetService(typeof(IOrganizationServiceFactory));

          IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
          //Create the task
          service.Create(followup);

          string endTaskMsg = "Task creation completed";
          logger.LogInformation(endTaskMsg);
          tracingService.Trace(endTaskMsg);
        }

        string outBoundScope = "OutboundCall";

        using (logger.BeginScope(outBoundScope))
        {
          string outboundStartMsg = "Outbound call started";
          logger.LogInformation(outboundStartMsg);
          tracingService.Trace(outboundStartMsg);

          using (HttpClient client = new HttpClient())
          {
            client.Timeout = TimeSpan.FromMilliseconds(15000); //15 seconds
            client.DefaultRequestHeaders.ConnectionClose = true; //Set KeepAlive to false

            HttpResponseMessage response = client
              .GetAsync(webAddress)
              .GetAwaiter()
              .GetResult(); //Make sure it is synchronous

            response.EnsureSuccessStatusCode();

            string responseText = response.Content
              .ReadAsStringAsync()
              .GetAwaiter()
              .GetResult(); //Make sure it is synchronous

            string shortResponseText = responseText.Substring(0, 20);

            logger.LogInformation(shortResponseText);
            tracingService.Trace(shortResponseText);

            string outboundEndMsg = "Outbound call ended successfully";

            logger.LogInformation(outboundEndMsg);
            tracingService.Trace(outboundEndMsg);
            
          }
        }
      }
      catch (Exception e)
      {
        string errMsg = "Plugin failed";
        logger.LogError(e, errMsg);
        tracingService.Trace($"{errMsg}:{e.Message}");
        throw new InvalidPluginExecutionException(e.Message, e);
      }
    }
  }
}
