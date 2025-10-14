namespace XrmTools.Helpers;

using System;
using System.IO;

public static class PathHelper
{
    /// <summary>
    /// Canonicalizes a path: resolves "." and "..", returns an absolute path,
    /// normalizes directory separators, trims trailing separators (except root),
    /// and normalizes case (uppercases drive letter on Windows).
    /// </summary>
    public static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

        // Normalize separators first so GetFullPath behaves predictably
        path = path.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        // Resolve to absolute & collapse dot-segments
        var full = Path.GetFullPath(path);

        // Normalize case conventions
        if (IsWindows())
        {
            // Uppercase drive letter on Windows (e.g., c:\ -> C:\)
            var root = Path.GetPathRoot(full);
            if (!string.IsNullOrEmpty(root) && root.Length >= 2 && root[1] == ':' && char.IsLetter(root[0]))
            {
                full = char.ToUpperInvariant(full[0]) + full.Substring(1);
            }
        }

        // Trim trailing separator unless it's a root (e.g., "C:\" or "\\server\share\")
        if (!IsPathRoot(full))
        {
            full = full.TrimEnd(Path.DirectorySeparatorChar);
        }

        return full;
    }

    /// <summary>
    /// Returns a relative path from 'fromPath' to 'toPath'.
    /// If 'fromPath' appears to be a file (or is an existing file), its directory is used.
    /// If paths are on different roots/drives, returns the absolute resolved 'toPath'.
    /// </summary>
    public static string GetRelativePath(string fromPath, string toPath)
    {
        if (string.IsNullOrWhiteSpace(fromPath)) throw new ArgumentException("fromPath is required.", nameof(fromPath));
        if (string.IsNullOrWhiteSpace(toPath)) throw new ArgumentException("toPath is required.", nameof(toPath));

        var fromResolved = ResolvePath(fromPath);
        var toResolved = ResolvePath(toPath);

        // If fromPath is a file, switch to its directory
        string baseDir = UseAsDirectory(fromResolved) ? fromResolved : (Path.GetDirectoryName(fromResolved) ?? fromResolved);

        // Different roots? Then no relative path makes sense; return absolute target
        if (!SameRoot(baseDir, toResolved))
            return toResolved;

        bool targetIsDir = UseAsDirectory(toResolved);

        // If both refer to the same directory, relative path is "."
        if (targetIsDir && PathsEqual(baseDir, toResolved))
            return ".";

        // Build file:// URIs to leverage MakeRelativeUri reliably
        var baseUri = new Uri(AppendDirectorySeparator(baseDir));                    // ensure dir semantics
        var targetUri = new Uri(targetIsDir ? AppendDirectorySeparator(toResolved)     // ensure dir semantics
                                            : toResolved);

        var relUri = baseUri.MakeRelativeUri(targetUri);
        var relative = Uri.UnescapeDataString(relUri.ToString());

        // Uri gives '/' separators; convert to OS-native if needed
        if (Path.DirectorySeparatorChar != '/')
            relative = relative.Replace('/', Path.DirectorySeparatorChar);

        // When both resolve to the same file, MakeRelativeUri may return just the file name (fine).
        // When it returns empty (rare), use "."
        return string.IsNullOrEmpty(relative) ? "." : relative;
    }

    private static bool PathsEqual(string a, string b)
    {
        var cmp = IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(a, b, cmp);
    }

    private static bool IsWindows()
        => Environment.OSVersion.Platform == PlatformID.Win32NT;

    private static bool IsPathRoot(string fullPath)
    {
        var root = Path.GetPathRoot(fullPath);
        return !string.IsNullOrEmpty(root) &&
               fullPath.Length == root.TrimEnd(Path.DirectorySeparatorChar).Length;
    }

    private static string AppendDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;

    private static bool SameRoot(string a, string b)
    {
        var ra = Path.GetPathRoot(a) ?? "";
        var rb = Path.GetPathRoot(b) ?? "";
        var cmp = IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(ra, rb, cmp);
    }

    /// <summary>
    /// Decide if a path should be treated as a directory.
    /// Prefer actual filesystem checks when available; otherwise use heuristics.
    /// </summary>
    private static bool UseAsDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) return true;
            if (File.Exists(path)) return false;
        }
        catch
        {
            // ignore IO permission issues; fall through to heuristics
        }

        // Heuristics when the path doesn't exist:
        // - Trailing separator => directory
        // - Having an extension => likely a file
        // - Otherwise assume directory
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString())) return true;
        if (Path.HasExtension(path)) return false;
        return true;
    }
}