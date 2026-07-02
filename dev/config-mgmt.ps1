. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-ConfigCmd {
    param(
        [string]$Action,
        [string]$Key,
        [string]$Value,
        [switch]$Ci
    )
    switch ($Action) {
        "show" { Show-Config }
        "backup" { Backup-Config -Ci:$Ci }
        "restore" { Restore-Config -Ci:$Ci }
        "set" { Set-ConfigValue -Key $Key -Value $Value -Ci:$Ci }
        default { Write-Error "Unknown config action: $Action"; exit 1 }
    }
}

function Show-Config {
    Write-Section "Configuration"
    $appSettings = "$Script:API_DIR/appsettings.json"
    if (Test-Path $appSettings) {
        $config = Get-Content $appSettings -Raw | ConvertFrom-Json
        Write-Info "appsettings.json:"
        $config.PSObject.Properties | ForEach-Object {
            $val = $_.Value
            if ($_.Name -eq "Jwt" -and $val.Key) {
                Write-Host "  $($Script:Colors.Cyan)$($_.Name):$($Script:Colors.Reset)"
                Write-Host "    Key = ******** (hidden)"
                Write-Host "    Issuer = $($val.Issuer)"
                Write-Host "    Audience = $($val.Audience)"
            } elseif ($_.Name -eq "ConnectionStrings") {
                Write-Host "  $($Script:Colors.Cyan)$($_.Name):$($Script:Colors.Reset)"
                Write-Host "    DefaultConnection = $($val.DefaultConnection)"
            } else {
                Write-Host "  $($Script:Colors.Cyan)$($_.Name):$($Script:Colors.Reset) $($val | ConvertTo-Json -Compress)"
            }
        }
    }

    $envFile = "$Script:API_DIR/appsettings.$(Get-Environment).json"
    if (Test-Path $envFile) {
        Write-Info "appsettings.$(Get-Environment).json:"
        $envConfig = Get-Content $envFile -Raw | ConvertFrom-Json
        $envConfig.PSObject.Properties | ForEach-Object {
            Write-Host "  $($Script:Colors.Cyan)$($_.Name):$($Script:Colors.Reset) $($_.Value | ConvertTo-Json -Compress)"
        }
    }
}

function Backup-Config {
    param([switch]$Ci)
    $backupDir = "$Script:ROOT_DIR/config-backups"
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = "$backupDir/$timestamp"
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null

    Copy-Item "$Script:API_DIR/appsettings.json" "$backupPath/appsettings.json" -Force
    Copy-Item "$Script:API_DIR/appsettings.Development.json" "$backupPath/appsettings.Development.json" -Force
    Copy-Item "$Script:FRONT_DIR/src/environments/environment.ts" "$backupPath/environment.ts" -Force
    Copy-Item "$Script:FRONT_DIR/src/environments/environment.prod.ts" "$backupPath/environment.prod.ts" -Force

    Write-Success "Config backed up to: $backupPath"
    return $backupPath
}

function Restore-Config {
    param([switch]$Ci)
    $backupDir = "$Script:ROOT_DIR/config-backups"
    if (-not (Test-Path $backupDir)) {
        Write-Error "No backups found"; exit 1
    }
    $backups = Get-ChildItem $backupDir -Directory | Sort-Object LastWriteTime -Descending
    if ($backups.Count -eq 0) { Write-Error "No backups found"; exit 1 }

    Write-Info "Available backups:"
    for ($i = 0; $i -lt [Math]::Min($backups.Count, 10); $i++) {
        Write-Host "  [$($i+1)] $($backups[$i].Name)"
    }

    if (-not $Ci) {
        $choice = Read-Host "Select backup to restore (1-$([Math]::Min($backups.Count, 10)))"
        $idx = [int]$choice - 1
        if ($idx -lt 0 -or $idx -ge $backups.Count) { Write-Error "Invalid selection"; exit 1 }
        $selected = $backups[$idx]
    } else {
        $selected = $backups[0]
    }

    $srcDir = $selected.FullName
    if (Test-Path "$srcDir/appsettings.json") { Copy-Item "$srcDir/appsettings.json" "$Script:API_DIR/appsettings.json" -Force }
    if (Test-Path "$srcDir/appsettings.Development.json") { Copy-Item "$srcDir/appsettings.Development.json" "$Script:API_DIR/appsettings.Development.json" -Force }
    if (Test-Path "$srcDir/environment.ts") { Copy-Item "$srcDir/environment.ts" "$Script:FRONT_DIR/src/environments/environment.ts" -Force }
    if (Test-Path "$srcDir/environment.prod.ts") { Copy-Item "$srcDir/environment.prod.ts" "$Script:FRONT_DIR/src/environments/environment.prod.ts" -Force }

    Write-Success "Config restored from: $($selected.Name)"
}

function Set-ConfigValue {
    param([string]$Key, [string]$Value, [switch]$Ci)
    if (-not $Key -or -not $Value) {
        Write-Error "Usage: dev config set <key> <value>"; exit 1
    }
    $configPath = "$Script:API_DIR/appsettings.json"
    $config = Get-Content $configPath -Raw | ConvertFrom-Json

    $keys = $Key -split ':'
    $current = $config
    for ($i = 0; $i -lt $keys.Count - 1; $i++) {
        if ($null -eq $current.$($keys[$i])) {
            $current | Add-Member -MemberType NoteProperty -Name $keys[$i] -Value @{}
        }
        $current = $current.$($keys[$i])
    }
    $lastKey = $keys[-1]
    $current.$lastKey = $Value

    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath -Encoding UTF8
    Write-Success "Set $Key = $Value"
}
