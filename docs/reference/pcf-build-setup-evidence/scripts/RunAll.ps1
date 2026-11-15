# Runs every acceptance script in order. Each one starts and stops its own Visual Studio Experimental instance.
param([string]$Commit = '', [string[]]$Only = @())
$ErrorActionPreference = 'Continue'
$scripts = '01-Wizard','02-Fnm','03-FreshBuildRebuild','05-NoNode','06-InstallFailure','07-Cancel','09-Concurrency','10-DifferentNode','11-OtherPackageManager','12-EnvironmentPropagation','13-WindowsPathAndHarness'
if ($Only.Count -eq 0) { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\00-Prepare.ps1" -Commit $Commit *> "$env:TEMP\xt-pcf\run-00.txt" }
foreach ($name in $scripts) {
  if ($Only.Count -gt 0 -and $Only -notcontains $name) { continue }
  $started = Get-Date
  & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\$name.ps1" *> "$env:TEMP\xt-pcf\run-$name.txt"
  "{0} exit={1} minutes={2:N1}" -f $name, $LASTEXITCODE, ((Get-Date) - $started).TotalMinutes
  # A script that failed midway may have left its own Experimental instance running; end only that one.
  $last = [int](Get-Content "$env:TEMP\xt-pcf\vs.pid" -ErrorAction SilentlyContinue)
  $left = Get-Process -Id $last -ErrorAction SilentlyContinue
  if ($left -and $left.ProcessName -eq 'devenv') { "  ended leftover Experimental instance $last"; $left.Kill() }
  Start-Sleep 8
}
