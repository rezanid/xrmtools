# Additional checks: Node.js that is on the registered Windows PATH but not in this Visual Studio's own PATH
# (as after installing Node.js while Visual Studio is open), and the local test harness with an fnm-only environment.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '13-windows-path-and-harness.log'
$path = Get-NoNodePath
Log "=== 13a: Visual Studio's own PATH has no Node.js; the registered Windows PATH does; no restart ==="
$vs = Start-Scenario accW -Path $path
try {
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '13a-rebuild-output.txt'
  Log "REBUILD $(Summary $r)"
  Lines $r.Output 'Xrm Tools:|XTNODE \w+ v|error' | Select-Object -Unique | ForEach-Object { Log "    $_" }
} finally { Stop-Vs $vs }

Log "=== 13b: Start Local Test Harness with fnm only (no Node.js on Visual Studio's PATH) ==="
function HarnessNodes { @(Get-CimInstance Win32_Process -Filter "Name='node.exe'" | Where-Object { $_.CommandLine -match 'pcf-scripts' }) }
$vs = Start-Scenario acc2 -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path }
try {
  Select-Project $vs 'Fnm2'
  [XtVs.Driver]::Execute($vs.Dte, 'XrmTools.Pcf.StartHarness', '')
  $deadline = (Get-Date).AddSeconds(150); $pane = ''
  while ((Get-Date) -lt $deadline) { Start-Sleep 5; $pane = Get-OutputPane $vs 'Xrm Tools'; if ($pane -match 'Local:\s*http|exited with code|not found|Set Up PCF Build') { break } }
  Lines $pane 'Starting PCF|Local:|exited with code|not found|Set Up PCF Build' | Select-Object -First 6 | ForEach-Object { Log "    $_" }
  Log "harness node process: $((HarnessNodes | Select-Object -First 1).ExecutablePath)"
  Select-Project $vs 'Fnm2'
  [XtVs.Driver]::Execute($vs.Dte, 'XrmTools.Pcf.StopHarness', ''); Start-Sleep 5
  Log "harness node processes after Stop Local Test Harness: $(@(HarnessNodes).Count)"
} finally { Stop-Vs $vs }
