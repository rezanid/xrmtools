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

    public enum Stages
    {
        /// <summary>
        /// pre-validation (not in transaction).
        /// </summary>
        PreValidation = 10,
        /// <summary>
        /// pre-operation (in transaction, ownerid cannot be changed).
        /// </summary>
        PreOperation = 20,
        //
        // Summary:
        //     
        /// <summary>
        /// Main operation (in transaction, only used in Custom APIs).
        /// </summary>
        MainOperation = 30,
        /// <summary>
        /// post-operation (operation executed, but still in transaction).
        /// </summary>
        PostOperation = 40,
        /// <summary>
        /// post-operation, deprecated.
        /// </summary>
        [Obsolete("Deprecated according to Microsoft.", true)]
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