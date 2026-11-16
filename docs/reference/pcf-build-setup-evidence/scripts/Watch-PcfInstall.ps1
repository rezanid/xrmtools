# Test aid for canceling a Visual Studio build while npm is installing a PCF project's dependencies.
# It only reads the project folder: it says when to cancel, then reports what the cancel left behind.
# Usage: start this, then start the Build in Visual Studio, and press Ctrl+Break in Visual Studio at the beep.
param([Parameter(Mandatory)][string]$Project, [int]$Folders = 100)
$modules = Join-Path $Project 'node_modules'
if (Test-Path $modules) { Write-Host "'$modules' already exists; this aid is for a project without node_modules." -ForegroundColor Red; return }
Write-Host "Watching $Project"
Write-Host "Start the Build in Visual Studio now. Waiting for npm to start unpacking packages..."
while (-not (Test-Path $modules) -or @([IO.Directory]::GetDirectories($modules)).Count -lt $Folders) { Start-Sleep -Milliseconds 200 }
[Console]::Beep(1000, 500)
Write-Host ">>> CANCEL THE BUILD NOW: Ctrl+Break in Visual Studio (Build > Cancel) <<<" -ForegroundColor Yellow
Start-Sleep 12
$scripts = Test-Path (Join-Path $modules 'pcf-scripts\package.json')
$finished = Test-Path (Join-Path $modules '.package-lock.json')
$running = @(Get-CimInstance Win32_Process -Filter "Name='node.exe'" | Where-Object { $_.CommandLine -like '*npm-cli.js*install*' }).Count
Write-Host ("12 s later: folders in node_modules = {0}; pcf-scripts present = {1}; npm finished the install = {2}; npm install still running = {3}" -f @([IO.Directory]::GetDirectories($modules)).Count, $scripts, $finished, ($running -gt 0))
if ($finished) { Write-Host "RESULT: the install finished before the cancel took effect. Delete '$modules' and try again." -ForegroundColor Red }
elseif ($running -gt 0) { Write-Host "RESULT: npm install is still running, so the build was not canceled. Cancel it, or wait and delete '$modules' to try again." -ForegroundColor Red }
else { Write-Host "RESULT: HALF-INSTALLED (this is the state the check needs). Continue with the checklist." -ForegroundColor Green }
