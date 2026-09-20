#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using System;
using System.ComponentModel;

internal sealed class PackageNode : ExplorerNodeBaseWithDates
{
    [ReadOnly(true)]
    public Guid PackageId { get; set; }
    [ReadOnly(true)]
    public string? Version { get; set; }
    [Browsable(false)]
    public override string ArtifactCategory => "Packages";
}
