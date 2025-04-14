﻿#nullable enable
namespace XrmTools.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using XrmTools.WebApi.Messages;
using XrmTools.Xrm.Model;
using XrmTools.WebApi;
using Newtonsoft.Json.Linq;
using System.Linq;
using XrmTools.Meta.Model;
using XrmTools.WebApi.Entities;

public class UpsertRequestBuilder(
    PluginAssemblyConfig config,
    string base64Assembly,
    Dictionary<string, SdkMessage> sdkMessages)
{
    private readonly PluginAssemblyConfig _config = config;
    private readonly string _base64Assembly = base64Assembly;
    private readonly Dictionary<string, SdkMessage> _sdkMessages = sdkMessages;
    private readonly List<HttpRequestMessage> _requests = [];

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
            },
            solutionUniqueName: _config.Solution?.UniqueName));

        return this;
    }

    public UpsertRequestBuilder WithPluginTypes()
    {
        foreach (var pluginType in _config.PluginTypes)
        {
            _requests.Add(UpsertPluginTypeRequestFor(pluginType));

            foreach (var step in pluginType.Steps)
            {
                if (!string.IsNullOrEmpty(step.MessageName) &&
                    _sdkMessages.TryGetValue(step.MessageName!, out var message))
                {
                    step.Message = message;
                }

                _requests.Add(UpsertPluginStepRequestFor(pluginType, step));

                foreach (var image in step.Images)
                {
                    _requests.Add(UpsertPluginImageRequestFor(step, image));
                }
            }

            if (pluginType.CustomApi != null)
            {
                _requests.Add(UpsertCustomApiFor(pluginType, pluginType.CustomApi));

                foreach (var parameter in pluginType.CustomApi.RequestParameters)
                {
                    _requests.Add(UpsertRequestParameter(pluginType.CustomApi, parameter));
                }
                foreach (var parameter in pluginType.CustomApi.ResponseProperties)
                {
                    _requests.Add(UpsertResponseParameter(pluginType.CustomApi, parameter));
                }
            }
        }

        return this;
    }

    public ICollection<HttpRequestMessage> Build() => _requests;

    private HttpRequestMessage UpsertPluginTypeRequestFor(PluginTypeConfig pluginType)
    {
        return new UpsertRequest(new EntityReference(pluginType.GetEntitySetName(), pluginType.PluginTypeId),
            new JObject
            {
                ["name"] = pluginType.Name,
                ["friendlyname"] = pluginType.FriendlyName,
                ["description"] = pluginType.Description,
                ["typename"] = pluginType.TypeName,
                ["pluginassemblyid@odata.bind"] = $"{_config.GetEntitySetName()}({_config.PluginAssemblyId})"
            },
            solutionUniqueName: _config.Solution?.UniqueName);
    }

    private HttpRequestMessage UpsertPluginStepRequestFor(
        PluginTypeConfig parentPlugin, PluginStepConfig pluginStepConfig)
    {
        var pluginStep = new JObject
        {
            ["name"] = pluginStepConfig.Name,
            ["asyncautodelete"] = pluginStepConfig.AsyncAutoDelete,
            ["description"] = pluginStepConfig.Description,
            ["filteringattributes"] = pluginStepConfig.FilteringAttributes,
            ["mode"] = (int?)pluginStepConfig.Mode,
            ["rank"] = pluginStepConfig.Rank ?? 0,
            ["stage"] = (int?)pluginStepConfig.Stage,
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
            pluginStep,
            solutionUniqueName: _config.Solution?.UniqueName);
    }

    private HttpRequestMessage UpsertPluginImageRequestFor(PluginStepConfig parentStep, PluginStepImageConfig pluginImage)
        => new UpsertRequest(new EntityReference(
            pluginImage.GetEntitySetName(),
            pluginImage.PluginStepImageId),
        new JObject
        {
            ["name"] = pluginImage.Name,
            ["attributes"] = pluginImage.ImageAttributes,
            ["entityalias"] = pluginImage.EntityAlias,
            ["messagepropertyname"] = parentStep.Message?.MessagePropertyNames.FirstOrDefault().Name,
            ["imagetype"] = (int)pluginImage.ImageType,
            ["sdkmessageprocessingstepid@odata.bind"] = $"sdkmessageprocessingsteps({parentStep.PluginStepId})"
            //["sdkmessageprocessingstepid@odata.bind"] = $"${parentStepContentId}"
        },
        solutionUniqueName: _config.Solution?.UniqueName);

    public HttpRequestMessage UpsertCustomApiFor(PluginTypeConfig parentPlugin, CustomApi customApi)
        => new UpsertRequest(
            customApi.ToReference(),
            new JObject
            {
                ["name"] = customApi.Name,
                ["displayname"] = customApi.DisplayName,
                ["uniquename"] = customApi.UniqueName,
                ["description"] = customApi.Description,
                ["isfunction"] = customApi.IsFunction,
                ["bindingtype"] = (int?)customApi.BindingType,
                ["executeprivilegename"] = customApi.ExecutePrivilegeName,
                ["boundentitylogicalname"] = customApi.BoundEntityLogicalName,
                ["isprivate"] = customApi.IsPrivate,
                ["allowedcustomprocessingsteptype"] = (int?)customApi.StepType
            },
            solutionUniqueName: _config.Solution?.UniqueName);

    public  HttpRequestMessage UpsertRequestParameter(CustomApi customApi, CustomApiRequestParameter parameter)
    {
        var record = new JObject
        {
            ["name"] = parameter.Name,
            ["displayname"] = parameter.DisplayName,
            ["uniquename"] = parameter.UniqueName,
            ["type"] = (int?)parameter.Type,
            ["isoptional"] = parameter.IsOptional,
            ["CustomAPIId@odata.bind"] = customApi.ToReference().Path,
        };
        if (parameter.Description is not null) record["description"] = parameter.Description;
        if (parameter.LogicalEntityName is not null) record["logicalentityname"] = parameter.LogicalEntityName;
        return  new UpsertRequest(parameter.ToReference(), record, solutionUniqueName: _config.Solution?.UniqueName);
    }

    public HttpRequestMessage UpsertResponseParameter(CustomApi customApi, CustomApiResponseProperty parameter)
    {
        var record = new JObject
        {
            ["name"] = parameter.Name,
            ["displayname"] = parameter.DisplayName,
            ["uniquename"] = parameter.UniqueName,
            ["type"] = (int?)parameter.Type,
            ["CustomAPIId@odata.bind"] = customApi.ToReference().Path,
        };
        if (parameter.Description is not null) record["description"] = parameter.Description;
        if (parameter.LogicalEntityName is not null) record["logicalentityname"] = parameter.LogicalEntityName;
        return new UpsertRequest(parameter.ToReference(), record, solutionUniqueName: _config.Solution?.UniqueName);
    }
}
#nullable restore