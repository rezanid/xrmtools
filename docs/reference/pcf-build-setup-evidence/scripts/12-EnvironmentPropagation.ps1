# Acceptance 12: no relaunch is needed. The selected Node.js reaches Microsoft's npm commands on every MSBuild node,
# also when the selection changes between builds in one Visual Studio session and worker nodes are reused.
. "$PSScriptRoot\VsExp.ps1"
Start-Log '12-environment-propagation.log'
function Workers { @(Get-CimInstance Win32_Process -Filter "Name='MSBuild.exe'" | Where-Object { $_.ParentProcessId -eq $vs.Process.Id } | ForEach-Object { $_.ProcessId } | Sort-Object) -join ', ' }
function Select-Node($version) {
  $map = @{}
  foreach ($n in 'P1','P2','P3') { $map["$script:Root\par3\$n\$n.pcfproj".ToUpperInvariant()] = @{ NodeDirectory = "$env:APPDATA\fnm\node-versions\$version\installation"; Version = $version } }
  New-Item -ItemType Directory "$env:LOCALAPPDATA\XrmTools\Pcf" -Force | Out-Null
  $map | ConvertTo-Json | Set-Content "$env:LOCALAPPDATA\XrmTools\Pcf\node-selections.json" -Encoding UTF8
}
$path = Get-NoNodePath
Select-Node 'v22.15.0'
$vs = Start-Scenario par3 -Path $path -Env @{ XRMTOOLS_PCF_WINDOWS_PATH = $path }
try {
  Log "Visual Studio $($vs.Process.Id) started without Node.js on PATH and was not restarted; three PCF projects build in parallel; the stored selection for all three is rewritten before each build"
  $n = 0
  foreach ($version in 'v22.15.0', 'v24.21.0', 'v22.15.0') {
    $n++; Select-Node $version
    $r = Invoke-VsBuild $vs -Action Rebuild -Seconds 900; Save-Output $r "12-rebuild-$n-$version-output.txt"
    $seen = Lines $r.Output 'XTNODE P\d v' | ForEach-Object { ($_ -replace '^\d+>\s*', '') -replace ' C:\\.*$', '' } | Sort-Object -Unique
    Log "build $n, selection $version -> REBUILD $(Summary $r)"
    Log "    Node.js reported from inside Microsoft's npm scripts: $($seen -join '; ')"
    Log "    XTPCF008 (build process saw another Node.js): $([bool]($r.Output -match 'XTPCF008')); MSBuild worker processes of this Visual Studio after the build: $(Workers)"
  }
} finally { Stop-Vs $vs; Remove-Item "$env:LOCALAPPDATA\XrmTools\Pcf\node-selections.json" -ErrorAction SilentlyContinue }
