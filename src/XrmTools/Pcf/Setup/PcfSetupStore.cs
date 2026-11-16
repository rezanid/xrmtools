#nullable enable
namespace XrmTools.Pcf.Setup;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

internal sealed class PcfNodeSelection
{
    public string NodeDirectory { get; set; } = string.Empty;
    /// <summary>The version at the time of selection; shown when the installation has since disappeared.</summary>
    public string? Version { get; set; }
}

/// <summary>
/// Per-user, per-machine PCF setup state. Nothing here is written into the project or its repository, because a Node.js
/// installation path is only meaningful on the machine where it was chosen.
/// </summary>
internal interface IPcfSetupStore
{
    PcfNodeSelection? GetSelection(string projectFile);
    void SetSelection(string projectFile, PcfNodeSelection? selection);
    bool IsSetupIncomplete(string projectDirectory);
    void SetSetupIncomplete(string projectDirectory, bool incomplete);
    /// <returns><c>null</c> when another setup for the same project is running, in this or another Visual Studio.</returns>
    IDisposable? TryLock(string projectDirectory);
    bool IsLocked(string projectDirectory);
}

[Export(typeof(IPcfSetupStore))]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfSetupStore : IPcfSetupStore
{
    private readonly string _root;
    private readonly object _sync = new();
    private readonly HashSet<string> _held = new(StringComparer.OrdinalIgnoreCase);

    public PcfSetupStore() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XrmTools", "Pcf")) { }

    internal PcfSetupStore(string root) => _root = root;

    private string SelectionsFile => Path.Combine(_root, "node-selections.json");

    public PcfNodeSelection? GetSelection(string projectFile)
    {
        lock (_sync) return ReadSelections().TryGetValue(Key(projectFile), out var selection) ? selection : null;
    }

    public void SetSelection(string projectFile, PcfNodeSelection? selection)
    {
        lock (_sync)
        {
            var selections = ReadSelections();
            if (selection is null) selections.Remove(Key(projectFile)); else selections[Key(projectFile)] = selection;
            Directory.CreateDirectory(_root);
            // Replace the file in one step so an interrupted write cannot leave it half written.
            var temporary = SelectionsFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(selections, Formatting.Indented));
            if (File.Exists(SelectionsFile)) File.Replace(temporary, SelectionsFile, null); else File.Move(temporary, SelectionsFile);
        }
    }

    public bool IsSetupIncomplete(string projectDirectory) => File.Exists(StateFile(projectDirectory, "incomplete"));

    public void SetSetupIncomplete(string projectDirectory, bool incomplete)
    {
        var file = StateFile(projectDirectory, "incomplete");
        try
        {
            if (!incomplete) { File.Delete(file); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, Path.GetFullPath(projectDirectory));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public IDisposable? TryLock(string projectDirectory)
    {
        var file = StateFile(projectDirectory, "lock");
        lock (_sync)
        {
            if (!_held.Add(file)) return null;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                // Windows releases this handle when the process ends, so a crash or kill cannot strand the lock.
                var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
                return new Releaser(() => { stream.Dispose(); lock (_sync) _held.Remove(file); });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _held.Remove(file);
                return null;
            }
        }
    }

    public bool IsLocked(string projectDirectory)
    {
        using var probe = TryLock(projectDirectory);
        return probe is null;
    }

    private Dictionary<string, PcfNodeSelection> ReadSelections()
    {
        try
        {
            if (File.Exists(SelectionsFile))
                return new Dictionary<string, PcfNodeSelection>(
                    JsonConvert.DeserializeObject<Dictionary<string, PcfNodeSelection>>(File.ReadAllText(SelectionsFile)) ?? [], StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        return new Dictionary<string, PcfNodeSelection>(StringComparer.OrdinalIgnoreCase);
    }

    private string StateFile(string projectDirectory, string extension)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Key(projectDirectory)));
        return Path.Combine(_root, "setup", BitConverter.ToString(hash, 0, 12).Replace("-", string.Empty) + "." + extension);
    }

    private static string Key(string path) => Path.GetFullPath(path).TrimEnd('\\', '/').ToUpperInvariant();

    private sealed class Releaser(Action release) : IDisposable
    {
        private Action? _release = release;
        public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
    }
}
#nullable restore
