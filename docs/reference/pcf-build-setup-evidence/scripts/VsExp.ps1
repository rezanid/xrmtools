# Visual Studio Experimental automation helpers for PCF acceptance runs (Windows PowerShell 5.1).
# Not part of the repository; used to drive real desktop validation and collect evidence.
Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing, System.Windows.Forms, UIAutomationClient, UIAutomationTypes, WindowsBase

if (-not ('XtVs.Rot' -as [type])) {
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
namespace XtVs {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Text;

	public static class Rot {
		[DllImport("ole32.dll")] static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable rot);
		[DllImport("ole32.dll")] static extern int CreateBindCtx(int reserved, out IBindCtx ctx);
		public static object Find(string suffix) {
			IRunningObjectTable rot; IBindCtx ctx; IEnumMoniker monikers;
			GetRunningObjectTable(0, out rot); CreateBindCtx(0, out ctx); rot.EnumRunning(out monikers);
			var one = new IMoniker[1];
			while (monikers.Next(1, one, IntPtr.Zero) == 0) {
				string name; one[0].GetDisplayName(ctx, null, out name);
				if (name.StartsWith("!VisualStudio.DTE.") && name.EndsWith(suffix)) { object value; rot.GetObject(one[0], out value); return value; }
			}
			return null;
		}
	}

	[ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IOleMessageFilter {
		[PreserveSig] int HandleInComingCall(int callType, IntPtr caller, int tick, IntPtr info);
		[PreserveSig] int RetryRejectedCall(IntPtr callee, int tick, int rejectType);
		[PreserveSig] int MessagePending(IntPtr callee, int tick, int pendingType);
	}
	public sealed class MessageFilter : IOleMessageFilter {
		[DllImport("ole32.dll")] static extern int CoRegisterMessageFilter(IOleMessageFilter filter, out IOleMessageFilter old);
		public static void Register() { IOleMessageFilter old; CoRegisterMessageFilter(new MessageFilter(), out old); }
		public int HandleInComingCall(int a, IntPtr b, int c, IntPtr d) { return 0; }
		public int RetryRejectedCall(IntPtr callee, int tick, int rejectType) { return rejectType == 2 || rejectType == 1 ? 250 : -1; }
		public int MessagePending(IntPtr callee, int tick, int pendingType) { return 2; }
	}

	public static class Win {
		public delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
		[DllImport("user32.dll")] static extern bool EnumWindows(EnumProc proc, IntPtr lParam);
		[DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
		[DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);
		[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr w, IntPtr l);
		[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int cmd);
		[DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int w, int h, bool repaint);
		[DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT rect, int size);
		[DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
		[StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
		public static List<KeyValuePair<IntPtr, string>> Windows(int pid) {
			var list = new List<KeyValuePair<IntPtr, string>>();
			EnumWindows((h, l) => {
				uint owner; GetWindowThreadProcessId(h, out owner);
				if (owner == pid && IsWindowVisible(h)) { var sb = new StringBuilder(512); GetWindowText(h, sb, 512); list.Add(new KeyValuePair<IntPtr, string>(h, sb.ToString())); }
				return true;
			}, IntPtr.Zero);
			return list;
		}
		[DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
        // PrintWindow renders the window itself, so the capture does not depend on what is in front of it.
        public static string Capture(IntPtr hwnd, string file) {
            RECT r; GetWindowRect(hwnd, out r);
            using (var bmp = new Bitmap(r.Right - r.Left, r.Bottom - r.Top)) {
                using (var g = Graphics.FromImage(bmp)) { IntPtr dc = g.GetHdc(); try { PrintWindow(hwnd, dc, 2); } finally { g.ReleaseHdc(dc); } }
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                var colors = new HashSet<int>();
                for (int x = 0; x < bmp.Width; x += 7) for (int y = 0; y < bmp.Height; y += 7) colors.Add(bmp.GetPixel(x, y).ToArgb());
                return bmp.Width + "x" + bmp.Height + ", " + colors.Count + " sampled colors";
            }
        }
    }
}
'@
}
[XtVs.MessageFilter]::Register()

$script:Devenv = 'C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\devenv.exe'

function Get-PersistentPath {
	$machine = [Environment]::GetEnvironmentVariable('Path', 'Machine'); $user = [Environment]::GetEnvironmentVariable('Path', 'User')
	[Environment]::ExpandEnvironmentVariables(($machine.TrimEnd(';') + ';' + $user))
}

function Start-VsExp {
    param([string]$Solution, [string]$Path = (Get-PersistentPath), [hashtable]$Env = @{}, [string[]]$RemoveEnv = @(), [string]$RootSuffix = 'Exp')
    # Start-Process (ShellExecute) gives devenv this process's environment without inheriting its output handles.
    $saved = @{}; $names = @('PATH') + @($Env.Keys) + $RemoveEnv
    foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name) }
    try {
        [Environment]::SetEnvironmentVariable('PATH', $Path)
        foreach ($name in $RemoveEnv) { [Environment]::SetEnvironmentVariable($name, $null) }
        foreach ($key in $Env.Keys) { [Environment]::SetEnvironmentVariable($key, [string]$Env[$key]) }
        $arguments = "/RootSuffix $RootSuffix" + $(if ($Solution) { " `"$Solution`"" } else { '' })
        $process = Start-Process -FilePath $script:Devenv -ArgumentList $arguments -PassThru
    } finally { foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name]) } }
    $deadline = (Get-Date).AddMinutes(4); $dte = $null
    while (-not $dte -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 2; $dte = [XtVs.Rot]::Find(":$($process.Id)") }
    if (-not $dte) { throw "DTE for devenv $($process.Id) did not appear." }
    [pscustomobject]@{ Process = $process; Dte = $dte }
}
function Get-VsWindows { param($Vs) [XtVs.Win]::Windows($Vs.Process.Id) | ForEach-Object { [pscustomobject]@{ Handle = $_.Key; Title = $_.Value } } }

# --- UI Automation helpers ---
function Get-UiWindow { param($Vs, [string]$Name, [int]$Seconds = 30)
	$deadline = (Get-Date).AddSeconds($Seconds)
	while ((Get-Date) -lt $deadline) {
		$handle = Get-VsWindows $Vs | Where-Object { $_.Title -eq $Name } | Select-Object -First 1
		if ($handle) { return [System.Windows.Automation.AutomationElement]::FromHandle($handle.Handle) }
		Start-Sleep -Milliseconds 500
	}
	throw "Window '$Name' did not appear."
}
function Find-Ui { param($Root, [string]$Name, [string]$AutomationId, [string]$ControlType, [int]$Seconds = 15)
	$conditions = @()
	if ($Name) { $conditions += New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $Name) }
	if ($AutomationId) { $conditions += New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $AutomationId) }
	if ($ControlType) { $conditions += New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::$ControlType) }
	$condition = if ($conditions.Count -eq 1) { $conditions[0] } else { New-Object System.Windows.Automation.AndCondition($conditions) }
	$deadline = (Get-Date).AddSeconds($Seconds)
	while ($true) {
		$element = $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
		if ($element -or (Get-Date) -ge $deadline) { return $element }
		Start-Sleep -Milliseconds 300
	}
}
function Set-UiValue { param($Element, [string]$Value) $Element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($Value) }
function Invoke-Ui { param($Element) $Element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }


function Connect-Vs { param([int]$ProcessId)
    $dte = [XtVs.Rot]::Find(":$ProcessId"); if (-not $dte) { throw "No DTE for process $ProcessId." }
    [pscustomobject]@{ Process = [System.Diagnostics.Process]::GetProcessById($ProcessId); Dte = $dte }
}
if (-not ('XtVs.Driver' -as [type])) {
Add-Type -ReferencedAssemblies Microsoft.CSharp, System.Core -TypeDefinition @"
namespace XtVs {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    public static class Driver {
        static T Retry<T>(Func<T> action) {
            for (int i = 0; ; i++) {
                try { return action(); }
                catch (System.Runtime.InteropServices.COMException ex) {
                    if ((ex.ErrorCode != unchecked((int)0x80010001) && ex.ErrorCode != unchecked((int)0x8001010A)) || i > 240) throw;
                    Thread.Sleep(500);
                }
            }
        }
        public static bool IsOpen(object dte) { return Retry(() => { dynamic d = dte; return (bool)d.Solution.IsOpen; }); }
        public static int ProjectCount(object dte) { return Retry(() => { dynamic d = dte; return (int)d.Solution.Projects.Count; }); }
        public static void Execute(object dte, string command, string args) { Retry(() => { dynamic d = dte; d.ExecuteCommand(command, args ?? ""); return 0; }); }
        public static int BuildState(object dte) { return Retry(() => { dynamic d = dte; return (int)d.Solution.SolutionBuild.BuildState; }); }
        public static int LastBuildInfo(object dte) { return Retry(() => { dynamic d = dte; return (int)d.Solution.SolutionBuild.LastBuildInfo; }); }
        public static long MainWindow(object dte) { return Retry(() => { dynamic d = dte; return (long)d.MainWindow.HWnd; }); }
        public static void Activate(object dte) { Retry(() => { dynamic d = dte; d.MainWindow.Activate(); return 0; }); }
        public static string Pane(object dte, string name) {
            return Retry(() => {
                dynamic d = dte; dynamic panes = d.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}").Object.OutputWindowPanes;
                for (int i = 1; i <= (int)panes.Count; i++) {
                    dynamic pane = panes.Item(i);
                    if ((string)pane.Name == name) { dynamic doc = pane.TextDocument; dynamic start = doc.StartPoint.CreateEditPoint(); return (string)start.GetText(doc.EndPoint); }
                }
                return "";
            });
        }
        public static string[] Errors(object dte) {
            return Retry(() => {
                dynamic d = dte; dynamic items = d.Windows.Item("{D78612C7-9962-4B83-95D9-268046DAD23A}").Object.ErrorItems;
                var list = new List<string>();
                for (int i = 1; i <= (int)items.Count; i++) { dynamic item = items.Item(i); list.Add(Convert.ToInt32((object)item.ErrorLevel) + "|" + Convert.ToString((object)item.Project) + "|" + Convert.ToString((object)item.Description)); }
                return list.ToArray();
            });
        }
        public static void Select(object dte, string name) {
            Retry(() => {
                dynamic d = dte; dynamic window = d.Windows.Item("{3AE79031-E1BC-11D0-8F78-00A0C9110057}"); window.Activate(); dynamic explorer = window.Object;
                var stack = new Stack<object>(); stack.Push(explorer.UIHierarchyItems.Item(1));
                while (stack.Count > 0) {
                    dynamic item = stack.Pop();
                    if ((string)item.Name == name) { try { object target = item; target.GetType().InvokeMember("Select", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { 1 }); } catch (Exception ex) { throw new InvalidOperationException("Select failed: " + ex.GetType().Name + " " + ex.Message + " | " + (ex.InnerException == null ? "" : ex.InnerException.Message)); } return 0; }
                    dynamic children = item.UIHierarchyItems; children.Expanded = true;
                    for (int i = 1; i <= (int)children.Count; i++) stack.Push(children.Item(i));
                }
                throw new InvalidOperationException("Solution item not found: " + name);
            });
        }
        public static void BuildApi(object dte) { Retry(() => { dynamic d = dte; d.Solution.SolutionBuild.Build(false); return 0; }); }
        public static void Close(object dte) { Retry(() => { dynamic d = dte; d.Solution.Close(false); return 0; }); }
        public static void Quit(object dte) { Retry(() => { dynamic d = dte; d.Quit(); return 0; }); }
        public static void Open(object dte, string solution) { Retry(() => { dynamic d = dte; d.Solution.Open(solution); return 0; }); }
        public static void SaveAll(object dte) { Retry(() => { dynamic d = dte; d.ExecuteCommand("File.SaveAll", ""); return 0; }); }
    }
}
"@
}

function Wait-SolutionLoaded { param($Vs, [int]$Seconds = 240, [int]$Settle = 12)
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        if ([XtVs.Driver]::IsOpen($Vs.Dte) -and [XtVs.Driver]::ProjectCount($Vs.Dte) -gt 0) { Start-Sleep -Seconds $Settle; return }
        Start-Sleep -Seconds 2
    }
    throw 'Solution did not load.'
}
function Get-OutputPane { param($Vs, [string]$Name = 'Build') [XtVs.Driver]::Pane($Vs.Dte, $Name) }
function Get-ErrorList { param($Vs) [XtVs.Driver]::Errors($Vs.Dte) | ForEach-Object { $p = $_ -split '\|', 3; [pscustomobject]@{ Level = [int]$p[0]; Project = $p[1]; Description = $p[2] } } }
function Invoke-VsBuild { param($Vs, [ValidateSet('Build','Rebuild','Clean')][string]$Action = 'Build', [int]$Seconds = 1800, [string]$Project, [switch]$NoWait)
    $command = if ($Project) { "Build.${Action}Selection" } else { "Build.${Action}Solution" }
    if ($Project) { Select-Project $Vs $Project }
    $started = Get-Date
    [XtVs.Driver]::Execute($Vs.Dte, $command, '')
    if ($NoWait) { return }
    Wait-VsBuild $Vs $Seconds $started $Action
}
function Wait-VsBuild { param($Vs, [int]$Seconds = 1800, $Started = (Get-Date), [string]$Action = 'Build')
    Start-Sleep -Seconds 2
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) { if ([XtVs.Driver]::BuildState($Vs.Dte) -eq 3) { break }; Start-Sleep -Seconds 1 }
    [pscustomobject]@{ Action = $Action; FailedProjects = [XtVs.Driver]::LastBuildInfo($Vs.Dte); Seconds = [int]((Get-Date) - $Started).TotalSeconds; Output = (Get-OutputPane $Vs 'Build') }
}
function Save-Screenshot { param($Vs, [string]$File, [string]$TitleLike)
    $handle = if ($TitleLike) { (Get-VsWindows $Vs | Where-Object { $_.Title -like $TitleLike } | Select-Object -First 1).Handle } else { [IntPtr][XtVs.Driver]::MainWindow($Vs.Dte) }
    if (-not $handle) { throw "Window '$TitleLike' not found." }
    [XtVs.Win]::SetForegroundWindow($handle) | Out-Null; Start-Sleep -Milliseconds 800
    New-Item -ItemType Directory (Split-Path $File) -Force | Out-Null
    "$File ($([XtVs.Win]::Capture($handle, $File)))"
}
function Stop-Vs { param($Vs)
    # A modal dialog left open makes Visual Studio refuse automation calls, so close ours first.
    foreach ($title in 'Set up PCF build', 'Add PCF project') { Close-VsWindow $Vs $title }
    Start-Sleep 2
    try { [XtVs.Driver]::Close($Vs.Dte) } catch { }
    try { [XtVs.Driver]::Quit($Vs.Dte) } catch { }
    if (-not $Vs.Process.WaitForExit(60000)) { $Vs.Process.Kill() }
}
function Start-VsCommand { param($Vs, [string]$Command, [string]$Arguments = '')
    # Commands that open a modal dialog block the DTE call, so they run from a separate process.
    Start-Process powershell.exe -WindowStyle Hidden -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"`"$PSScriptRoot\Exec.ps1`"",'-ProcessId',$Vs.Process.Id,'-Command',"`"$Command`"" | Out-Null
}
function Select-Project { param($Vs, [string]$Name)
    [XtVs.Driver]::Activate($Vs.Dte); [XtVs.Driver]::Execute($Vs.Dte, 'View.SolutionExplorer', '')
    $main = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr][XtVs.Driver]::MainWindow($Vs.Dte))
    $item = Find-Ui $main -Name $Name -ControlType TreeItem -Seconds 30
    if (-not $item) { throw "Solution Explorer item '$Name' not found." }
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); $item.SetFocus(); Start-Sleep -Milliseconds 600
}
function Get-UiElements { param($Root)
    $all = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($e in $all) { [pscustomobject]@{ Type = $e.Current.ControlType.ProgrammaticName -replace 'ControlType\.', ''; Id = $e.Current.AutomationId; Name = $e.Current.Name; Enabled = $e.Current.IsEnabled } }
}
function Get-UiValue { param($Element) $p = $null; if ($Element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$p)) { $p.Current.Value } else { $Element.Current.Name } }
function Set-UiToggle { param($Element, [bool]$On) $p = $Element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern); if (($p.Current.ToggleState -eq 'On') -ne $On) { $p.Toggle() } }
function Expand-Ui { param($Element) $Element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand() }
function Select-UiItem { param($Combo, [string]$NameLike)
    Expand-Ui $Combo; Start-Sleep -Milliseconds 500
    $items = $Combo.FindAll([System.Windows.Automation.TreeScope]::Descendants, (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)))
    foreach ($item in $items) { if ($item.Current.Name -like $NameLike) { $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); $Combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse(); return $item.Current.Name } }
    throw "No item like '$NameLike'. Items: $(($items | ForEach-Object { $_.Current.Name }) -join ' | ')"
}
function Wait-Ui { param([scriptblock]$Condition, [int]$Seconds = 60, [string]$What = 'condition')
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) { $value = & $Condition; if ($value) { return $value }; Start-Sleep -Milliseconds 500 }
    throw "Timed out waiting for $What."
}
function Close-VsWindow { param($Vs, [string]$Title) Get-VsWindows $Vs | Where-Object { $_.Title -eq $Title } | ForEach-Object { [XtVs.Win]::PostMessage($_.Handle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null } }


# --- helpers shared by the acceptance scripts ---
$script:Evidence = "$env:TEMP\xt-pcf\evidence"
$script:Root = "$env:TEMP\xt-pcf"
function Start-Log { param([string]$Name) $script:LogFile = Join-Path $script:Evidence $Name; New-Item -ItemType Directory $script:Evidence -Force | Out-Null; Remove-Item $script:LogFile -ErrorAction SilentlyContinue }
function Log { param($Text) $Text | Tee-Object -FilePath $script:LogFile -Append }
function Lines { param($Output, [string]$Pattern) ($Output -split "`r?`n") | Where-Object { $_ -match $Pattern } | ForEach-Object { $_.Trim() } }
function Get-NoNodePath { ((Get-PersistentPath) -split ';' | Where-Object { $_ -and $_ -notmatch 'nvm' }) -join ';' }
function Start-Scenario { param([string]$Solution, [string]$Path = (Get-PersistentPath), [hashtable]$Env = @{}, [int]$Settle = 30)
    $vs = Start-VsExp -Solution "$script:Root\$Solution\$Solution.slnx" -Path $Path -Env $Env
    Set-Content "$script:Root\vs.pid" $vs.Process.Id
    Wait-SolutionLoaded $vs -Settle $Settle
    [XtVs.Win]::ShowWindow([IntPtr][XtVs.Driver]::MainWindow($vs.Dte), 3) | Out-Null
    $vs
}
function Save-Main { param($Vs, [string]$Name, [switch]$Errors)
    [XtVs.Driver]::Activate($Vs.Dte)
    try { [XtVs.Driver]::Execute($Vs.Dte, $(if ($Errors) { 'View.ErrorList' } else { 'View.Output' }), '') } catch { }
    Start-Sleep 2
    Save-Screenshot $Vs (Join-Path $script:Evidence $Name)
}
function Save-Dialog { param($Vs, [string]$Name, [string]$Title) Save-Screenshot $Vs (Join-Path $script:Evidence $Name) $Title }
function Save-Output { param($Result, [string]$Name) $Result.Output | Set-Content (Join-Path $script:Evidence $Name) -Encoding UTF8 }
function Get-Main { param($Vs) [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr][XtVs.Driver]::MainWindow($Vs.Dte)) }
function Get-Notice { param($Vs, [string]$Like, [int]$Seconds = 60)
    $main = Get-Main $Vs
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) { $found = Get-UiElements $main | Where-Object { $_.Name -like $Like } | Select-Object -First 1; if ($found) { return $found.Name.Trim() }; Start-Sleep 1 }
    return $null
}
function Open-NoticeSetup { param($Vs) Invoke-Ui (Find-Ui (Get-Main $Vs) -Name 'Set up PCF build' -ControlType Hyperlink -Seconds 20); Open-SetupWindow $Vs }
function Open-SetupWindow { param($Vs)
    $s = Get-UiWindow $Vs 'Set up PCF build' 60
    Wait-Ui { $b = Find-Ui $s -AutomationId SetUpButton -Seconds 1; $b -and $b.Current.IsEnabled } 120 'setup dialog to load' | Out-Null
    $s
}
function Open-Setup { param($Vs, [string]$Project) Select-Project $Vs $Project; Start-VsCommand $Vs 'XrmTools.Pcf.SetUpBuild'; Open-SetupWindow $Vs }
function Wait-SetupIdle { param($Dialog, [int]$Seconds = 1500) Start-Sleep 4; Wait-Ui { (Find-Ui $Dialog -AutomationId SetUpButton -Seconds 1).Current.IsEnabled } $Seconds 'setup to finish' | Out-Null }
function Text { param($Root, [string]$Id) $e = Find-Ui $Root -AutomationId $Id -Seconds 3; if ($e) { $e.Current.Name } else { '<not shown>' } }
function Open-Wizard { param($Vs, [string]$Name)
    Start-VsCommand $Vs 'XrmTools.Pcf.AddProject'
    $w = Get-UiWindow $Vs 'Add PCF project' 60
    Wait-Ui { (Find-Ui $w -AutomationId SetupSummary -Seconds 1).Current.Name -notlike 'Checking*' } 90 'environment check' | Out-Null
    Set-UiValue (Find-Ui $w -AutomationId ProjectName) $Name
    Set-UiValue (Find-Ui $w -AutomationId ControlName) 'WizControl'
    Set-UiValue (Find-Ui $w -AutomationId Namespace) 'XrmToolsAcc'
    $w
}
function Wait-WizardClosed { param($Vs, [int]$Seconds = 1500) Wait-Ui { -not (Get-VsWindows $Vs | Where-Object { $_.Title -eq 'Add PCF project' }) } $Seconds 'wizard to close' | Out-Null; Start-Sleep 15 }
function Wait-WizardIdle { param($Wizard, [int]$Seconds = 1500) Start-Sleep 4; Wait-Ui { $b = Find-Ui $Wizard -AutomationId RetryButton -Seconds 1; $b -and $b.Current.IsEnabled } $Seconds 'wizard to become idle' | Out-Null }
function Summary { param($Result) "failedProjects=$($Result.FailedProjects) seconds=$($Result.Seconds); $(Lines $Result.Output '==========' | Select-Object -First 1)" }
function Ran { param($Result) "npm install ran: $([bool]($Result.Output -match 'npm install|Restoring NPM|added \d+ packages')); pcf-scripts ran: $([bool]($Result.Output -match 'pcf-scripts (build|rebuild|clean) '))" }
function Reopen { param($Vs, [string]$Solution, [scriptblock]$While) [XtVs.Driver]::Close($Vs.Dte); Start-Sleep 5; if ($While) { & $While }; [XtVs.Driver]::Open($Vs.Dte, "$script:Root\$Solution\$Solution.slnx"); Wait-SolutionLoaded $Vs -Settle 30 }

# Get-FileHash is unavailable when Windows PowerShell inherits PowerShell 7's module path.
function Get-Sha256 { param([string]$Path) $sha = [System.Security.Cryptography.SHA256]::Create(); try { $s = [IO.File]::OpenRead($Path); try { [BitConverter]::ToString($sha.ComputeHash($s)).Replace('-', '') } finally { $s.Dispose() } } finally { $sha.Dispose() } }