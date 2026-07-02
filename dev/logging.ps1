. "$PSScriptRoot/colors.ps1"

$Script:LogFile = $null
$Script:LogLevel = "INFO"
$Script:StartTime = $null
$Script:QuietMode = $false
$Script:VerboseMode = $false
$Script:DebugMode = $false

$Script:LevelColors = @{
    "TRACE" = $Script:Colors.Dim
    "DEBUG" = $Script:Colors.Dim
    "INFO"  = $Script:Colors.Green
    "WARN"  = $Script:Colors.Yellow
    "ERROR" = $Script:Colors.Red
    "FATAL" = $Script:Colors.BgRed
}

function Set-LogLevel {
    param([string]$Level)
    $Script:LogLevel = $Level.ToUpper()
}

function Set-QuietMode  { $Script:QuietMode = $true }
function Set-VerboseMode { $Script:VerboseMode = $true }
function Set-DebugMode { $Script:DebugMode = $true }

function Get-Timestamp {
    return (Get-Date -Format "HH:mm:ss")
}

function Get-LogTimestamp {
    return (Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff")
}

function Init-LogFile {
    $logDir = "$PSScriptRoot/logs"
    if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }
    $logName = "dev-$(Get-Date -Format 'yyyy-MM-dd').log"
    $Script:LogFile = "$logDir/$logName"
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO",
        [string]$ForegroundColor = $null
    )

    $levelNum = @{ "TRACE"=0; "DEBUG"=1; "INFO"=2; "WARN"=3; "ERROR"=4; "FATAL"=5 }
    $currentNum = $levelNum[$Script:LogLevel]
    $msgNum = $levelNum[$Level]
    if ($msgNum -lt $currentNum) { return }

    $timestamp = Get-Timestamp
    $logTimestamp = Get-LogTimestamp
    $color = if ($ForegroundColor) { $ForegroundColor } else { $Script:LevelColors[$Level] }
    $label = $Level.PadRight(5)

    if (-not $Script:QuietMode -or $Level -in @("ERROR","FATAL","WARN")) {
        if ($Level -eq "FATAL") {
            Write-Host "$($Script:Colors.BgRed)$($Script:Colors.White)[$timestamp $label] $Message$($Script:Colors.Reset)"
        } elseif ($Level -eq "ERROR") {
            Write-Host "$($Script:Colors.Red)[$timestamp $label] $Message$($Script:Colors.Reset)"
        } elseif ($Level -eq "WARN") {
            Write-Host "$($Script:Colors.Yellow)[$timestamp $label] $Message$($Script:Colors.Reset)"
        } elseif ($Level -eq "INFO") {
            Write-Host "$($Script:Colors.Green)[$timestamp $label] $Message$($Script:Colors.Reset)"
        } elseif ($Level -in @("DEBUG","TRACE") -and $Script:DebugMode) {
            Write-Host "$($Script:Colors.Dim)[$timestamp $label] $Message$($Script:Colors.Reset)"
        } else {
            Write-Host "$($Script:Colors.Cyan)[$timestamp $label] $Message$($Script:Colors.Reset)"
        }
    }

    if ($Script:LogFile) {
        $logEntry = "[$logTimestamp $label] $Message"
        Add-Content -Path $Script:LogFile -Value $logEntry -Encoding UTF8
    }
}

function Write-Info    { param([string]$m) Write-Log -Message $m -Level "INFO" }
function Write-Warn    { param([string]$m) Write-Log -Message $m -Level "WARN" }
function Write-Error   { param([string]$m) Write-Log -Message $m -Level "ERROR" }
function Write-Debug   { param([string]$m) if ($Script:DebugMode) { Write-Log -Message $m -Level "DEBUG" } }
function Write-Success { param([string]$m) Write-Host "$($Script:Colors.Green)$($Script:Colors.Bold)✓ $m$($Script:Colors.Reset)" }
function Write-Fail    { param([string]$m) Write-Host "$($Script:Colors.Red)$($Script:Colors.Bold)✗ $m$($Script:Colors.Reset)" }
function Write-Section { param([string]$m) Write-Host "`n$($Script:Colors.Bold)$($Script:Colors.Blue)═══ $m ═══$($Script:Colors.Reset)" }
function Write-Hint    { param([string]$m) Write-Host "$($Script:Colors.Cyan)  ? $m$($Script:Colors.Reset)" }

function Start-Timer {
    $Script:StartTime = Get-Date
}

function Stop-Timer {
    if ($Script:StartTime) {
        $elapsed = (Get-Date) - $Script:StartTime
        Write-Host "$($Script:Colors.Dim)Completed in $($elapsed.TotalSeconds.ToString('F2'))s$($Script:Colors.Reset)"
        $Script:StartTime = $null
    }
}

function Write-Result {
    param([bool]$Success, [string]$Message)
    if ($Success) { Write-Success $Message } else { Write-Fail $Message }
}

function Write-Banner {
    Write-Host @"
$($Script:Colors.Cyan)$($Script:Colors.Bold)
  ___  __  __  ___  _      _    ___
 / _ \|  \/  |/ _ \| |    / \  |_ _|
| | | | |\/| | | | | |   / _ \  | |
| |_| | |  | | |_| | |___/ ___ \ | |
 \___/|_|  |_|\___/|_____/_/   \_\___|
$($Script:Colors.Reset)
$($Script:Colors.Dim)  3OMAL-Platform Developer CLI$($Script:Colors.Reset)
"@
}
