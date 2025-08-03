namespace XrmTools.WebApi.Entities;

using System;
using XrmTools.WebApi.Types;

public sealed class SecurityPrivilegeMetadata
{
    public bool CanBeBasic { get; set; }

    public bool CanBeDeep { get; set; }

    public bool CanBeGlobal { get; set; }

    public bool CanBeLocal { get; set; }

    public bool CanBeEntityReference { get; set; }

    public bool CanBeParentEntityReference { get; set; }

    public bool CanBeRecordFilter { get; set; }

    public string Name { get; set; }

    public Guid PrivilegeId { get; set; }

    public PrivilegeType PrivilegeType { get; set; }
}
