#nullable enable
namespace XrmTools.WebApi.Entities;

using System;
using XrmTools.Meta.WebApi.Types;

public abstract class Component<T> : Entity<T> where T : Component<T>
{
    public Guid? SolutionId { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public DateTime? OverwriteTime { get; set; }
    public ComponentState? ComponentState { get; set; }
    public bool? IsManaged { get; set; }
}
#nullable restore