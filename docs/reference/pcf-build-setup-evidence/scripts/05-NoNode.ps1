# Acceptance 5: no Node.js installation anywhere; guidance in the build, the setup dialog and the wizard.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '05-no-node-installation.log'
$path = ((Get-PersistentPath) -split ';' | Where-Object { $_ -and $_ -notmatch 'nvm|fnm' }) -join ';'
$vs = Start-Scenario acc5 -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path; FNM_DIR = "$script:Root\fnmempty" }
try {
  Log "Visual Studio started without node, npm or fnm on PATH; the registered Windows PATH is replaced by the same value; fnm's directory has no installed versions"
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 600; Save-Output $r '05a-rebuild-output.txt'
  Log "REBUILD $(Summary $r)"
  Lines $r.Output 'error|is not recognized' | ForEach-Object { Log "    $_" }
  Log "notification: $(Get-Notice $vs "*'None5' cannot build*")"
  Log (Save-Main $vs '05a-build-stopped-node-missing.png' -Errors)
  $s = Open-NoticeSetup $vs
  Log "setup dialog: summary='$(Text $s SetupSummary)'"
  Log "setup dialog: problem='$(Text $s ProblemText)'"
  Log "setup dialog: actions offered: $(((Get-UiElements $s | Where-Object { $_.Type -eq 'Button' -and $_.Id -in 'BrowseButton','CheckAgainButton','GetNodeButton' }).Name) -join ', ')"
  Log "setup dialog: guidance='$((Get-UiElements $s | Where-Object { $_.Name -like 'Get Node.js opens*' } | Select-Object -First 1).Name)'"
  Invoke-Ui (Find-Ui $s -AutomationId SetUpButton); Wait-SetupIdle $s 60
  Log "setup dialog after choosing Set up without Node.js: status='$(Text $s StatusText)'"
  Log (Save-Dialog $vs '05b-setup-dialog-node-missing.png' 'Set up PCF build')
  Invoke-Ui (Find-Ui $s -AutomationId CloseButton); Start-Sleep 3
  $w = Open-Wizard $vs 'NoNode'
  Log "wizard: summary='$(Text $w SetupSummary)'"
  Log "wizard: install='$(Text $w InstallSummary)'"
  Log "wizard: setup options $((Find-Ui $w -AutomationId SetupOptions).GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Current.ExpandCollapseState) by itself; problem='$(Text $w ProblemText)'"
  $rect = $w.Current.BoundingRectangle
  Log "wizard window: $([int]$rect.Width)x$([int]$rect.Height) px; Create button on screen: $(-not (Find-Ui $w -AutomationId CreateButton).Current.IsOffscreen)"
  Log (Save-Dialog $vs '05c-wizard-node-missing.png' 'Add PCF project')
  Invoke-Ui (Find-Ui $w -AutomationId CancelButton); Start-Sleep 2
} finally { Stop-Vs $vs }
