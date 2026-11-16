#nullable enable
namespace XrmTools.Pcf.Setup;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <param name="Node">The Node.js the new Visual Studio window gets first on its PATH.</param>
/// <param name="CloseThisWindow">Close the current window once the new one has started.</param>
internal sealed record PcfInstanceRequest(NodeInstallation Node, bool CloseThisWindow);

/// <summary>
/// Opens the current solution in a new Visual Studio window whose PATH starts with a chosen Node.js. A build runs with
/// the environment of its Visual Studio instance, so this is how a build gets a Node.js the current instance does not
/// have. The current instance's environment is not changed.
/// </summary>
internal interface IPcfInstanceLauncher
{
    /// <returns><c>false</c> when the developer canceled a save prompt, in which case nothing was started or closed.</returns>
    Task<bool> LaunchAsync(PcfInstanceRequest request);
}

[Export(typeof(IPcfInstanceLauncher))]
internal sealed class PcfInstanceLauncher : IPcfInstanceLauncher
{
    public async Task<bool> LaunchAsync(PcfInstanceRequest request)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var services = ServiceProvider.GlobalProvider;
        var solution = services.GetService(typeof(SVsSolution)) as IVsSolution ?? throw new InvalidOperationException("Visual Studio's solution service is unavailable.");
        ErrorHandler.ThrowOnFailure(solution.GetSolutionInfo(out _, out var solutionFile, out _));
        if (string.IsNullOrEmpty(solutionFile) || !File.Exists(solutionFile))
            throw new InvalidOperationException("Save the solution first, so that the new Visual Studio window can open it.");
        // The same installation and the same instance (for example /rootsuffix Exp) as this window.
        string? rootSuffix = null;
        if (services.GetService(typeof(SVsAppCommandLine)) is IVsAppCommandLine commandLine
            && ErrorHandler.Succeeded(commandLine.GetOption("rootsuffix", out var present, out var value)) && present != 0)
            rootSuffix = value;
        string devenv;
        using (var current = Process.GetCurrentProcess()) devenv = current.MainModule.FileName;

        if (request.CloseThisWindow)
        {
            // Visual Studio asks about unsaved changes here. Cancel leaves everything as it was.
            var result = solution.CloseSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_PromptSave, null, 0);
            if (result == VSConstants.OLE_E_PROMPTSAVECANCELLED) return false;
            ErrorHandler.ThrowOnFailure(result);
        }
        try
        {
            PcfInstanceProcess.Start(devenv, PcfInstanceProcess.Arguments(rootSuffix, solutionFile),
                PcfInstanceProcess.EnvironmentWith(Environment.GetEnvironmentVariables(), request.Node.PathEntries), Path.GetDirectoryName(solutionFile)!);
        }
        catch (Win32Exception) when (request.CloseThisWindow)
        {
            // Nothing new was started, so put the solution back where the developer had it.
            solution.OpenSolutionFile(0, solutionFile);
            throw;
        }
        if (request.CloseThisWindow && services.GetService(typeof(SVsUIShell)) is IVsUIShell shell)
        {
            // File > Exit, queued so that this call returns first; it still asks about anything else that is unsaved.
            var group = VSConstants.GUID_VSStandardCommandSet97;
            object? argument = null;
            shell.PostExecCommand(ref group, (uint)VSConstants.VSStd97CmdID.Exit, 0, ref argument);
        }
        return true;
    }
}

/// <summary>Starts a process with an explicit environment and without handing it any of this process's handles.</summary>
internal static class PcfInstanceProcess
{
    public static string Arguments(string? rootSuffix, string document)
        => (string.IsNullOrWhiteSpace(rootSuffix) ? string.Empty : $"/rootsuffix {rootSuffix!.Trim()} ") + "\"" + document + "\"";

    /// <summary>A copy of <paramref name="current"/> in which PATH starts with <paramref name="pathEntries"/>.</summary>
    public static IDictionary<string, string> EnvironmentWith(IDictionary current, IEnumerable<string> pathEntries)
    {
        var variables = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry entry in current) variables[(string)entry.Key] = (string?)entry.Value ?? string.Empty;
        // Windows names it "Path"; keep whatever spelling is already there.
        var name = variables.Keys.FirstOrDefault(key => string.Equals(key, "PATH", StringComparison.OrdinalIgnoreCase)) ?? "PATH";
        variables.TryGetValue(name, out var path);
        variables[name] = string.Join(Path.PathSeparator.ToString(), pathEntries) + Path.PathSeparator + path;
        return variables;
    }

    public static string EnvironmentBlock(IDictionary<string, string> variables)
    {
        var block = new StringBuilder();
        foreach (var variable in variables.OrderBy(variable => variable.Key, StringComparer.OrdinalIgnoreCase))
            block.Append(variable.Key).Append('=').Append(variable.Value).Append('\0');
        return block.Append('\0').ToString();
    }

    /// <returns>The id of the started process.</returns>
    public static int Start(string application, string arguments, IDictionary<string, string> environment, string workingDirectory)
    {
        var startup = new StartupInfo { Size = Marshal.SizeOf<StartupInfo>() };
        var commandLine = new StringBuilder("\"" + application + "\" " + arguments);
        // Handles are not inherited: a second Visual Studio must not keep this one's pipes and files open.
        if (!CreateProcess(application, commandLine, IntPtr.Zero, IntPtr.Zero, false, CreateUnicodeEnvironment,
                Encoding.Unicode.GetBytes(EnvironmentBlock(environment)), workingDirectory, ref startup, out var process))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"'{application}' could not be started: {new Win32Exception(error).Message}");
        }
        CloseHandle(process.Thread);
        CloseHandle(process.Process);
        return process.ProcessId;
    }

    private const uint CreateUnicodeEnvironment = 0x00000400;

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfo
    {
        public int Size;
        public IntPtr Reserved, Desktop, Title;
        public int X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
        public short ShowWindow, Reserved2Size;
        public IntPtr Reserved2, StdInput, StdOutput, StdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr Process, Thread;
        public int ProcessId, ThreadId;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcess(string application, StringBuilder commandLine, IntPtr processAttributes, IntPtr threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles, uint creationFlags, byte[] environment, string currentDirectory,
        ref StartupInfo startupInfo, out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);
}
#nullable restore
