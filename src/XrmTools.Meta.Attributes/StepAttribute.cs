namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Adds plugin step to a plugin type. This attribute should only be applied after a <see cref="PluginAttribute" /> to the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class StepAttribute : Attribute
    {
        // Constructor (aka positional) parameters
        public ExecutionMode Mode { get; set; }
        public Stages Stage { get; }
        public string MessageName { get; }

        // Named parameters
        public string PrimaryEntityName { get; set; }
        public string FilteringAttributes { get; set; }
        public string Id { get; set; }
        public int ExecutionOrder { get; set; } = 1;
        public string ImpersonatingUserFullname { get; set; }
        public SupportedDeployments? SupportedDeployment { get; set; } = SupportedDeployments.Server;
        public PluginStepStates? State { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Configuration { get; set; }
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