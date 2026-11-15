# Acceptance 7: cancel a running setup; no ready state, no stranded lock or process; retry succeeds.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '07-cancel-and-retry.log'
$proj = "$script:Root\acc7\Fresh7"; $state = "$env:LOCALAPPDATA\XrmTools\Pcf\setup"
function Descendants($root) { $all = @(Get-CimInstance Win32_Process); $ids = @($root); $seen = @{}; $found = @(); while ($ids.Count -gt 0) { $next = @($all | Where-Object { $ids -contains $_.ParentProcessId -and -not $seen.ContainsKey([int]$_.ProcessId) }); foreach ($p in $next) { $seen[[int]$p.ProcessId] = $true }; $found += $next; $ids = @($next | ForEach-Object { $_.ProcessId }) }; $found }
function NpmProcesses { @(Descendants $vs.Process.Id | Where-Object { $_.Name -match '^(node|npm|cmd)\.exe$' -and $_.CommandLine -match 'npm' }).Count }
function StateFiles($ext) { @(Get-ChildItem $state -Filter "*.$ext" -ErrorAction SilentlyContinue).Count }
$vs = Start-Scenario acc7
try {
  $s = Open-Setup $vs 'Fresh7'
  Log "before: dependencies='$(Text $s DependenciesText)'"
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton)
  Start-Sleep 20
  Log "while installing: status='$(Text $s StatusText)'; npm processes under Visual Studio: $(NpmProcesses); setup lock files: $(StateFiles 'lock'); node_modules exists: $(Test-Path "$proj\node_modules")"
  Log (Save-Dialog $vs '07a-setup-installing.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton)   # Close cancels while setup is running
  $sw = [Diagnostics.Stopwatch]::StartNew(); Wait-SetupIdle $s 120
  Log "cancel completed within $([int]$sw.Elapsed.TotalSeconds)s: status='$(Text $s StatusText)'"
  Log "after cancel: dependencies='$(Text $s DependenciesText)'; npm processes under Visual Studio: $(NpmProcesses); setup lock files: $(StateFiles 'lock'); incomplete markers: $(StateFiles 'incomplete'); pcf-scripts present: $(Test-Path "$proj\node_modules\pcf-scripts\package.json")"
  Log (Save-Dialog $vs '07b-setup-canceled.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s
  Log "after retry: dependencies='$(Text $s DependenciesText)' status='$(Text $s StatusText)'"
  Log "after retry: setup lock files: $(StateFiles 'lock'); incomplete markers: $(StateFiles 'incomplete'); package-lock.json unchanged by npm ci: $((Get-Sha256 "$proj\package-lock.json") -eq (Get-Sha256 "$script:Root\cli1\package-lock.json"))"
  Log (Save-Dialog $vs '07c-setup-ready-after-retry.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3

  Log "=== 7b: a second project's setup is canceled and then finished by Build instead of by setup ==="
  $proj2 = "$script:Root\acc7\Fresh7b"
  $s = Open-Setup $vs 'Fresh7b'
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Start-Sleep 45
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Wait-SetupIdle $s 120
  Log "after cancel: status='$(Text $s StatusText)'; node_modules exists: $(Test-Path "$proj2\node_modules"); pcf-scripts present: $(Test-Path "$proj2\node_modules\pcf-scripts\package.json"); incomplete markers: $(StateFiles 'incomplete')"
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3
  $b = Invoke-VsBuild $vs -Action Build -Seconds 2400; Save-Output $b '07d-build-output.txt'
  Log "BUILD SOLUTION $(Summary $b)"
  Lines $b.Output 'Xrm Tools:|added \d+ packages|up to date, audited|error' | ForEach-Object { Log "    $_" }
  Log "after the build: incomplete markers: $(StateFiles 'incomplete'); Fresh7b pcf-scripts present: $(Test-Path "$proj2\node_modules\pcf-scripts\package.json"); bundle.js produced: $(Test-Path "$proj2\out\controls\FieldControl\bundle.js")"
  $u = Invoke-VsBuild $vs -Action Build -Seconds 600
  Log "UNCHANGED BUILD AFTER $(Summary $u); $(Ran $u)"
} finally { Stop-Vs $vs }


