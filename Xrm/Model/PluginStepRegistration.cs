using System;
using System.Collections.Generic;

namespace XrmGen.Xrm.Model;

public class PluginStepRegistration
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UnsecureConfig { get; set; }
    public string SecureConfig { get; set; }
    public string MessageName { get; set; }
    public string PrimaryEntityName { get; set; }
    public bool AsyncAutoDelete { get; set; }
    public bool ImpersonatingUserFullName { get; set; }
    public string CustomConfiguration { get; set; }
    public string FilteringAttributes { get; set; }
    public int InvocationSource { get; set; }
    public int Mode { get; set; }
    public int Rank { get; set; }
    public int Stage { get; set; }
    public string StageName
    {
        get => Stage switch
        {
            10 => "PreValidation",
            20 => "PreOperation",
            30 => "PostOperation",
            _ => throw new InvalidOperationException($"Unknown stage: {Stage}")
        };
    } 
    public int SupportedDeployment { get; set; }
    public Guid SdkMessageId { get; set; }
    public int StateCode { get; set; }
    public IEnumerable<PluginStepImageRegistration> Images { get; set; }
}