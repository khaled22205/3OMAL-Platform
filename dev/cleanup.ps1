. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-CleanCmd {
    param(
        [string]$Scope = "all",
        [switch]$Ci
    )

    if (-not $Ci) {
        Write-Warn "This will remove build artifacts, packages, and caches"
        $confirm = Read-Host "Continue? (y/N)"
        if ($confirm -ne "y") { Write-Info "Aborted"; return }
    }

    Write-Section "Cleanup"
    Start-Timer

    switch ($Scope) {
        "all" {
            Clean-BinObj
            Clean-NodeModules
            Clean-Dist
            Clean-NugetCache
            Clean-NpmCache
            Clean-TestResults
            Clean-Logs
        }
        "back" { Clean-BinObj }
        "front" { Clean-NodeModules; Clean-Dist }
        "packages" { Clean-NugetCache; Clean-NpmCache }
        "artifacts" { Clean-BinObj; Clean-Dist; Clean-TestResults }
        default { Write-Error "Unknown clean scope: $Scope"; exit 1 }
    }

    Stop-Timer
    Write-Success "Cleanup complete"
}

function Clean-BinObj {
    Write-Info "Removing bin/obj folders..."
    Get-ChildItem -Path $Script:BACK_DIR -Recurse -Directory -Include "bin", "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Write-Success "bin/obj removed"
}

function Clean-NodeModules {
    $nm = "$Script:FRONT_DIR/node_modules"
    if (Test-Path $nm) {
        Write-Info "Removing node_modules..."
        Remove-Item -Path $nm -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "node_modules removed"
    }
}

function Clean-Dist {
    $dist = "$Script:FRONT_DIR/dist"
    if (Test-Path $dist) {
        Write-Info "Removing dist..."
        Remove-Item -Path $dist -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "dist removed"
    }
}

function Clean-NugetCache {
    Write-Info "Clearing NuGet cache..."
    & dotnet nuget locals all --clear 2>&1 | Out-Null
    Write-Success "NuGet cache cleared"
}

function Clean-NpmCache {
    Write-Info "Clearing npm cache..."
    & npm cache clean --force 2>&1 | Out-Null
    Write-Success "npm cache cleared"
}

function Clean-TestResults {
    Get-ChildItem -Path $Script:BACK_DIR -Recurse -Directory -Include "TestResults" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    $coverage = "$Script:BACK_DIR/coverage"
    if (Test-Path $coverage) { Remove-Item -Path $coverage -Recurse -Force -ErrorAction SilentlyContinue }
    Write-Success "Test artifacts removed"
}

function Clean-Logs {
    $logDir = "$PSScriptRoot/logs"
    if (Test-Path $logDir) {
        Get-ChildItem -Path $logDir -Filter "*.log" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
        Write-Success "Logs cleared"
    }
}
