# Acceptance 11: a project explicitly configured for another package manager is left untouched and explained.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '11-other-package-manager.log'
$proj = "$script:Root\acc11\Pnpm11"
function Snapshot { (Get-ChildItem $proj -Force -File | Where-Object { $_.Name -notlike '*.user' } | Sort-Object Name | ForEach-Object { "$($_.Name):$((Get-Sha256 $_.FullName).Substring(0, 12))" }) -join ' ' }
$vs = Start-Scenario acc11
try {
  Log "package.json declares: $((Select-String -Path "$proj\package.json" -Pattern packageManager).Line.Trim()) (pnpm and Yarn are also installed on this machine)"
  $before = Snapshot
  foreach ($action in 'Build', 'Rebuild') {
    $r = Invoke-VsBuild $vs -Action $action -Seconds 900; Save-Output $r "11-$($action.ToLower())-output.txt"
    Log "$($action.ToUpper()) $(Summary $r)"
    Lines $r.Output 'error|npm install|Restoring NPM|added \d+ packages' | ForEach-Object { Log "    $_" }
  }
  Log "project files unchanged: $($before -eq (Snapshot)); package-lock.json created: $(Test-Path "$proj\package-lock.json"); node_modules created: $(Test-Path "$proj\node_modules")"
  Log "notification: $(Get-Notice $vs "*'Pnpm11' is configured for pnpm, so the build did not run npm install*")"
  Log (Save-Main $vs '11a-build-stopped-other-package-manager.png' -Errors)
  $s = Open-NoticeSetup $vs
  Log "setup dialog: package manager='$(Text $s PackageManagerText)'"
  Log "setup dialog: dependencies='$(Text $s DependenciesText)' status='$(Text $s StatusText)'"
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s 120
  Log "after choosing Set up: status='$(Text $s StatusText)'; project files unchanged: $($before -eq (Snapshot)); node_modules created: $(Test-Path "$proj\node_modules")"
  Log (Save-Dialog $vs '11b-setup-dialog-other-package-manager.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 2
} finally { Stop-Vs $vs }

