#nullable enable
namespace XrmTools.Pcf;

using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using XrmTools.DataverseSolutions;

internal sealed class PcfHarnessProcess : IDisposable
{
    private readonly Process _process;
    private readonly SafeFileHandle _job;
    private readonly TaskCompletionSource<Uri> _address = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<int> _exit = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Action<string> _output;
    private int _disposed;

    public bool IsRunning
    {
        get { lock (_process) return _disposed == 0 && !_process.HasExited; }
    }

    private PcfHarnessProcess(Process process, SafeFileHandle job, Action<string> output)
    {
        _process = process;
        _job = job;
        _output = output;
        process.OutputDataReceived += OnOutput;
        process.ErrorDataReceived += OnOutput;
        process.Exited += (_, _) =>
        {
            lock (process)
            {
                if (_disposed != 0) return;
                var exitCode = process.ExitCode;
                _exit.TrySetResult(exitCode);
                output($"Local PCF harness exited with code {exitCode}. Start it again after correcting any errors.");
            }
        };
    }

    public static PcfHarnessProcess Start(string projectDirectory, Action<string> output)
    {
        var packagePath = Path.Combine(projectDirectory, "package.json");
        if (!File.Exists(packagePath)) throw new FileNotFoundException("The PCF project has no package.json.", packagePath);
        var package = JObject.Parse(File.ReadAllText(packagePath));
        if (string.IsNullOrWhiteSpace((string?)package["scripts"]?["start"]))
            throw new InvalidOperationException("The PCF project's package.json must define a start script for the local harness.");
        if (!Directory.Exists(Path.Combine(projectDirectory, "node_modules", "pcf-scripts")))
            throw new InvalidOperationException("PCF dependencies are missing. Run npm install in the project directory before starting the local harness.");

        var info = CreateStartInfo(projectDirectory, Environment.GetEnvironmentVariable("PATH") ?? string.Empty);
        output($"Starting PCF harness using {info.EnvironmentVariables["XRMTOOLS_PCF_NPM"] ?? info.FileName}.");
        var job = CreateKillOnCloseJob();
        var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        var harness = new PcfHarnessProcess(process, job, output);
        var started = false;
        try
        {
            started = process.Start();
            if (!started) throw new InvalidOperationException("npm could not be started.");
            if (!AssignProcessToJobObject(job, process.Handle)) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not take ownership of the PCF harness process tree.");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return harness;
        }
        catch
        {
            try
            {
                if (started && !process.HasExited) process.Kill();
            }
            finally
            {
                harness.Dispose();
            }
            throw;
        }
    }

    public async Task<Uri> WaitForReadyAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        var canceled = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = timeout.Token.Register(() => canceled.TrySetCanceled());
        var first = await Task.WhenAny(_address.Task, _exit.Task, canceled.Task).ConfigureAwait(false);
        if (first == _exit.Task) throw new InvalidOperationException($"The PCF harness exited with code {await _exit.Task.ConfigureAwait(false)} before it became ready. See the Xrm Tools output.");
        Uri address;
        try
        {
            address = await ((Task<Uri>)first).ConfigureAwait(false);
            using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(2) };
            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                if (!IsRunning) throw new InvalidOperationException("The PCF harness exited before becoming ready. See the Xrm Tools output.");
                try
                {
                    using var response = await client.GetAsync(address, timeout.Token).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode) return address;
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) when (!timeout.IsCancellationRequested) { }
                await Task.Delay(200, timeout.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The PCF harness did not become ready within two minutes. See the Xrm Tools output.");
        }
    }

    internal static Uri? ParseAddress(string line)
    {
        var text = ProcessCommandFormatting.SanitizeOutput(line, null);
        var match = Regex.Match(text, @"\bLocal:\s*(https?://[^\s]+)", RegexOptions.IgnoreCase);
        return match.Success && Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out var address)
            && address.IsLoopback && address.UserInfo.Length == 0 ? address : null;
    }

    internal static ProcessStartInfo CreateStartInfo(string projectDirectory, string path)
    {
        var node = FindExecutable(path, "Node.js", "node.exe");
        var npm = FindExecutable(path, "npm", "npm.exe", "npm.cmd");
        var isBatchFile = string.Equals(Path.GetExtension(npm), ".cmd", StringComparison.OrdinalIgnoreCase);
        var info = new ProcessStartInfo(isBatchFile ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : npm)
        {
            WorkingDirectory = projectDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        if (isBatchFile)
        {
            // Expand the launcher path once, inside quotes, without interpreting metacharacters in it.
            info.EnvironmentVariables["XRMTOOLS_PCF_NPM"] = npm;
            info.Arguments = "/d /s /c \"\"%XRMTOOLS_PCF_NPM%\" start -- watch\"";
        }
        else
        {
            ProcessCommandFormatting.ApplyArguments(info, ["start", "--", "watch"]);
        }
        info.EnvironmentVariables["PATH"] = Path.GetDirectoryName(node) + Path.PathSeparator
            + Path.GetDirectoryName(npm) + Path.PathSeparator + path;
        return info;
    }

    private static string FindExecutable(string path, string tool, params string[] names)
    {
        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory)) continue;
            var searchDirectory = directory.Trim().Trim('"');
            if (!Path.IsPathRooted(searchDirectory)) continue;
            foreach (var name in names)
            {
                var candidate = Path.Combine(searchDirectory, name);
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            }
        }
        throw new FileNotFoundException($"{tool} was not found on Visual Studio's PATH. Ensure Node.js and npm are available, then close and reopen all Visual Studio instances, including the instance launching Experimental. Shell-only version-manager activation must also be applied before launching Visual Studio.");
    }

    private void OnOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null) return;
        var line = ProcessCommandFormatting.SanitizeOutput(e.Data, null);
        _output(line);
        var address = ParseAddress(line);
        if (address is not null) _address.TrySetResult(address);
    }

    public void Dispose()
    {
        lock (_process)
        {
            if (_disposed != 0) return;
            _disposed = 1;
            // Closing the job terminates only this harness and its descendants, including npm's shells/watchers.
            _job.Dispose();
            _process.Dispose();
            _address.TrySetCanceled();
        }
    }

    private static SafeFileHandle CreateKillOnCloseJob()
    {
        var handle = CreateJobObject(IntPtr.Zero, null);
        if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
        var limits = new JobLimits { BasicLimitInformation = new BasicLimits { LimitFlags = 0x2000 } };
        if (!SetInformationJobObject(handle, 9, ref limits, (uint)Marshal.SizeOf<JobLimits>()))
        {
            var error = Marshal.GetLastWin32Error();
            handle.Dispose();
            throw new Win32Exception(error);
        }
        return handle;
    }

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
#nullable restore
