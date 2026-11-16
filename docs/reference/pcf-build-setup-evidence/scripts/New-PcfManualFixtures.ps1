# Creates disposable PCF projects for the manual Visual Studio checks of PR #232.
# Everything is written to a new, uniquely named folder under the Windows temporary folder. Nothing else is modified:
# no existing fixture, no repository, no Visual Studio or Xrm Tools setting.
# Needs pac, node and npm on PATH. Takes two to three minutes, most of it one "npm install" for the seed project.
param([string]$Name = ('xt-pcf-manual-' + (Get-Date -Format 'yyyyMMdd-HHmmss')))
$ErrorActionPreference = 'Stop'
$root = Join-Path ([IO.Path]::GetTempPath().TrimEnd('\')) $Name
if (Test-Path $root) { throw "'$root' already exists. Run again for a new folder." }
foreach ($tool in 'pac', 'node', 'npm') { if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' was not found on PATH." } }
New-Item -ItemType Directory $root | Out-Null

Write-Host "Creating the seed project with pac pcf init..."
$template = "$root\_seed\Field"
New-Item -ItemType Directory $template | Out-Null
& pac pcf init --namespace XtManual --name FieldControl --template field --outputDirectory $template | Out-Null
if (-not (Test-Path "$template\Field.pcfproj")) { throw "pac pcf init did not create $template\Field.pcfproj." }

Write-Host "Installing the seed project's dependencies once (npm install)..."
$seed = "$root\_seed\Installed"
robocopy $template $seed /E /NFL /NDL /NJH /NJS /NP | Out-Null
Push-Location $seed
try { & cmd /d /c "npm install --no-audit --no-fund >nul 2>&1"; if ($LASTEXITCODE -ne 0) { throw "npm install failed in $seed (exit $LASTEXITCODE)." } }
finally { Pop-Location }
# As Microsoft's own restore does: mark pcf-scripts current, so a build of an installed fixture does not install again.
(Get-Item "$seed\node_modules\pcf-scripts\package.json").LastWriteTimeUtc = [DateTime]::UtcNow

# -Installed: node_modules and package-lock.json are in place. -Lock: only package-lock.json. Neither: a project as pac creates it.
function New-Project([string]$Solution, [string]$Project, [switch]$Installed, [switch]$Lock, [string]$Nvmrc) {
	$dir = "$root\$Solution\$Project"
	robocopy $template $dir /E /NFL /NDL /NJH /NJS /NP | Out-Null
	if ($Installed) { robocopy "$seed\node_modules" "$dir\node_modules" /E /MT:32 /NFL /NDL /NJH /NJS /NP | Out-Null }
	if ($Installed -or $Lock) { Copy-Item "$seed\package-lock.json" "$dir\package-lock.json" }
	Rename-Item "$dir\Field.pcfproj" "$Project.pcfproj"
	$file = "$dir\$Project.pcfproj"
	$xml = (Get-Content $file -Raw) -replace '<Name>Field</Name>', "<Name>$Project</Name>" -replace '<ProjectGuid>[^<]+</ProjectGuid>', "<ProjectGuid>$([guid]::NewGuid())</ProjectGuid>"
	Set-Content $file $xml -NoNewline
	# Each npm script first prints "XTNODE <project> <node version> <node path>", so the build output shows which
	# Node.js Microsoft's npm commands really ran. package.json keeps its timestamp, so nothing looks changed.
	$package = "$dir\package.json"; $json = Get-Content $package -Raw
	$probe = 'node -e \"console.log(''XTNODE ' + $Project + ' '' + process.version + '' '' + process.execPath)\" && '
	foreach ($script in 'build', 'clean', 'rebuild') { $json = $json -replace ('"' + $script + '": "pcf-scripts ' + $script + '"'), ('"' + $script + '": "' + $probe + 'pcf-scripts ' + $script + '"') }
	$stamp = (Get-Item $package).LastWriteTimeUtc; Set-Content $package $json -NoNewline; (Get-Item $package).LastWriteTimeUtc = $stamp
	if ($Nvmrc) { Set-Content "$dir\.nvmrc" $Nvmrc -Encoding ASCII }
	"  <Project Path=`"$Project/$Project.pcfproj`" Type=`"2f8b96df-a546-4f1e-b20b-30cfcd8043f8`" />"
}
function New-Solution([string]$Solution, [string[]]$Entries) {
	Set-Content "$root\$Solution\$Solution.slnx" "<Solution>`r`n$($Entries -join "`r`n")`r`n</Solution>"
	"$root\$Solution\$Solution.slnx"
}

Write-Host "Creating the fixtures..."
$wizard = New-Solution '1-wizard' @(New-Project '1-wizard' 'Base' -Installed)
$fresh  = New-Solution '2-fresh'  @((New-Project '2-fresh' 'FreshBuild' -Lock), (New-Project '2-fresh' 'FreshRebuild' -Lock), (New-Project '2-fresh' 'FreshClean' -Lock))
$cancel = New-Solution '3-cancel' @((New-Project '3-cancel' 'CancelLock' -Lock), (New-Project '3-cancel' 'CancelNoLock'))
$node   = New-Solution '4-node'   @((New-Project '4-node' 'P22' -Installed -Nvmrc '22'), (New-Project '4-node' 'P24' -Installed -Nvmrc '24'))
Copy-Item "$PSScriptRoot\Watch-PcfInstall.ps1" $root -ErrorAction SilentlyContinue

$fnm = Get-Command fnm -ErrorAction SilentlyContinue
$lines = @(
	"Fixture root : $root"
	"Tools        : node $(node --version), npm $(npm --version); fnm: $(if ($fnm) { (& fnm list) -join ' | ' } else { 'not found' })"
	""
	"1 wizard     : $wizard"
	"2 fresh      : $fresh"
	"               FreshBuild, FreshRebuild, FreshClean - package-lock.json, no node_modules"
	"3 cancel     : $cancel"
	"               CancelLock   - package-lock.json, no node_modules : $root\3-cancel\CancelLock"
	"               CancelNoLock - no lock file, no node_modules      : $root\3-cancel\CancelNoLock"
	"4 node       : $node"
	"               P22 (.nvmrc 22) and P24 (.nvmrc 24), dependencies installed"
	"Cancel aid   : powershell -ExecutionPolicy Bypass -File `"$root\Watch-PcfInstall.ps1`" `"$root\3-cancel\CancelLock`""
	""
	"Delete '$root' when you are done."
)
$lines | Set-Content "$root\paths.txt"
$lines | ForEach-Object { Write-Host $_ }
