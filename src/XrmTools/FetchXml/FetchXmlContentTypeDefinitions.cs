#nullable enable
namespace XrmTools.FetchXml;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

/// <summary>
/// Declares the Fetch XML content type and file extensions (.fetch / .fetchxml).
/// Based on the built-in XML content type so editors (colorizer, etc.) still apply.
/// </summary>
internal static class FetchXmlContentTypeDefinitions
{
    public const string ContentTypeName = "FetchXml"; // internal reference

    [Export]
    [Name(ContentTypeName)]
    [BaseDefinition("XML")] // Inherit from XML so we get the XML editor stack
    internal static readonly ContentTypeDefinition FetchXmlContentType = null!; // MEF populated

    [Export]
    [FileExtension(".fetch")]
    //[ContentType("XML")]
    [ContentType(ContentTypeName)]
    internal static readonly FileExtensionToContentTypeDefinition FetchExtensionDefinition = null!;

    [Export]
    [FileExtension(".fetchxml")] 
    [ContentType(ContentTypeName)]
    internal static readonly FileExtensionToContentTypeDefinition FetchXmlExtensionDefinition = null!;
}
#nullable restore
