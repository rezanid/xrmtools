#nullable enable
namespace XrmTools.WebApi.Entities;

using System;

public abstract class Component<T> : Entity<T> where T : Component<T>
{
    public Guid? SolutionId { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public DateTime? OverwriteTime { get; set; }
    public int? ComponentState { get; set; }
    public bool? IsManaged { get; set; }
    public string? Version { get; set; }
}
#nullable restore