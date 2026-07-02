. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-DbCmd {
    param(
        [string]$Action,
        [string]$Name,
        [switch]$Ci
    )
    switch ($Action) {
        "create" { Invoke-DbCreate -Ci:$Ci }
        "drop" { Invoke-DbDrop -Ci:$Ci }
        "migrate" { Invoke-DbMigrate -Ci:$Ci }
        "add" { Invoke-DbAddMigration -Name $Name -Ci:$Ci }
        "remove" { Invoke-DbRemoveMigration -Ci:$Ci }
        "list" { Invoke-DbListMigrations }
        "pending" { Invoke-DbPendingMigrations }
        default { Write-Error "Unknown db action: $Action"; exit 1 }
    }
}

function Invoke-DbCreate {
    param([switch]$Ci)
    Write-Section "Create Database"
    Start-Timer

    if (-not (Test-DbConnection)) {
        if (-not $Ci) {
            $confirm = Read-Host "Database not reachable. Attempt to create? (y/N)"
            if ($confirm -ne "y") { Write-Info "Aborted"; return }
        } else {
            Write-Error "Database not reachable"; exit 1
        }
    }

    & dotnet ef database update --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Stop-Timer
        Write-Success "Database created and migrations applied"
    } else {
        Stop-Timer
        Write-Error "Failed to create database"
        exit 1
    }
}

function Invoke-DbDrop {
    param([switch]$Ci)
    Write-Section "Drop Database"
    Write-Warn "This will PERMANENTLY delete all data!"

    if (-not $Ci) {
        $confirm = Read-Host "Are you sure you want to drop the database? (type 'yes' to confirm)"
        if ($confirm -ne "yes") { Write-Info "Aborted"; return }
    }

    Start-Timer
    & dotnet ef database drop --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" --force 2>&1
    if ($LASTEXITCODE -eq 0) {
        Stop-Timer
        Write-Success "Database dropped"
    } else {
        Stop-Timer
        Write-Error "Failed to drop database"
        exit 1
    }
}

function Invoke-DbMigrate {
    param([switch]$Ci)
    Write-Section "Apply Migrations"
    Start-Timer
    & dotnet ef database update --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Stop-Timer
        Write-Success "Migrations applied"
    } else {
        Stop-Timer
        Write-Error "Migration failed"
        exit 1
    }
}

function Invoke-DbAddMigration {
    param([string]$Name, [switch]$Ci)
    if (-not $Name) {
        if ($Ci) { Write-Error "Migration name required: dev db add <name>"; exit 1 }
        $Name = Read-Host "Enter migration name"
    }
    Write-Section "Add Migration: $Name"
    Start-Timer
    & dotnet ef migrations add $Name --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Stop-Timer
        Write-Success "Migration '$Name' added"
    } else {
        Stop-Timer
        Write-Error "Failed to add migration"
        exit 1
    }
}

function Invoke-DbRemoveMigration {
    param([switch]$Ci)
    Write-Section "Remove Last Migration"
    if (-not $Ci) {
        $confirm = Read-Host "Remove the last migration? (y/N)"
        if ($confirm -ne "y") { Write-Info "Aborted"; return }
    }
    Start-Timer
    & dotnet ef migrations remove --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Stop-Timer
        Write-Success "Last migration removed"
    } else {
        Stop-Timer
        Write-Error "Failed to remove migration"
        exit 1
    }
}

function Invoke-DbListMigrations {
    Write-Section "Migrations"
    & dotnet ef migrations list --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
}

function Invoke-DbPendingMigrations {
    Write-Section "Pending Migrations"
    $result = & dotnet ef migrations list --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    $pending = $result | Where-Object { $_ -match "^[0-9]{14}" }
    if ($pending) {
        Write-Info "Pending migrations:"
        $pending | ForEach-Object { Write-Host "  - $_" }
    } else {
        Write-Success "No pending migrations"
    }
}

function Test-DbConnection {
    try {
        $cs = Get-ConnectionString
        if (-not $cs) { return $false }
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        $conn.Close()
        return $true
    } catch {
        return $false
    }
}

function Reset-Database {
    param([switch]$Ci)
    Write-Section "Reset Database"
    Write-Warn "This will DROP and recreate the database!"

    if (-not $Ci) {
        $confirm = Read-Host "Are you sure? Type 'reset' to confirm"
        if ($confirm -ne "reset") { Write-Info "Aborted"; return }
    }

    Start-Timer
    & dotnet ef database drop --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" --force 2>&1 | Out-Null
    Write-Info "Database dropped"
    & dotnet ef database update --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Stop-Timer
        Write-Error "Migration failed"
        exit 1
    }
    Write-Info "Migrations applied"
    Stop-Timer
    Write-Success "Database reset complete"
}
