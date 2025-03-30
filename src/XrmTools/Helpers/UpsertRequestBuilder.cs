#nullable enable
namespace XrmTools.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using XrmTools.WebApi.Messages;
using XrmTools.Xrm.Model;
using XrmTools.WebApi;
using Newtonsoft.Json.Linq;
using System.Linq;
using XrmTools.Meta.Model;

public class UpsertRequestBuilder
{
    private readonly PluginAssemblyConfig _config;
    private readonly string _base64Assembly;
    private readonly Dictionary<string, SdkMessage> _sdkMessages;
    private readonly List<HttpRequestMessage> _requests = [];

    public UpsertRequestBuilder(
        PluginAssemblyConfig config,
        string base64Assembly,
        Dictionary<string, SdkMessage> sdkMessages)
    {
        _config = config;
        _base64Assembly = base64Assembly;
        _sdkMessages = sdkMessages;
    }

    public UpsertRequestBuilder WithAssembly()
    {
        _requests.Add(new UpsertRequest(
            new EntityReference(_config.GetEntitySetName(), _config.PluginAssemblyId),
            new JObject
            {
                ["name"] = _config.Name,
                ["version"] = _config.Version,
                ["isolationmode"] = (int?)_config.IsolationMode,
                ["publickeytoken"] = _config.PublicKeyToken,
                ["sourcetype"] = (int?)_config.SourceType,
                ["content"] = _base64Assembly
            }));

        return this;
    }

    public UpsertRequestBuilder WithPluginTypes()
    {
        foreach (var pluginType in _config.PluginTypes)
        {
            _requests.Add(BuildPluginTypeRequest(pluginType));

            foreach (var step in pluginType.Steps)
            {
                if (!string.IsNullOrEmpty(step.MessageName) &&
                    _sdkMessages.TryGetValue(step.MessageName!, out var message))
                {
                    step.Message = message;
                }

                _requests.Add(CreatePluginStepRequestFor(pluginType, step));

                foreach (var image in step.Images)
                {
                    _requests.Add(CreatePluginImageRequestFor(step, image));
                }
            }
        }

        return this;
    }

    public ICollection<HttpRequestMessage> Build() => _requests;

    private HttpRequestMessage BuildPluginTypeRequest(PluginTypeConfig pluginType)
    {
        return new UpsertRequest(new EntityReference(pluginType.GetEntitySetName(), pluginType.PluginTypeId),
            new JObject
            {
                ["name"] = pluginType.Name,
                ["friendlyname"] = pluginType.FriendlyName,
                ["description"] = pluginType.Description,
                ["typename"] = pluginType.TypeName,
                ["pluginassemblyid@odata.bind"] = $"{_config.GetEntitySetName()}({_config.PluginAssemblyId})"
            });
    }

    private static HttpRequestMessage CreatePluginStepRequestFor(
        PluginTypeConfig parentPlugin, PluginStepConfig pluginStepConfig)
    {
        var pluginStep = new JObject
        {
            ["name"] = pluginStepConfig.Name,
            ["asyncautodelete"] = pluginStepConfig.AsyncAutoDelete,
            ["description"] = pluginStepConfig.Description,
            ["filteringattributes"] = pluginStepConfig.FilteringAttributes,
            ["mode"] = (int)pluginStepConfig.Mode,
            ["rank"] = pluginStepConfig.Rank ?? 0,
            ["stage"] = (int)pluginStepConfig.Stage,
            ["supporteddeployment"] = (int)(pluginStepConfig.SupportedDeployment ?? SupportedDeployments.Server),
            ["plugintypeid@odata.bind"] = $"plugintypes({parentPlugin.PluginTypeId})"
            //["plugintypeid@odata.bind"] = $"${parentPluginContentId}"
        };
        if (pluginStepConfig.Message is SdkMessage message)
        {
            pluginStep["sdkmessageid@odata.bind"] = $"sdkmessages({message.Id})";
            if (!string.IsNullOrEmpty(pluginStepConfig.PrimaryEntityName) &&
                message.Filters.FirstOrDefault(f => f.PrimaryObjectTypeCode == pluginStepConfig.PrimaryEntityName) is SdkMessageFilter filter)
            {
                pluginStep["sdkmessagefilterid@odata.bind"] = $"sdkmessagefilters({filter.Id})";
            }
        }
        return new UpsertRequest(new EntityReference(
            pluginStepConfig.GetEntitySetName(), pluginStepConfig.PluginStepId),
        //return new CreateRequest(pluginStepConfig.GetEntitySetName(),
            pluginStep);
    }

    private static HttpRequestMessage CreatePluginImageRequestFor(
        PluginStepConfig parentStep, PluginStepImageConfig pluginImage)
        => new UpsertRequest(new EntityReference(
            pluginImage.GetEntitySetName(),
            pluginImage.PluginStepImageId),
        new JObject
        {
            ["name"] = pluginImage.Name,
            ["attributes"] = pluginImage.ImageAttributes,
            ["entityalias"] = pluginImage.EntityAlias,
            ["messagepropertyname"] = parentStep.Message.MessagePropertyNames.FirstOrDefault().Name,
            ["imagetype"] = (int)pluginImage.ImageType,
            ["sdkmessageprocessingstepid@odata.bind"] = $"sdkmessageprocessingsteps({parentStep.PluginStepId})"
            //["sdkmessageprocessingstepid@odata.bind"] = $"${parentStepContentId}"
        });
}

#nullable restore