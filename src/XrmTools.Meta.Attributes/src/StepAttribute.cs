namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Adds plugin step to a plugin type. This attribute should only be applied after a <see cref="PluginAttribute" /> to the class.
    /// <summary>
    /// Defines a plugin step for a plugin class, specifying the message, stage, execution mode, and other step configuration.
    /// The class that this attribute is applied to should have a <see cref="PluginAttribute" />, before this attribute.
    /// </summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class StepAttribute : Attribute
    {
        // Constructor (aka positional) parameters
        public ExecutionMode Mode { get; set; }
        public Stages Stage { get; }
        public string MessageName { get; }

        // Named parameters
        public string PrimaryEntityName { get; set; } = string.Empty;
        public string FilteringAttributes { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public int ExecutionOrder { get; set; } = 1;
        public string ImpersonatingUserFullname { get; set; } = string.Empty;
        public SupportedDeployments? SupportedDeployment { get; set; } = SupportedDeployments.Server;
        public PluginStepStates? State { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool CanBeBypassed { get; set; } = false;
        public bool AsyncAutoDelete { get; set; } = false;

        public StepAttribute(string messageName, Stages stage, ExecutionMode mode)
        {
            MessageName = messageName;
            Stage = stage;
            Mode = mode;
        }

        public StepAttribute(string messageName, string primaryEntityName, Stages stage, ExecutionMode mode) : this(messageName, stage, mode)
        {
            PrimaryEntityName = primaryEntityName;
        }

        public StepAttribute(string messageName, string primaryEntityName, string filteringAttributes, Stages stage, ExecutionMode mode)
            : this(messageName, primaryEntityName, stage, mode)
        { 
            FilteringAttributes = filteringAttributes;
        }
    }
}