# Acceptance 9: a setup and a build requested for the same project at the same time do not race.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '09-concurrent-setup-and-build.log'
$proj = "$script:Root\acc9\FreshA"
function Reset-FreshA { Remove-Item "$proj\node_modules" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item "$proj\out" -Recurse -Force -ErrorAction SilentlyContinue }
$vs = Start-Scenario acc9
try {
  Log "=== 9a: setup requested while a build is installing dependencies for the same project ==="
  Invoke-VsBuild $vs -Action Rebuild -NoWait
  Start-Sleep 12
  Log "build state when setup is requested: $([XtVs.Driver]::BuildState($vs.Dte)) (2 = in progress)"
  $s = Open-Setup $vs 'FreshA'
  Log "setup dialog during the build: dependencies='$(Text $s DependenciesText)'"
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s 120
  Log "Set up chosen during the build: status='$(Text $s StatusText)'; npm output box shown: $([bool](Find-Ui $s -AutomationId SetupOutput -Seconds 1))"
  Log (Save-Dialog $vs '09a-setup-refused-during-build.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 2
  $r = Wait-VsBuild $vs 2400 (Get-Date) 'Rebuild'; Save-Output $r '09a-rebuild-output.txt'
  Log "REBUILD (not disturbed) $(Summary $r); 'added N packages' lines: $(@(Lines $r.Output 'added \d+ packages').Count), one for each of the two fresh projects"

  Log "=== 9b: build requested while this Visual Studio's setup dialog is installing ==="
  Reopen $vs acc9 { Reset-FreshA }
  $s = Open-Setup $vs 'FreshA'
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Start-Sleep 10
  Log "setup running: status='$(Text $s StatusText)'"
  $helper = Start-Process powershell.exe -WindowStyle Hidden -PassThru -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"`"$PSScriptRoot\Exec.ps1`"",'-ProcessId',$vs.Process.Id,'-Command','Build.BuildSolution'
  Start-Sleep 20
  # While the dialog is modal, Visual Studio answers nothing else over automation, so its state is read from the dialog and the processes.
  Log "20s after a Build.BuildSolution request from another process: request still waiting on Visual Studio: $(-not $helper.HasExited); MSBuild or npm started by a build: $([bool](Get-CimInstance Win32_Process | Where-Object { $_.ParentProcessId -eq $vs.Process.Id -and $_.Name -eq 'cmd.exe' -and $_.CommandLine -match 'pcf-scripts|npm run' })); setup still installing: $(-not (Find-Ui $s -AutomationId SetUpButton -Seconds 1).Current.IsEnabled)"
  if (-not $helper.HasExited) { $helper.Kill() }
  Wait-SetupIdle $s
  Log "setup finished undisturbed: dependencies='$(Text $s DependenciesText)' status='$(Text $s StatusText)'"
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3

  Log "=== 9c: build requested while another Visual Studio process runs setup for the same project (its setup lock is held) ==="
  Reopen $vs acc9 { Reset-FreshA }
  $sha = [System.Security.Cryptography.SHA256]::Create()
  $hash = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes([IO.Path]::GetFullPath($proj).TrimEnd('\').ToUpperInvariant())), 0, 12).Replace('-', '')
  $lockFile = "$env:LOCALAPPDATA\XrmTools\Pcf\setup\$hash.lock"
  New-Item -ItemType Directory (Split-Path $lockFile) -Force | Out-Null
  $held = New-Object IO.FileStream($lockFile, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None, 1, [IO.FileOptions]::DeleteOnClose)
  Log "process $PID holds $lockFile, as a second Visual Studio running setup would"
  $b = Invoke-VsBuild $vs -Action Build -Seconds 900; Save-Output $b '09c-build-during-setup-output.txt'
  Log "BUILD while the setup lock is held $(Summary $b)"
  Lines $b.Output 'XTPCF|npm install|Restoring NPM|added \d+ packages' | ForEach-Object { Log "    $_" }
  Log "  FreshA node_modules touched by that build: $(Test-Path "$proj\node_modules")"
  Log "  notification: $(Get-Notice $vs "*Setup is still installing npm dependencies for 'FreshA'*")"
  Log "  $(Save-Main $vs '09c-build-stopped-while-setup-runs.png' -Errors)"
  $held.Dispose(); Log "released the setup lock (lock file left behind: $(Test-Path $lockFile))"
  $b2 = Invoke-VsBuild $vs -Action Build; Save-Output $b2 '09c-build-after-setup-output.txt'
  Log "BUILD after the lock was released $(Summary $b2)"
  Lines $b2.Output 'XTPCF|Xrm Tools:|added \d+ packages' | ForEach-Object { Log "    $_" }
} finally { Stop-Vs $vs }

