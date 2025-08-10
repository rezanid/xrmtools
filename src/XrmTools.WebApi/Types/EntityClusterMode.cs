namespace XrmTools.WebApi.Types;
using System.Runtime.Serialization;

public enum EntityClusterMode
{
    [EnumMember(Value = "Partitioned")]
    Partitioned,
    [EnumMember(Value = "Replicated")]
    Replicated,
    [EnumMember(Value = "Local")]
    Local,
    [EnumMember(Value = "FilteredReplicated")]
    FilteredReplicated
}
