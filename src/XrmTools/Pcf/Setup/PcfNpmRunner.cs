#nullable enable
namespace XrmTools.Pcf.Setup;

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;

internal sealed class PcfNpmResult
{
    public int ExitCode { get; init; }
    public IReadOnlyList<string> Output { get; init; } = [];
    public bool Succeeded => ExitCode == 0;
}

internal interface IPcfNpmRunner
{
    /// <summary>Runs npm in the project directory with the given installation first on the child's PATH.</summary>
    Task<PcfNpmResult> RunAsync(NodeInstallation node, string projectDirectory, string command, IProgress<string> output, CancellationToken cancellationToken);
}

[Export(typeof(IPcfNpmRunner))]
internal sealed class PcfNpmRunner : IPcfNpmRunner
{
    public async Task<PcfNpmResult> RunAsync(NodeInstallation node, string projectDirectory, string command, IProgress<string> output, CancellationToken cancellationToken)
    {
        var launcher = node.NpmLauncher ?? throw new InvalidOperationException("The selected Node.js installation has no npm.");
        var batch = Path.GetExtension(launcher).ToLowerInvariant() is ".cmd" or ".bat";
        var info = new ProcessStartInfo(batch ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : launcher)
        {
            // npm reads the project's own .npmrc, registry and script settings from here; no option overrides them.
            WorkingDirectory = projectDirectory,
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        if (batch)
        {
            // The launcher path is expanded once inside quotes, so its characters are never interpreted by cmd.
            info.EnvironmentVariables["XRMTOOLS_PCF_NPM"] = launcher;
            info.Arguments = $"/d /s /c \"\"%XRMTOOLS_PCF_NPM%\" {command}\"";
        }
        else info.Arguments = command;
        // Only this child process gets the selected Node.js; Visual Studio's own environment is left as it is.
        info.EnvironmentVariables["PATH"] = node.PathWith(Environment.GetEnvironmentVariable("PATH"));

        var lines = new List<string>();
        using var job = KillOnCloseJob.Create();
        using var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.Exited += (_, _) => exited.TrySetResult(true);
        DataReceivedEventHandler collect = (_, e) =>
        {
            if (e.Data is null) return;
            var line = ProcessCommandFormatting.SanitizeOutput(e.Data, null);
            lock (lines) lines.Add(line);
            output.Report(line);
        };
        process.OutputDataReceived += collect; process.ErrorDataReceived += collect;
        cancellationToken.ThrowIfCancellationRequested();
        try { if (!process.Start()) throw new InvalidOperationException($"npm could not be started from '{launcher}'."); }
        catch (Win32Exception ex) { throw new InvalidOperationException($"npm could not be started from '{launcher}': {ex.Message}", ex); }
        job.Assign(process);
        process.BeginOutputReadLine(); process.BeginErrorReadLine();
        // Closing the job ends npm and everything it started, including lifecycle scripts.
        using (cancellationToken.Register(job.Dispose))
            await exited.Task.ConfigureAwait(false);
        // Waits for the redirected output to drain so npm's last error lines are not lost. Bounded, because a stray
        // descendant holding the pipe must not keep a finished or canceled setup waiting.
        await Task.WhenAny(Task.Run(process.WaitForExit), Task.Delay(5000)).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        lock (lines) return new PcfNpmResult { ExitCode = process.ExitCode, Output = lines.ToArray() };
    }

    private sealed class KillOnCloseJob : IDisposable
    {
        private readonly SafeFileHandle _handle;
        private KillOnCloseJob(SafeFileHandle handle) => _handle = handle;

        public static KillOnCloseJob Create()
        {
            var handle = CreateJobObject(IntPtr.Zero, null);
            if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
            var limits = new JobLimits { BasicLimitInformation = new BasicLimits { LimitFlags = 0x2000 } };
            if (SetInformationJobObject(handle, 9, ref limits, (uint)Marshal.SizeOf<JobLimits>())) return new KillOnCloseJob(handle);
            var error = Marshal.GetLastWin32Error();
            handle.Dispose();
            throw new Win32Exception(error);
        }

        public void Assign(Process process)
        {
            if (AssignProcessToJobObject(_handle, process.Handle)) return;
            var error = Marshal.GetLastWin32Error();
            try { process.Kill(); } catch (Exception ex) when (ex is InvalidOperationException or Win32Exception) { }
            throw new Win32Exception(error, "Could not take ownership of the npm process tree.");
        }

        public void Dispose() => _handle.Dispose();

        [StructLayout(LayoutKind.Sequential)]
        private struct BasicLimits
        {
            public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize, MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass, SchedulingClass;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct IoCounters { public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)]
        private struct JobLimits
        {
            public BasicLimits BasicLimitInformation;
            public IoCounters IoInfo;
            public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateJobObject(IntPtr attributes, string? name);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(SafeFileHandle job, int informationClass, ref JobLimits information, uint length);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);
    }
}

internal enum PcfSetupFailureKind { RegistryAuthentication, Network, LockFileMismatch, PackageNotFound, EngineMismatch, FileAccess, Other }

/// <summary>Names the cause of a failed npm command only when npm itself reported one.</summary>
internal sealed class PcfNpmFailure
{
    private PcfNpmFailure(PcfSetupFailureKind kind, string? code, string summary, string remedy) { Kind = kind; Code = code; Summary = summary; Remedy = remedy; }

    public PcfSetupFailureKind Kind { get; }
    public string? Code { get; }
    public string Summary { get; }
    public string Remedy { get; }

    public static PcfNpmFailure Classify(string command, PcfNpmResult result)
    {
        var text = string.Join("\n", result.Output);
        var code = Regex.Match(text, @"^npm (?:ERR!|error) code (\S+)", RegexOptions.Multiline | RegexOptions.IgnoreCase) is { Success: true } match ? match.Groups[1].Value.ToUpperInvariant() : null;
        if (code == "EUSAGE" && Regex.IsMatch(text, @"package\.json and package-lock\.json[\s\S]*in sync|npm ci.*can only install", RegexOptions.IgnoreCase))
            return new(PcfSetupFailureKind.LockFileMismatch, code, "package-lock.json does not match package.json, so npm ci refused to install.",
                "Nothing was changed. If the lock file should be updated, choose Update lock file to run npm install, which rewrites package-lock.json; otherwise restore the matching package.json or lock file.");
        switch (code)
        {
            case "E401": case "E403": case "ENEEDAUTH":
                return new(PcfSetupFailureKind.RegistryAuthentication, code, $"The npm registry rejected the request ({code}).",
                    "Sign in to the registry this project uses (for example with npm login, or by refreshing the token in your .npmrc), then retry. Xrm Tools does not change registry credentials.");
            case "ENOTFOUND": case "ETIMEDOUT": case "ECONNRESET": case "ECONNREFUSED": case "EAI_AGAIN": case "ENETUNREACH":
            case "ERR_SOCKET_TIMEOUT": case "UNABLE_TO_GET_ISSUER_CERT_LOCALLY": case "SELF_SIGNED_CERT_IN_CHAIN": case "UNABLE_TO_VERIFY_LEAF_SIGNATURE": case "CERT_HAS_EXPIRED":
                return new(PcfSetupFailureKind.Network, code, $"npm could not reach the registry ({code}).",
                    "Check the network connection, proxy and registry settings in your npm configuration, then retry.");
            case "E404":
                return new(PcfSetupFailureKind.PackageNotFound, code, "A package was not found in the configured registry (E404).",
                    "Check the package name and version in package.json and the registry configured for this project, then retry.");
            case "EBADENGINE": case "ENOTSUP":
                return new(PcfSetupFailureKind.EngineMismatch, code, $"A package does not support the selected Node.js or npm version ({code}).",
                    "Select a Node.js version that the project's packages support, then retry.");
            case "EPERM": case "EACCES": case "EBUSY": case "ENOTEMPTY":
                return new(PcfSetupFailureKind.FileAccess, code, $"npm could not change files in the project folder ({code}).",
                    "Stop the local test harness and any other process using node_modules, then retry.");
        }
        return new(PcfSetupFailureKind.Other, code, $"npm {command} exited with code {result.ExitCode}{(code is null ? string.Empty : $" ({code})")}.",
            "Review the npm output for the cause, correct it, then retry.");
    }

    /// <summary>The last lines npm marked as errors, for display next to the summary.</summary>
    public static IReadOnlyList<string> ErrorLines(PcfNpmResult result, int count = 6)
    {
        var errors = result.Output.Where(line => Regex.IsMatch(line, @"^npm (?:ERR!|error)(?:\s|$)", RegexOptions.IgnoreCase) && !Regex.IsMatch(line, @"A complete log of this run|debug-\d+\.log", RegexOptions.IgnoreCase)).ToArray();
        return errors.Skip(Math.Max(0, errors.Length - count)).ToArray();
    }
}
#nullable restore
