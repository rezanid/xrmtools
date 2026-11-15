# Acceptance 6: dependency installation fails in the wizard; the project is kept; Retry setup and Open project recover.
# The failure is produced by a user npm configuration that points at an unreachable registry, then corrected.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '06-install-failure-and-retry.log'
$npmrc = "$script:Root\npmrc-acc6"
function Break-Registry { Set-Content $npmrc "registry=http://127.0.0.1:9/`nfetch-retries=0`n" -Encoding ASCII }
function Fix-Registry { Set-Content $npmrc "" -Encoding ASCII }
Break-Registry
$vs = Start-Scenario accA -Env @{ NPM_CONFIG_USERCONFIG = $npmrc }
try {
  Log "Visual Studio $($vs.Process.Id): normal PATH; user npm configuration: $((Get-Content $npmrc) -join '; ')"
  Log "=== 6a: install fails, the cause is corrected, Retry setup ==="
  $w = Open-Wizard $vs 'Wiz6a'
  Invoke-Ui (Find-Ui $w -AutomationId CreateButton); Wait-WizardIdle $w
  Log "status after the failed install: $(Text $w StatusText)"
  Log "buttons: Retry setup shown=$([bool](Find-Ui $w -AutomationId RetryButton -Seconds 1)); Open project shown=$([bool](Find-Ui $w -AutomationId OpenButton -Seconds 1)); Create shown=$([bool](Find-Ui $w -AutomationId CreateButton -Seconds 1))"
  Log "project kept: Wiz6a.pcfproj=$(Test-Path "$script:Root\accA\Wiz6a\Wiz6a.pcfproj"); pcf-scripts installed=$(Test-Path "$script:Root\accA\Wiz6a\node_modules\pcf-scripts\package.json")"
  Log "npm output shown in the wizard (last lines): $(((Get-UiValue (Find-Ui $w -AutomationId SetupOutput)) -split "`r?`n" | Where-Object { $_ -match 'npm error (code|errno|syscall)' }) -join ' | ')"
  Log (Save-Dialog $vs '06a-wizard-install-failed.png' 'Add PCF project')
  Fix-Registry; Log "corrected the npm configuration; choosing Retry setup"
  Invoke-Ui (Find-Ui $w -AutomationId RetryButton); Wait-WizardClosed $vs
  Log "after Retry setup: projects in solution=$([XtVs.Driver]::ProjectCount($vs.Dte)); pcf-scripts installed=$(Test-Path "$script:Root\accA\Wiz6a\node_modules\pcf-scripts\package.json")"

  Log "=== 6b: install fails, Open project, notification, Set up PCF build ==="
  Break-Registry
  $w = Open-Wizard $vs 'Wiz6b'
  Invoke-Ui (Find-Ui $w -AutomationId CreateButton); Wait-WizardIdle $w
  Log "status after the failed install: $(Text $w StatusText)"
  Invoke-Ui (Find-Ui $w -AutomationId OpenButton); Wait-WizardClosed $vs 120
  Log "Open project: projects in solution=$([XtVs.Driver]::ProjectCount($vs.Dte)); notification: $(Get-Notice $vs "*Setup for 'Wiz6b' is incomplete*")"
  Log (Save-Main $vs '06b-opened-with-incomplete-setup-notification.png')
  Fix-Registry; Log "corrected the npm configuration; choosing 'Set up PCF build' in the notification"
  $s = Open-NoticeSetup $vs
  Log "setup dialog: dependencies='$(Text $s DependenciesText)' status='$(Text $s StatusText)'"
  Log (Save-Dialog $vs '06c-setup-dialog-incomplete.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s
  Log "setup dialog after Set up: dependencies='$(Text $s DependenciesText)' status='$(Text $s StatusText)'"
  Log (Save-Dialog $vs '06d-setup-dialog-ready.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3
  Log "notification still shown after setup: $([bool](Get-Notice $vs "*Setup for 'Wiz6b' is incomplete*" 3))"
  $b = Invoke-VsBuild $vs -Action Build; Save-Output $b '06e-build-output.txt'
  Log "BUILD after recovery $(Summary $b); $(Ran $b)"
} finally { Stop-Vs $vs; Fix-Registry }
