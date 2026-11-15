# Acceptance 1 and 8: normal Windows launch, usable Node.js/npm, wizard defaults, build, unchanged build.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '01-wizard-normal-launch.log'
$vs = Start-Scenario accA
try {
  Log "Visual Studio $($vs.Process.Id) started with the registered Windows PATH (normal launch)"
  $w = Open-Wizard $vs 'Wiz1'
  Log "wizard summary : $(Text $w SetupSummary)"
  Log "wizard install : $(Text $w InstallSummary)"
  $expander = Find-Ui $w -AutomationId SetupOptions
  Log "setup options  : $($expander.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Current.ExpandCollapseState)"
  Log (Save-Dialog $vs '01a-wizard-defaults.png' 'Add PCF project')
  Expand-Ui $expander; Start-Sleep 1
  Log "install project dependencies checkbox: $((Find-Ui $w -AutomationId InstallCheckBox).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState)"
  Log "selected Node.js installation: $(((Find-Ui $w -AutomationId NodeChoices).GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection() | ForEach-Object { $_.Current.Name }) -join ', ')"
  $r = $w.Current.BoundingRectangle; Log "wizard size with setup options open: $([int]$r.Width)x$([int]$r.Height) px"
  Log (Save-Dialog $vs '01b-wizard-setup-options.png' 'Add PCF project')
  Invoke-Ui (Find-Ui $w -AutomationId CreateButton)
  Start-Sleep 25
  Log "status while installing: $(Text $w StatusText)"
  Log (Save-Dialog $vs '01c-wizard-installing.png' 'Add PCF project')
  $sw = [Diagnostics.Stopwatch]::StartNew(); Wait-WizardClosed $vs
  Log "wizard closed $([int]$sw.Elapsed.TotalSeconds + 25)s after Create"
  $proj = "$script:Root\accA\Wiz1"
  Log "projects in solution: $([XtVs.Driver]::ProjectCount($vs.Dte)); Wiz1.pcfproj: $(Test-Path "$proj\Wiz1.pcfproj"); pcf-scripts installed: $(Test-Path "$proj\node_modules\pcf-scripts\package.json"); package-lock.json: $(Test-Path "$proj\package-lock.json")"
  $b = Invoke-VsBuild $vs -Action Build; Save-Output $b '01d-build-output.txt'
  Log "BUILD $(Summary $b); $(Ran $b)"
  Log (Save-Main $vs '01e-build-succeeded.png')
  $u = Invoke-VsBuild $vs -Action Build -Seconds 600; Save-Output $u '08-unchanged-build-output.txt'
  Log "UNCHANGED BUILD (acceptance 8) $(Summary $u); $(Ran $u)"
} finally { Stop-Vs $vs }
