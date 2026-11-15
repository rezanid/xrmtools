param([int]$ProcessId, [string]$Command, [string]$Arguments = '')
. "$PSScriptRoot\VsExp.ps1"
$vs = Connect-Vs $ProcessId
try { [XtVs.Driver]::Execute($vs.Dte, $Command, $Arguments) } catch { $_ | Out-File "$env:TEMP\xt-pcf\exec-error.txt" }
