. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-AuditCmd {
    param([switch]$Ci)
    Write-Section "Package Security Audit"
    Start-Timer
    $ok = $true

    Write-Info "Scanning NuGet packages for vulnerabilities..."
    Push-Location $Script:BACK_DIR
    $nugetResult = & dotnet list package --vulnerable 2>&1
    $nugetExit = $LASTEXITCODE
    Pop-Location

    $vulnNuget = $nugetResult | Select-String "vulnerable"
    if ($vulnNuget) {
        Write-Warn "Vulnerable NuGet packages found:"
        $vulnNuget | ForEach-Object { Write-Host "  $($Script:Colors.Yellow)$_$($Script:Colors.Reset)" }
        $ok = $false
    } else {
        Write-Success "NuGet packages: no vulnerabilities"
    }

    Write-Info "Scanning npm packages for vulnerabilities..."
    Push-Location $Script:FRONT_DIR
    $npmResult = & npm audit 2>&1
    $npmExit = $LASTEXITCODE
    Pop-Location

    if ($npmExit -ne 0) {
        $critical = $npmResult | Select-String "critical"
        $high = $npmResult | Select-String "high"
        if ($critical -or $high) {
            Write-Warn "Vulnerabilities found in npm packages"
            if ($critical) { $critical | ForEach-Object { Write-Host "  $($Script:Colors.Red)CRITICAL: $_$($Script:Colors.Reset)" } }
            if ($high) { $high | ForEach-Object { Write-Host "  $($Script:Colors.Yellow)HIGH: $_$($Script:Colors.Reset)" } }
            Write-Hint "Run 'npm audit fix' in front/ directory"
            $ok = $false
        } else {
            Write-Info "npm audit: low/moderate findings only"
        }
    } else {
        Write-Success "npm packages: no vulnerabilities"
    }

    Stop-Timer
    if ($ok) { Write-Success "All packages clean" }
    else { Write-Warn "Vulnerabilities detected"; if ($Ci) { exit 1 } }
}
