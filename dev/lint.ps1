. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-LintCmd {
    param(
        [switch]$Fix,
        [switch]$Ci
    )
    Write-Section "Linting & Formatting"
    Start-Timer
    $ok = $true

    if (-not (Lint-Backend -Fix:$Fix)) { $ok = $false }
    if (-not (Lint-Frontend -Fix:$Fix)) { $ok = $false }

    Stop-Timer
    if ($ok) { Write-Success "Linting passed" } else { Write-Error "Linting found issues"; exit 1 }
}

function Lint-Backend {
    param([switch]$Fix)
    Write-Info "Linting backend..."
    Push-Location $Script:BACK_DIR
    $args = @("format")
    if (-not $Fix) { $args += "--verify-no-changes" }
    $result = & dotnet $args 2>&1
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -eq 0) {
        Write-Success "Backend formatting OK"
        return $true
    } else {
        Write-Fail "Backend formatting issues found"
        if ($Fix) {
            Write-Info "Formatting applied"
            return $true
        }
        $result | ForEach-Object { Write-Host "  $_" }
        Write-Hint "Run 'dev lint --fix' to auto-format"
        return $false
    }
}

function Lint-Frontend {
    param([switch]$Fix)
    Write-Info "Linting frontend..."
    Push-Location $Script:FRONT_DIR

    $eslintOk = $true
    $eslintArgs = @("eslint", "src/")
    if ($Fix) { $eslintArgs += "--fix" }
    $eslint = & npx $eslintArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "ESLint found issues"
        $eslint | ForEach-Object { Write-Host "  $_" }
        $eslintOk = $false
    } else {
        Write-Success "ESLint OK"
    }

    $prettierArgs = @("prettier", "--check", "src/")
    if ($Fix) { $prettierArgs = @("prettier", "--write", "src/") }
    $prettier = & npx $prettierArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Prettier formatting issues"
        if (-not $Fix) { Write-Hint "Run 'dev lint --fix' to auto-format" }
        $prettierOk = $false
    } else {
        Write-Success "Prettier OK"
        $prettierOk = $true
    }

    Pop-Location
    return ($eslintOk -and $prettierOk)
}
