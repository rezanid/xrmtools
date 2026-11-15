# Acceptance 3 and 4: fresh checkouts without node_modules; first Build, and separately first Rebuild.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '03-04-fresh-build-rebuild.log'
foreach ($case in @(@{ Sln = 'acc3'; Proj = 'Fresh3'; Action = 'Build'; N = '03' }, @{ Sln = 'acc4'; Proj = 'Fresh4'; Action = 'Rebuild'; N = '04' })) {
  $projDir = "$script:Root\$($case.Sln)\$($case.Proj)"
  Log "=== acceptance $($case.N): first $($case.Action) of a fresh checkout (node_modules present: $(Test-Path "$projDir\node_modules"); package-lock.json present: $(Test-Path "$projDir\package-lock.json")) ==="
  $vs = Start-Scenario $case.Sln
  try {
    $r = Invoke-VsBuild $vs -Action $case.Action -Seconds 2400; Save-Output $r "$($case.N)-first-$($case.Action.ToLower())-output.txt"
    Log "FIRST $($case.Action.ToUpper()) $(Summary $r)"
    Lines $r.Output 'Xrm Tools:|Restoring NPM|added \d+ packages|XTNODE \w+ v|error|> .*pcf-scripts (clean|build|rebuild) ' | ForEach-Object { Log "    $(($_ -replace '--noColor.*$', '...'))" }
    Log "  pcf-scripts installed: $(Test-Path "$projDir\node_modules\pcf-scripts\package.json"); bundle.js produced: $(Test-Path "$projDir\out\controls\FieldControl\bundle.js"); package-lock.json unchanged: $((Get-Sha256 "$projDir\package-lock.json") -eq (Get-Sha256 "$script:Root\cli1\package-lock.json"))"
    Log "  $(Save-Main $vs "$($case.N)-first-$($case.Action.ToLower()).png")"
    $u = Invoke-VsBuild $vs -Action Build -Seconds 600; Save-Output $u "$($case.N)-unchanged-build-output.txt"
    Log "UNCHANGED BUILD AFTER $(Summary $u); $(Ran $u)"
  } finally { Stop-Vs $vs }
}

