using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.IO;

namespace XrmTools;

public class Validation
{
    public static DTE2 DTE = Package.GetGlobalService(typeof(DTE)) as DTE2;

    public static bool IsSupportedFile(string allowedExtension)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var doc = DTE.ActiveDocument;
        return (doc != null && !string.IsNullOrEmpty(doc.FullName) || Path.GetFileName(doc.FullName).EndsWith(allowedExtension));
    }
}
