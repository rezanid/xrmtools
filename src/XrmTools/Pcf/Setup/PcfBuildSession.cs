#nullable enable
namespace XrmTools.Pcf.Setup;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

/// <summary>
/// Owns the only place where Xrm Tools changes Visual Studio's process environment. A selected Node.js installation is
/// put first on PATH for the duration of one solution build and the original value is restored when that build ends,
/// so every MSBuild node of the build sees the same value and nothing outlives the build.
/// </summary>
[Export]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfBuildSession
{
    private readonly object _sync = new();
    private readonly Func<string> _read;
    private readonly Action<string> _write;
    private string? _original;
    private string? _activated;
    private bool _building;

    public PcfBuildSession() : this(() => Environment.GetEnvironmentVariable("PATH") ?? string.Empty, value => Environment.SetEnvironmentVariable("PATH", value)) { }

    internal PcfBuildSession(Func<string> read, Action<string> write) { _read = read; _write = write; }

    public bool IsBuilding
    {
        get { lock (_sync) return _building; }
    }

    /// <summary>The PATH Visual Studio was started with, even while a build has a selection activated.</summary>
    public string BasePath
    {
        get { lock (_sync) return _original ?? _read(); }
    }

    /// <summary>Call before inspecting projects for a build, so a setup that starts afterwards sees the build.</summary>
    public void BeginBuild()
    {
        lock (_sync)
        {
            RestorePath();
            _building = true;
        }
    }

    public void Activate(IEnumerable<string> pathEntries)
    {
        lock (_sync)
        {
            RestorePath();
            var entries = pathEntries.ToArray();
            if (entries.Length == 0) return;
            _original = _read();
            _activated = string.Join(Path.PathSeparator.ToString(), entries) + Path.PathSeparator + _original;
            _write(_activated);
        }
    }

    public void EndBuild()
    {
        lock (_sync)
        {
            _building = false;
            RestorePath();
        }
    }

    private void RestorePath()
    {
        if (_original is null) return;
        // Leave PATH alone if something else changed it during the build; overwriting would discard that change.
        if (string.Equals(_read(), _activated, StringComparison.Ordinal)) _write(_original);
        _original = null;
        _activated = null;
    }
}
#nullable restore
