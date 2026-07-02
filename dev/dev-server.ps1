. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-RunCmd {
    param(
        [string]$Target = "all",
        [switch]$NoBrowser,
        [switch]$Ci
    )
    switch ($Target) {
        "all" { Run-All -NoBrowser:$NoBrowser }
        "back" { Run-Backend }
        "front" { Run-Frontend -NoBrowser:$NoBrowser }
        default { Write-Error "Unknown run target: $Target. Use: back, front, or all"; exit 1 }
    }
}

function Run-All {
    param([switch]$NoBrowser)
    Write-Section "Starting Development Servers"
    Write-Info "Backend:  http://localhost:$($Script:BACKEND_PORT)"
    Write-Info "Frontend: http://localhost:$($Script:FRONTEND_PORT)"
    Write-Info "Swagger:  $($Script:SWAGGER_URL)"
    Write-Hint "Press Ctrl+C to stop both servers"

    if (-not $NoBrowser) {
        Start-Process $Script:SWAGGER_URL
    }

    $backendJob = Start-Job -ScriptBlock {
        param($dir, $port)
        Set-Location $dir
        $env:ASPNETCORE_URLS = "http://localhost:$port"
        dotnet run --urls "http://localhost:$port"
    } -ArgumentList $Script:API_DIR, $Script:BACKEND_PORT

    $frontendJob = Start-Job -ScriptBlock {
        param($dir)
        Set-Location $dir
        ng serve
    } -ArgumentList $Script:FRONT_DIR

    try {
        while ($true) {
            Start-Sleep 1
            Receive-Job $backendJob
            Receive-Job $frontendJob
            if ($backendJob.State -eq "Failed" -or $frontendJob.State -eq "Failed") {
                Write-Error "A server has stopped unexpectedly"
                break
            }
        }
    } finally {
        Stop-Job $backendJob -ErrorAction SilentlyContinue
        Stop-Job $frontendJob -ErrorAction SilentlyContinue
        Remove-Job $backendJob -ErrorAction SilentlyContinue
        Remove-Job $frontendJob -ErrorAction SilentlyContinue
    }
}

function Run-Backend {
    Write-Section "Starting Backend Server"
    Write-Info "API: http://localhost:$($Script:BACKEND_PORT)"
    Write-Info "Swagger: $($Script:SWAGGER_URL)"
    Write-Hint "Press Ctrl+C to stop"

    Start-Process $Script:SWAGGER_URL
    Push-Location $Script:API_DIR
    $env:ASPNETCORE_URLS = "http://localhost:$($Script:BACKEND_PORT)"
    & dotnet run --urls "http://localhost:$($Script:BACKEND_PORT)"
    Pop-Location
}

function Run-Frontend {
    param([switch]$NoBrowser)
    Write-Section "Starting Frontend Dev Server"
    Write-Info "Angular: http://localhost:$($Script:FRONTEND_PORT)"

    if (-not $NoBrowser) {
        Start-Process "http://localhost:$($Script:FRONTEND_PORT)"
    }

    Push-Location $Script:FRONT_DIR
    & ng serve
    Pop-Location
}
