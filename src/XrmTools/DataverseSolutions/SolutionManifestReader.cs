#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

internal static class SolutionManifestReader
{
    public static string ReadUniqueName(string solutionRootPath)
    {
        if (string.IsNullOrWhiteSpace(solutionRootPath))
        {
            throw new ArgumentException("The solution root path is required.", nameof(solutionRootPath));
        }

        var manifestPath = Path.Combine(solutionRootPath, "Other", "Solution.xml");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException($"The solution manifest was not found at '{manifestPath}'.");
        }

        using var reader = XmlReader.Create(manifestPath, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        var document = XDocument.Load(reader);
        var uniqueName = document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "UniqueName")?
            .Value
            .Trim();

        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            throw new InvalidOperationException($"The solution manifest '{manifestPath}' does not contain a solution unique name.");
        }

        return uniqueName;
    }
}
#nullable restore
