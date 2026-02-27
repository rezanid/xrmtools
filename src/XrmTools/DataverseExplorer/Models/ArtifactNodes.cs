#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using Microsoft.VisualStudio.Imaging.Interop;
using System;

/// <summary>
/// Represents a Plugin Assembly node in the tree.
/// Can be expanded to show Plugin Types and their Steps/Images.
/// </summary>
public class AssemblyNode : ExplorerNodeBase
{
    public Guid AssemblyId { get; set; }
    public string? PublicKeyToken { get; set; }
    public string? Version { get; set; }
    public string? IsolationMode { get; set; }
    public string? SourceType { get; set; }

    /// <summary>
    /// Indicates whether child plugin types have been loaded from Dataverse.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }

    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Type (class) node in the tree.
/// Can be expanded to show its Plugin Steps and their Images.
/// </summary>
public class PluginTypeNode : ExplorerNodeBase
{
    public Guid PluginTypeId { get; set; }
    public string? TypeName { get; set; }
    public string? FriendlyName { get; set; }
    public string? WorkflowActivityGroupName { get; set; }

    /// <summary>
    /// Indicates whether child steps and images have been loaded from Dataverse.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }

    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Step (SDK Message Processing Step) node in the tree.
/// Can be expanded to show its attached Images.
/// </summary>
public class PluginStepNode : ExplorerNodeBase
{
    public Guid StepId { get; set; }
    public string? Stage { get; set; }
    public string? Mode { get; set; }
    public int? Rank { get; set; }
    public string? SdkMessageId { get; set; }
    public string? StateCode { get; set; }
    public bool? AsyncAutoDelete { get; set; }
    public string? FilteringAttributes { get; set; }
    public string? InvocationSource { get; set; }
    public string? SupportedDeployment { get; set; }

    /// <summary>
    /// Indicates whether child images have been loaded.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }

    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}

/// <summary>
/// Represents a Plugin Image (Step Image for data context snapshot) node in the tree.
/// This is a leaf node with no children.
/// </summary>
public class PluginImageNode : ExplorerNodeBase
{
    public Guid ImageId { get; set; }
    public string? ImageType { get; set; }
    public string? MessagePropertyName { get; set; }
    public string? Attributes { get; set; }
    public string? EntityAlias { get; set; }

    public override bool CanLoadChildren => false;
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
    public Guid CustomApiId { get; set; }
    public string? Name { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }
    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}

internal class CustomApiParameterNode : ExplorerNodeBase
{
    public Guid ParameterId { get; set; }
    public string? Name { get; set; }
    public string? ParameterType { get; set; }
    public bool IsOptional { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }
    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}


internal class CustomApiResponseNode : ExplorerNodeBase
{
    public Guid ResponseId { get; set; }
    public string? Name { get; set; }
    public string? PropertyType { get; set; }
    /// <summary>
    /// Indicates whether child custom API request parameters have been loaded.
    /// </summary>
    public bool AreChildrenLoaded { get; set; }
    public override bool CanLoadChildren => !AreChildrenLoaded;
    public override string ArtifactCategory => "Assemblies";
}

#nullable restore
