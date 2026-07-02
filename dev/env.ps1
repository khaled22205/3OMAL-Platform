. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-EnvCmd {
    param(
        [string]$Action,
        [string]$EnvName,
        [switch]$Ci
    )

    switch ($Action) {
        "show" { Show-Environment }
        "switch" { Switch-Environment -EnvName $EnvName -Ci:$Ci }
        "list" { List-Environments }
        default { Write-Error "Unknown env action: $Action"; exit 1 }
    }
}

function Show-Environment {
    $current = Get-Environment
    Write-Section "Environment"
    Write-Host "  Active:  $($Script:Colors.Bold)$current$($Script:Colors.Reset)"
    Write-Host "  Default: $($Script:DEFAULT_ENV)"
    Write-Host "  Backend URL:  http://localhost:$($Script:BACKEND_PORT)"
    Write-Host "  Frontend URL: http://localhost:$($Script:FRONTEND_PORT)"
    Write-Host "  Config file:  appsettings.$current.json"
    Write-Host "  Supported: $($Script:SUPPORTED_ENVS -join ', ')"
}

function Switch-Environment {
    param([string]$EnvName, [switch]$Ci)
    if (-not $EnvName) { Write-Error "Usage: dev env switch <environment>"; return }
    if ($EnvName -notin $Script:SUPPORTED_ENVS) {
        Write-Error "Invalid environment '$EnvName'. Supported: $($Script:SUPPORTED_ENVS -join ', ')"
        exit 1
    }
    Set-Environment $EnvName
    Write-Success "Switched to '$EnvName' environment"
}

function List-Environments {
    Write-Info "Available environments:"
    foreach ($env in $Script:SUPPORTED_ENVS) {
        $marker = if ($env -eq (Get-Environment)) { " <-- active" } else { "" }
        Write-Host "  - $env$marker"
    }
}

function Validate-Environment {
    $env = Get-Environment
    Write-Info "Validating environment: $env"
    $errors = @()
    $cs = Get-ConnectionString
    if (-not $cs) { $errors += "Connection string not found in appsettings.json" }
    if (-not (Get-ConfigValue "Jwt:Key")) { $errors += "JWT signing key not configured" }
    if ($errors.Count -gt 0) {
        Write-Error "Environment validation failed:"
        $errors | ForEach-Object { Write-Fail $_ }
        return $false
    }
    Write-Success "Environment '$env' is valid"
    return $true
}

function Ensure-EnvironmentFile {
    param([string]$Env)
    $envFile = "$Script:API_DIR/appsettings.$Env.json"
    if (-not (Test-Path $envFile)) {
        Write-Warn "Config file not found: appsettings.$Env.json — creating from template"
        $template = @{}
        $template | ConvertTo-Json | Set-Content $envFile -Encoding UTF8
        Write-Success "Created $envFile"
    }
}
