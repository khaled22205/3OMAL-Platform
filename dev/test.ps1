. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-TestCmd {
    param(
        [string]$Target = "all",
        [string]$Filter,
        [switch]$Coverage,
        [switch]$Ci
    )
    switch ($Target) {
        "all" { Run-AllTests -Filter $Filter -Coverage:$Coverage -Ci:$Ci }
        "back" { Run-BackendTests -Filter $Filter -Coverage:$Coverage -Ci:$Ci }
        "front" { Run-FrontendTests -Coverage:$Coverage -Ci:$Ci }
        default { Write-Error "Unknown test target: $Target. Use: back, front, or all"; exit 1 }
    }
}

function Run-AllTests {
    param([string]$Filter, [switch]$Coverage, [switch]$Ci)
    Write-Section "Running All Tests"
    Start-Timer
    $ok = $true
    if (-not (Run-BackendTests -Filter $Filter -Coverage:$Coverage -Ci:$Ci)) { $ok = $false }
    if (-not (Run-FrontendTests -Coverage:$Coverage -Ci:$Ci)) { $ok = $false }
    Stop-Timer
    if ($ok) { Write-Success "All tests passed" } else { Write-Error "Some tests failed"; exit 1 }
}

function Run-BackendTests {
    param([string]$Filter, [switch]$Coverage, [switch]$Ci)
    Write-Info "Running backend tests..."
    Start-Timer

    Push-Location $Script:BACK_DIR
    $testArgs = @("test", "`"$Script:SOLUTION_FILE`"", "--verbosity", "normal")
    if ($Filter) { $testArgs += "--filter", "`"$Filter`"" }
    if ($Ci) { $testArgs += "--logger", "trx" }

    if ($Coverage) {
        $testArgs += "/p:CollectCoverage=true"
        $testArgs += "/p:CoverletOutputFormat=opencover"
        $testArgs += "/p:CoverletOutput=../../coverage/"
        $testArgs += "/p:ExcludeByAttribute=Obsolete"
        $testArgs += "/p:Exclude=[*]Infrastructure.Data.Migrations.*"
    }

    $result = & dotnet $testArgs 2>&1
    $exitCode = $LASTEXITCODE
    Pop-Location

    $passed = $result | Select-String "passed" | Select-Object -Last 1
    $failed = $result | Select-String "failed" | Select-Object -Last 1
    $summary = if ($passed) { $passed.ToString().Trim() } else { "No test summary" }

    Stop-Timer
    if ($exitCode -eq 0) {
        Write-Success "Backend tests: $summary"
        return $true
    } else {
        Write-Fail "Backend tests failed"
        $result | Select-String "failed" | ForEach-Object { Write-Host "  $_" }
        return $false
    }
}

function Run-FrontendTests {
    param([switch]$Coverage, [switch]$Ci)
    Write-Info "Running frontend tests..."
    Start-Timer

    Push-Location $Script:FRONT_DIR
    $testArgs = @("vitest", "run")
    if ($Coverage) { $testArgs += "--coverage" }

    $result = & npx $testArgs 2>&1
    $exitCode = $LASTEXITCODE
    Pop-Location

    Stop-Timer
    if ($exitCode -eq 0) {
        Write-Success "Frontend tests passed"
        return $true
    } else {
        Write-Fail "Frontend tests failed"
        $result | ForEach-Object { Write-Host "  $_" }
        return $false
    }
}
