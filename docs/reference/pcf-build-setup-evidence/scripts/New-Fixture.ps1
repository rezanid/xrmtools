param([string]$Solution, [string[]]$Projects, [switch]$Installed, [switch]$Lock, [switch]$NoProbe)
$root = "$env:TEMP\xt-pcf"; $sol = "$root\$Solution"
Remove-Item $sol -Recurse -Force -ErrorAction SilentlyContinue; New-Item -ItemType Directory $sol | Out-Null
$entries = @()
foreach ($n in $Projects) {
  robocopy "$root\templates\Field" "$sol\$n" /E /NFL /NDL /NJH /NJS /NP | Out-Null
  if ($Installed) { robocopy "$root\cli1\node_modules" "$sol\$n\node_modules" /E /MT:32 /NFL /NDL /NJH /NJS /NP | Out-Null; Copy-Item "$root\cli1\package-lock.json" "$sol\$n\package-lock.json" }
  if ($Lock -and -not $Installed) { Copy-Item "$root\cli1\package-lock.json" "$sol\$n\package-lock.json" }
  Rename-Item "$sol\$n\Field.pcfproj" "$n.pcfproj"
  $p = "$sol\$n\$n.pcfproj"; $x = Get-Content $p -Raw; $x = $x -replace '<Name>Field</Name>', "<Name>$n</Name>" -replace '<ProjectGuid>[^<]+</ProjectGuid>', "<ProjectGuid>$([guid]::NewGuid())</ProjectGuid>"; Set-Content $p $x -NoNewline
  if (-not $NoProbe) {
    $pkg = "$sol\$n\package.json"; $j = Get-Content $pkg -Raw
    $probe = 'node -e \"console.log(''XTNODE ' + $n + ' '' + process.version + '' '' + process.execPath)\" && '
    foreach ($s in 'build','clean','rebuild') { $j = $j -replace ('"' + $s + '": "pcf-scripts ' + $s + '"'), ('"' + $s + '": "' + $probe + 'pcf-scripts ' + $s + '"') }
    $stamp = (Get-Item $pkg).LastWriteTimeUtc; Set-Content $pkg $j -NoNewline; (Get-Item $pkg).LastWriteTimeUtc = $stamp
  }
  $entries += "  <Project Path=`"$n/$n.pcfproj`" Type=`"2f8b96df-a546-4f1e-b20b-30cfcd8043f8`" />"
}
"<Solution>`r`n$($entries -join "`r`n")`r`n</Solution>" | Set-Content "$sol\$Solution.slnx"
"$sol\$Solution.slnx"

