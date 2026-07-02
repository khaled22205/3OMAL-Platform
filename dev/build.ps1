. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-BuildCmd {
    param(
        [string]$Target = "all",
        [string]$Configuration = "Release",
        [switch]$Watch,
        [switch]$Ci
    )
    switch ($Target) {
        "all" { Build-All -Configuration $Configuration -Watch:$Watch -Ci:$Ci }
        "back" { Build-Backend -Configuration $Configuration -Ci:$Ci }
        "front" { Build-Frontend -Configuration $Configuration -Watch:$Watch -Ci:$Ci }
        default { Write-Error "Unknown build target: $Target. Use: back, front, or all"; exit 1 }
    }
}

function Build-All {
    param([string]$Configuration, [switch]$Watch, [switch]$Ci)
    Write-Section "Building All"
    Start-Timer
    $ok = $true
    if (-not (Build-Backend -Configuration $Configuration -Ci:$Ci)) { $ok = $false }
    if (-not (Build-Frontend -Configuration $Configuration -Watch:$Watch -Ci:$Ci)) { $ok = $false }
    Stop-Timer
    if ($ok) { Write-Success "All builds completed" } else { Write-Error "Build failed"; exit 1 }
}

function Build-Backend {
    param([string]$Configuration, [switch]$Ci)
    Write-Info "Building backend ($Configuration)..."
    Start-Timer

    $restoreArgs = @("restore", "`"$Script:SOLUTION_FILE`"")
    if ($Ci) { $restoreArgs += "--verbosity", "normal" }

    $restoreResult = & dotnet $restoreArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "NuGet restore failed"
        $restoreResult | ForEach-Object { Write-Host "  $_" }
        Stop-Timer
        return $false
    }
    Write-Success "Packages restored"

    $buildArgs = @("build", "`"$Script:SOLUTION_FILE`"", "--no-restore", "--configuration", $Configuration)
    if ($Ci) { $buildArgs += "--verbosity", "normal" }

    $buildResult = & dotnet $buildArgs 2>&1
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Stop-Timer
        Write-Success "Backend build succeeded"
        return $true
    } else {
        $errors = $buildResult | Select-String "error CS"
        $warnings = $buildResult | Select-String "warning CS"
        Write-Fail "Backend build failed ($exitCode errors)"
        if ($errors) { $errors | ForEach-Object { Write-Host "  $($Script:Colors.Red)$_$($Script:Colors.Reset)" } }
        if ($warnings) { Write-Host "  $($Script:Colors.Yellow)$($warnings.Count) warnings$($Script:Colors.Reset)" }
        Stop-Timer
        return $false
    }
}

function Build-Frontend {
    param([string]$Configuration, [switch]$Watch, [switch]$Ci)
    Write-Info "Building frontend ($Configuration)..."
    Start-Timer

    Push-Location $Script:FRONT_DIR

    if (-not (Test-Path "node_modules")) {
        Write-Info "Installing npm packages..."
        $npmInstall = if ($Ci) { "ci" } else { "install" }
        & npm $npmInstall 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warn "npm install had issues" }
    }

    if ($Watch) {
        Write-Info "Starting frontend watch mode..."
        & npx ng build --watch --configuration development 2>&1
        return $true
    }

    $ngConfig = if ($Configuration -eq "Release" -or $Configuration -eq "production") { "production" } else { "development" }
    $buildResult = & npx ng build --configuration $ngConfig 2>&1
    $exitCode = $LASTEXITCODE

    Pop-Location

    if ($exitCode -eq 0) {
        Stop-Timer
        Write-Success "Frontend build succeeded"
        return $true
    } else {
        Write-Fail "Frontend build failed ($exitCode)"
        $buildResult | Select-String "error" | ForEach-Object { Write-Host "  $($Script:Colors.Red)$_$($Script:Colors.Reset)" }
        Stop-Timer
        return $false
    }
}
