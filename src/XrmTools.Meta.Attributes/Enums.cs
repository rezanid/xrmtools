namespace XrmTools.Meta.Attributes
{
    using System;

    public enum ProcessingStepTypes
    {
        None = 0,
        AsyncOnly = 1,
        SyncAndAsync = 2,
    }

    public enum PluginStepStates
    {
        Active = 0,
        Inactive = 1
    }

    /// <summary>
    /// Read more about the stages of a plugin execution pipeline at https://learn.microsoft.com/en-us/power-apps/developer/data-platform/event-framework#event-execution-pipeline
    /// </summary>
    public enum Stages
    {
        /// <summary>
        /// This provides an opportunity to include logic to cancel the operation before the database transaction.
        /// </summary>
        PreValidation = 10,
        /// <summary>
        /// Occurs before the main system operation and within the database transaction. (ownerid cannot be changed).
        /// </summary>
        PreOperation = 20,
        /// <summary>
        /// Main operation (custom API and Custom virtual table data providers).
        /// </summary>
        MainOperation = 30,
        /// <summary>
        /// Occurs after the main system operation and within the database transaction. Use this stage to modify any properties of the message before it is returned to the caller.
        /// Avoid applying changes to an entity included in the message because this will trigger a new Update event.
        /// </summary>
        PostOperation = 40,
        /// <summary>
        /// post-operation, deprecated.
        /// </summary>
        [Obsolete("Not in use anymore according to Microsoft.", true)]
        DepecratedPostOperation = 50
    }

    public enum BindingTypes
    {
        Global = 0,
        Entity = 1,
        EntityCollection = 2,
    }

    public enum CustomApiFieldType
    {
        Boolean = 0,
        DateTime = 1,
        Decimal = 2,
        Entity = 3,
        EntityCollection = 4,
        EntityReference = 5,
        Float = 6,
        Integer = 7,
        Money = 8,
        Picklist = 9,
        String = 10,
        StringArray = 11,
        Guid = 12,
    }

    public enum SupportedDeployments
    {
        Server = 0,
        Client = 1,
        Both = 2
    }

    public enum ExecutionMode
    {
        Synchronous = 0,
        Asynchronous = 1
    }

    public enum ImageTypes
    {
        PreImage = 0,
        PostImage = 1,
        Both = 2
    }

    public enum IsolationModes
    {
        [Obsolete]
        None = 1,
        Sandbox = 2,
        [Obsolete]
        External = 3
    }

    public enum SourceTypes
    {
        Database = 0,
        Disk = 1,
        Normal = 2,
        AzureWebApp = 3,
        FileStore = 4
    }
}