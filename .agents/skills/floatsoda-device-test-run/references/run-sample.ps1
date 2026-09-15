<#
.SYNOPSIS
  Launch one catalog sample for the walkthrough, stopping whichever one is running first.
.EXAMPLE
  ./run-sample.ps1 Padding                 # stop previous, launch the Padding sample, report alive / exited
  ./run-sample.ps1 Padding -Desktop        # same, but pass --desktop to show it in a desktop window
  ./run-sample.ps1 -Scenario OverlayQuit   # same, for a floatsoda-device-test-gen harness scenario
  ./run-sample.ps1 stop                    # stop the current process only
.NOTES
  Log: <ResultsDir>/logs/<Name>.log (+ .err). State: <ResultsDir>/current.pid ("<pid>|<exe path>").
  ResultsDir defaults to $HOME/tmp/floatsoda-walkthrough/<today>.
  Harness exe: tests/FloatSoda.DeviceTest/bin/Debug/net10.0/FloatSoda.DeviceTest.exe --scenario <Id>
#>
param(
    [Parameter(Mandatory = $true)][string]$Name,
    [switch]$Scenario,
    [switch]$Desktop,
    [string]$ResultsDir = (Join-Path $HOME ("tmp/floatsoda-walkthrough/" + (Get-Date -Format 'yyyy-MM-dd')))
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$logDir = Join-Path $ResultsDir 'logs'
New-Item -ItemType Directory -Force $logDir | Out-Null
$pidFile = Join-Path $ResultsDir 'current.pid'

if (Test-Path $pidFile) {
    # current.pid holds "<pid>|<exe path>". Stop only if that PID still runs that exe —
    # a stale file (crash after the 4 s check, external kill) may point at a reused PID.
    $old = (Get-Content $pidFile -ErrorAction SilentlyContinue) -split '\|', 2
    $proc = if ($old[0]) { Get-Process -Id $old[0] -ErrorAction SilentlyContinue } else { $null }
    if ($proc -and $old.Count -eq 2 -and $proc.Path -eq $old[1]) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    } elseif ($proc) {
        Write-Warning "current.pid ($($old[0])) now belongs to '$($proc.Path)', not the recorded sample; not stopping it"
    }
    Remove-Item $pidFile -Force
}
if ($Name -eq 'stop') { 'stopped'; exit 0 }

if ($Scenario) {
    $exe = Join-Path $repo 'tests/FloatSoda.DeviceTest/bin/Debug/net10.0/FloatSoda.DeviceTest.exe'
    $args = @('--scenario', $Name)
} else {
    $exe = Join-Path $repo "samples/FloatSoda.Samples.$Name/bin/Debug/net10.0/FloatSoda.Samples.$Name.exe"
    $args = if ($Desktop) { @('--desktop') } else { @() }
}
if (-not (Test-Path $exe)) { throw "not built: $exe (run dotnet build first)" }
$log = Join-Path $logDir "$Name.log"
$startArgs = @{ FilePath = $exe; WorkingDirectory = (Split-Path $exe); RedirectStandardOutput = $log; RedirectStandardError = "$log.err"; PassThru = $true }
if ($args.Count -gt 0) { $startArgs.ArgumentList = $args }
$p = Start-Process @startArgs
Set-Content $pidFile "$($p.Id)|$exe"
Start-Sleep -Seconds 4
if ($p.HasExited) {
    "exited early (code $($p.ExitCode)):"
    Get-Content $log -Tail 20 -ErrorAction SilentlyContinue
    Get-Content "$log.err" -Tail 20 -ErrorAction SilentlyContinue
    exit 1
}
"running $Name (pid $($p.Id))"
