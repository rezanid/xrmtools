# Prepares disposable fixtures and records the tool versions used by the acceptance runs.
param([string]$Commit = '', [switch]$EnvironmentOnly)
. "$PSScriptRoot\VsExp.ps1"
$root = $script:Root
if (-not $EnvironmentOnly) {
Remove-Item $script:Evidence -Recurse -Force -ErrorAction SilentlyContinue; New-Item -ItemType Directory $script:Evidence | Out-Null
Remove-Item "$env:LOCALAPPDATA\XrmTools\Pcf" -Recurse -Force -ErrorAction SilentlyContinue
foreach ($name in 'accA','acc3','acc4','acc5','acc7','acc9','acc9tmp','acc10','acc11','acc2','acc2b','accW','par3') { Remove-Item "$root\$name" -Recurse -Force -ErrorAction SilentlyContinue }
& "$PSScriptRoot\New-Fixture.ps1" -Solution accA -Projects Base -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc3 -Projects Fresh3 -Lock | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc4 -Projects Fresh4 -Lock | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc5 -Projects None5 -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc7 -Projects Fresh7, Fresh7b -Lock | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc2 -Projects Fnm2 -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc2b -Projects Choose2 -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution accW -Projects WinPath -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution par3 -Projects P1, P2, P3 -Installed | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc10 -Projects P22, P24 -Installed | Out-Null
Set-Content "$root\acc10\P22\.nvmrc" "22" -Encoding ASCII; Set-Content "$root\acc10\P24\.nvmrc" "24" -Encoding ASCII
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc11 -Projects Pnpm11 | Out-Null
$pkg = "$root\acc11\Pnpm11\package.json"; $stamp = (Get-Item $pkg).LastWriteTimeUtc
(Get-Content $pkg -Raw).Replace('"version": "1.0.0",', '"version": "1.0.0",' + "`n" + '  "packageManager": "pnpm@9.12.0",') | Set-Content $pkg -NoNewline; (Get-Item $pkg).LastWriteTimeUtc = $stamp
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc9 -Projects FreshA, FreshB -Lock | Out-Null
& "$PSScriptRoot\New-Fixture.ps1" -Solution acc9tmp -Projects Base9 -Installed | Out-Null
Move-Item "$root\acc9tmp\Base9" "$root\acc9\Base9"; Remove-Item "$root\acc9tmp" -Recurse -Force
Set-Content "$root\acc9\acc9.slnx" "<Solution>`r`n  <Project Path=`"FreshA/FreshA.pcfproj`" Type=`"2f8b96df-a546-4f1e-b20b-30cfcd8043f8`" />`r`n  <Project Path=`"FreshB/FreshB.pcfproj`" Type=`"2f8b96df-a546-4f1e-b20b-30cfcd8043f8`" />`r`n  <Project Path=`"Base9/Base9.pcfproj`" Type=`"2f8b96df-a546-4f1e-b20b-30cfcd8043f8`" />`r`n</Solution>"
# An fnm directory with two versions and no default, and an empty one. Junctions only; the real fnm folder is not changed.
$fd = "$root\fnmdir"; if (Test-Path $fd) { Get-ChildItem "$fd\node-versions" | ForEach-Object { cmd /c rmdir "$($_.FullName)" | Out-Null }; Remove-Item $fd -Recurse -Force }
New-Item -ItemType Directory "$fd\node-versions" | Out-Null
foreach ($v in 'v22.15.0','v24.21.0') { cmd /c mklink /J "$fd\node-versions\$v" "$env:APPDATA\fnm\node-versions\$v" | Out-Null }
New-Item -ItemType Directory "$root\fnmempty" -Force | Out-Null
}

Start-Log '00-environment.log'
$vsDir = Split-Path (Split-Path (Split-Path $script:Devenv))
Log "date (UTC): $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm'))"
Log "commit under test: $Commit"
Log "Windows: $([Environment]::OSVersion.VersionString), $env:PROCESSOR_ARCHITECTURE"
Log "Visual Studio: $((Get-Item $script:Devenv).VersionInfo.ProductVersion) at $vsDir, Experimental instance (/RootSuffix Exp)"
Log "MSBuild: $((Get-Item "$vsDir\MSBuild\Current\Bin\MSBuild.exe").VersionInfo.FileVersion)"
$ext = Get-ChildItem "$env:LOCALAPPDATA\Microsoft\VisualStudio\18.0_de2c1603Exp\Extensions" -Recurse -Filter XrmTools.dll | Select-Object -First 1
Log "Xrm Tools deployed to Experimental: $($ext.VersionInfo.FileVersion), built $($ext.LastWriteTimeUtc.ToString('yyyy-MM-dd HH:mm')) UTC, SHA-256 $((Get-Sha256 $ext.FullName).Substring(0, 16))"
Log "PAC: $((& pac help 2>&1 | Select-String 'Version:').Line.Trim())"
Log "Microsoft.PowerApps.MSBuild.Pcf: $((Get-ChildItem "$env:USERPROFILE\.nuget\packages\microsoft.powerapps.msbuild.pcf" -Name) -join ', ')"
Log "pcf-scripts: $((Get-Content "$root\cli1\node_modules\pcf-scripts\package.json" -Raw | ConvertFrom-Json).version)"
$fnm = (Get-Command fnm).Source
Log "fnm: $(& $fnm --version) at $fnm; installed: $((& $fnm list) -join ' | ')"
foreach ($v in 'v22.15.0','v24.21.0') { Log "  fnm $v : node $(& $fnm exec --using $v node --version), npm $(& $fnm exec --using $v npm.cmd --version)" }
$persistent = Get-PersistentPath
$node = ($persistent -split ';' | Where-Object { $_ -and (Test-Path (Join-Path $_ 'node.exe')) } | Select-Object -First 1)
Log "registered Windows PATH: node.exe in '$node' (node $(& (Join-Path $node 'node.exe') --version), npm $(& (Get-ChildItem $node -Filter npm.* | Where-Object { $_.Extension -in '.exe', '.cmd' } | Select-Object -First 1).FullName --version))"
Log "fixtures: $root (PAC field template; projects named per scenario; npm scripts print 'XTNODE <project> <node version> <node path>' so the build output shows which Node.js Microsoft's npm commands ran)"



