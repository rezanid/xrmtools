# Acceptance 2: Visual Studio launched normally, Node.js/npm not on its PATH, fnm installed.
# XRMTOOLS_PCF_WINDOWS_PATH stands in for the registered Windows PATH, which on this machine contains Node.js and must not be changed.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '02-fnm-without-node-on-path.log'
function OnPath($p, $names) { foreach ($d in ($p -split ';')) { foreach ($n in $names) { if ($d -and (Test-Path (Join-Path $d $n))) { return (Join-Path $d $n) } } } }
$path = Get-NoNodePath
Log "PATH given to Visual Studio: node.exe on it: $([bool](OnPath $path @('node.exe'))); npm on it: $([bool](OnPath $path @('npm.exe','npm.cmd'))); fnm on it: $(OnPath $path @('fnm.exe'))"

Log "=== 2a: fnm has a default version; nothing to decide ==="
$vs = Start-Scenario acc2 -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path }
try {
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '02a-rebuild-output.txt'
  Log "REBUILD $(Summary $r)"
  Lines $r.Output 'Xrm Tools:|XTNODE \w+ v|error' | Select-Object -Unique | ForEach-Object { Log "    $_" }
  Log (Save-Main $vs '02a-build-with-fnm-default.png')
} finally { Stop-Vs $vs }

Log "=== 2b: fnm has two versions and no default; the build stops and Set up PCF build decides ==="
$fnmDir = "$script:Root\fnmdir"
$vs = Start-Scenario acc2b -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path; FNM_DIR = $fnmDir }
try {
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '02b-rebuild-needs-decision-output.txt'
  Log "REBUILD $(Summary $r)"
  Lines $r.Output 'error|XTNODE \w+ v' | ForEach-Object { Log "    $_" }
  Log "notification: $(Get-Notice $vs "*'Choose2' cannot build*")"
  Log (Save-Main $vs '02b-build-stopped-with-setup-action.png' -Errors)
  $s = Open-NoticeSetup $vs
  Log "setup dialog: summary='$(Text $s SetupSummary)'"
  Log "setup dialog: problem='$(Text $s ProblemText)'"
  Log (Save-Dialog $vs '02c-setup-dialog-choose-version.png' 'Set up PCF build')
  Log "selected in the dialog: $(Select-UiItem (Find-Ui $s -AutomationId NodeChoices) '*v24.21.0*')"
  Start-Sleep 1
  Log "setup dialog after selecting: summary='$(Text $s SetupSummary)'"
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s 300
  Log "setup dialog after Set up: status='$(Text $s StatusText)'"
  Log (Save-Dialog $vs '02d-setup-dialog-ready.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3
  Log "stored selection ($env:LOCALAPPDATA\XrmTools\Pcf\node-selections.json): $((Get-Content "$env:LOCALAPPDATA\XrmTools\Pcf\node-selections.json" -Raw) -replace '\s+', ' ')"
  Log "files added to the project folder by the selection: $(@(Get-ChildItem "$script:Root\acc2b\Choose2" -File -Force | Where-Object { $_.Name -like '*.user' -or $_.Name -like '.node-version' -or $_.Name -like '.nvmrc' }).Count)"
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '02e-rebuild-after-setup-output.txt'
  Log "REBUILD after setup $(Summary $r)"
  Lines $r.Output 'Xrm Tools:|XTNODE \w+ v|error' | Select-Object -Unique | ForEach-Object { Log "    $_" }
} finally { Stop-Vs $vs }

Log "=== 2c: a new Visual Studio session remembers the selection; then the selected installation disappears ==="
$vs = Start-Scenario acc2b -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path; FNM_DIR = $fnmDir }
try {
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900
  Log "REBUILD in the new session $(Summary $r)"
  Lines $r.Output 'XTNODE \w+ v' | Select-Object -Unique | ForEach-Object { Log "    $_" }
  cmd /c rmdir "$fnmDir\node-versions\v24.21.0" | Out-Null
  Log "removed the selected installation's folder (exists now: $(Test-Path "$fnmDir\node-versions\v24.21.0"))"
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '02f-rebuild-selection-gone-output.txt'
  Log "REBUILD after the installation disappeared $(Summary $r)"
  Lines $r.Output 'error|XTNODE \w+ v' | ForEach-Object { Log "    $_" }
} finally { Stop-Vs $vs; if (-not (Test-Path "$fnmDir\node-versions\v24.21.0")) { cmd /c mklink /J "$fnmDir\node-versions\v24.21.0" "$env:APPDATA\fnm\node-versions\v24.21.0" | Out-Null }; Remove-Item "$env:LOCALAPPDATA\XrmTools\Pcf\node-selections.json" -ErrorAction SilentlyContinue }
