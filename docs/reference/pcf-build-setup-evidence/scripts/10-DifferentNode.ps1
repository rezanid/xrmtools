# Acceptance 10: two projects in one solution that declare different Node.js versions.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '10-different-node-requirements.log'
$path = Get-NoNodePath
$vs = Start-Scenario acc10 -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path }
try {
  Log "P22\.nvmrc = $(Get-Content "$script:Root\acc10\P22\.nvmrc"); P24\.nvmrc = $(Get-Content "$script:Root\acc10\P24\.nvmrc"); fnm has v22.15.0 and v24.21.0; no Node.js on Visual Studio's PATH"
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '10a-rebuild-both-output.txt'
  Log "REBUILD SOLUTION, both projects loaded: $(Summary $r)"
  Lines $r.Output 'Xrm Tools:|XTNODE P\d\d v|error' | Select-Object -Unique | ForEach-Object { Log "    $_" }
  Log "  an npm script ran for P24 with any Node.js version: $([bool]($r.Output -match 'XTNODE P24'))"
  Log "  notification: $(Get-Notice $vs "*'P24' needs a different Node.js version*")"
  Log "  $(Save-Main $vs '10a-one-project-stopped-with-limitation.png' -Errors)"
  Select-Project $vs 'P22'
  [XtVs.Driver]::Execute($vs.Dte, 'Project.UnloadProject', ''); Start-Sleep 8
  Log "unloaded P22 in the same Visual Studio session"
  $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r '10b-rebuild-p24-alone-output.txt'
  Log "REBUILD SOLUTION, P22 unloaded: $(Summary $r)"
  Lines $r.Output 'Xrm Tools:|XTNODE P\d\d v|error' | Select-Object -Unique | ForEach-Object { Log "    $_" }
} finally { Stop-Vs $vs }
