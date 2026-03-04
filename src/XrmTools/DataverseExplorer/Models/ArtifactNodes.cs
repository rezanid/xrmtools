#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.ComponentModel;

/// <summary>
/// Represents a Plugin Assembly node in the tree.
/// Can be expanded to show Plugin Types and their Steps/Images.
/// </summary>
internal sealed class AssemblyNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid AssemblyId { get; set; }
    [ReadOnly(true)]
    public string? PublicKeyToken { get; set; }
    [ReadOnly(true)]
    public string? Version { get; set; }
    [ReadOnly(true)]
    public string? IsolationMode { get; set; }
    [ReadOnly(true)]
    public string? SourceType { get; set; }

    /// <summary>
    /// Indicates whether child plugin types have been loaded from Dataverse.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }

    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Type (class) node in the tree.
/// Can be expanded to show its Plugin Steps and their Images.
/// </summary>
public class PluginTypeNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid PluginTypeId { get; set; }
    [ReadOnly(true)]
    public string? TypeName { get; set; }
    [ReadOnly(true)]
    public string? FriendlyName { get; set; }
    [ReadOnly(true)]
    public string? WorkflowActivityGroupName { get; set; }

    /// <summary>
    /// Indicates whether child steps and images have been loaded from Dataverse.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }

    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Step (SDK Message Processing Step) node in the tree.
/// Can be expanded to show its attached Images.
/// </summary>
public class PluginStepNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid StepId { get; set; }
    [ReadOnly(true)]
    public string? Stage { get; set; }
    [ReadOnly(true)]
    public string? Mode { get; set; }
    [ReadOnly(true)]
    public int? Rank { get; set; }
    [ReadOnly(true)]
    public string? SdkMessageId { get; set; }
    [ReadOnly(true)]
    public string? StateCode { get; set; }
    [ReadOnly(true)]
    public bool? AsyncAutoDelete { get; set; }
    [ReadOnly(true)]
    public string? FilteringAttributes { get; set; }
    [ReadOnly(true)]
    public string? InvocationSource { get; set; }
    [ReadOnly(true)]
    public string? SupportedDeployment { get; set; }

    /// <summary>
    /// Indicates whether child images have been loaded.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }

    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Image (Step Image for data context snapshot) node in the tree.
/// This is a leaf node with no children.
/// </summary>
public class PluginImageNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid ImageId { get; set; }
    [ReadOnly(true)]
    public string? ImageType { get; set; }
    [ReadOnly(true)]
    public string? MessagePropertyName { get; set; }
    [ReadOnly(true)]
    public string? Attributes { get; set; }
    [ReadOnly(true)]
    public string? EntityAlias { get; set; }

    [Browsable(false)]
    public override bool CanLoadChildren => false;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Category node that groups artifacts by type (e.g., "Assemblies", "Cloud Flows").
/// Acts as a root node for expandable artifact categories.
/// </summary>
public class CategoryNode : ExplorerNodeBase
{
    private string _artifactCategory = string.Empty;

    /// <summary>
    /// Gets the artifact category this node represents.
    /// </summary>
    public override string ArtifactCategory => _artifactCategory;

    /// <summary>
    /// Sets the artifact category for this node.
    /// </summary>
    public void SetArtifactCategory(string category)
    {
        _artifactCategory = category;
    }

    /// <summary>
    /// Indicates whether child artifacts of this category have been loaded.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }

    public override bool CanLoadChildren => !AreChildrenLoaded;
}

/// <summary>
/// Represents a Custom API node in the tree.
/// </summary>
public class CustomApiNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid CustomApiId { get; set; }
    [ReadOnly(true)]
    public string? Name { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }
    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
    [Browsable(false)]
    public string? TypeName { get; set; }
}

internal class CustomApiParameterNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid ParameterId { get; set; }
    [ReadOnly(true)]
    public string? Name { get; set; }
    [ReadOnly(true)]
    public string? ParameterType { get; set; }
    [ReadOnly(true)]
    public bool IsOptional { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }
    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}


internal class CustomApiResponseNode : ExplorerNodeBase
{
    [ReadOnly(true)]
    public Guid ResponseId { get; set; }
    [ReadOnly(true)]
    public string? Name { get; set; }
    [ReadOnly(true)]
    public string? PropertyType { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    [Browsable(false)]
    public bool AreChildrenLoaded { get; set; }
    [Browsable(false)]
    public override bool CanLoadChildren => !AreChildrenLoaded;
    [Browsable(false)]
    public override string ArtifactCategory => "Assemblies";
}

#nullable restore
