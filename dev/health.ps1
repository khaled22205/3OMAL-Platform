. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-HealthCmd {
    param([switch]$Ci)
    Write-Section "System Health Check"
    $allOk = $true
    $results = @{}
    Start-Timer

    $results["dotnet-sdk"] = Test-DotNetSdk
    $results["nodejs"] = Test-NodeVersion
    $results["angular-cli"] = Test-AngularCli
    $results["sql-server"] = Test-SqlConnection
    $results["database"] = Test-DatabaseAccess
    $results["ports"] = Test-RequiredPorts
    $results["https-cert"] = Test-HttpsCert
    $results["jwt-key"] = Test-JwtKey
    $results["signalr"] = Test-SignalRConfig
    $results["env-vars"] = Test-EnvVars

    Stop-Timer
    Write-Section "Health Report"
    $passCount = 0
    $failCount = 0
    foreach ($check in $results.Keys) {
        $status = $results[$check]
        if ($status -eq $true) {
            Write-Host "  $($Script:Colors.Green)✓ PASS$($Script:Colors.Reset)  $check"
            $passCount++
        } elseif ($status -eq $null) {
            Write-Host "  $($Script:Colors.Yellow)⚠ SKIP$($Script:Colors.Reset)  $check"
        } else {
            Write-Host "  $($Script:Colors.Red)✗ FAIL$($Script:Colors.Reset)  $check"
            $failCount++
            $allOk = $false
        }
    }
    Write-Host "`n  $passCount passed, $failCount failed"
    if ($allOk) { Write-Success "System healthy" }
    else { Write-Warn "$failCount checks failed"; if ($Ci) { exit 1 } }
    return $allOk
}

function Test-DotNetSdk {
    try {
        $v = dotnet --version 2>$null
        if ($v -and $v.StartsWith("10")) { return $true }
        Write-Fail ".NET SDK: $v (10.x required)"
        return $false
    } catch { Write-Fail ".NET SDK: not found"; return $false }
}

function Test-NodeVersion {
    try {
        $v = node --version 2>$null
        if ($v -and $v -match "v(\d+)") {
            if ([int]$matches[1] -ge 22) { return $true }
            Write-Warn "Node.js $v (22+ recommended)"
            return $true
        }
        Write-Fail "Node.js: not found"
        return $false
    } catch { Write-Fail "Node.js: not found"; return $false }
}

function Test-AngularCli {
    try {
        $v = ng version 2>$null | Select-String "Angular CLI:"
        if ($v) { return $true }
        $v2 = & npx ng version 2>$null | Select-String "Angular CLI:"
        if ($v2) { return $true }
        Write-Warn "Angular CLI: not found (local or global)"
        return $null
    } catch { Write-Warn "Angular CLI: error checking"; return $null }
}

function Test-SqlConnection {
    try {
        $cs = Get-ConnectionString
        if (-not $cs) { Write-Warn "SQL Server: no connection string"; return $null }
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        $conn.Close()
        return $true
    } catch { Write-Warn "SQL Server: unreachable ($_ )"; return $false }
}

function Test-DatabaseAccess {
    try {
        $cs = Get-ConnectionString
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo'"
        $tables = $cmd.ExecuteScalar()
        $conn.Close()
        Write-Info "  Database contains $tables tables"
        return $true
    } catch { Write-Warn "Database: cannot access schema"; return $false }
}

function Test-RequiredPorts {
    $allFree = $true
    $ports = @($Script:BACKEND_PORT, $Script:BACKEND_HTTPS_PORT, $Script:FRONTEND_PORT)
    foreach ($port in $ports) {
        $inUse = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($inUse) { Write-Warn "Port $port: in use"; $allFree = $false }
        else { Write-Info "  Port $port: available" }
    }
    return $allFree
}

function Test-HttpsCert {
    try {
        $result = & dotnet dev-certs https --check 2>&1
        if ($LASTEXITCODE -eq 0) { return $true }
        Write-Warn "HTTPS cert: not trusted. Run: dotnet dev-certs https --trust"
        return $false
    } catch { Write-Warn "HTTPS cert: could not check"; return $null }
}

function Test-JwtKey {
    $key = Get-ConfigValue "Jwt:Key"
    if ($key -and $key.Length -ge 16) { return $true }
    Write-Fail "JWT key: missing or too short (< 16 chars)"
    return $false
}

function Test-SignalRConfig {
    $backplane = Get-ConfigValue "SignalR:Backplane"
    $maxSize = Get-ConfigValue "SignalR:MaximumReceiveMessageSize"
    if ($maxSize) { Write-Info "  SignalR max message size: $maxSize bytes" }
    if ($backplane) { Write-Info "  SignalR backplane: $backplane" }
    else { Write-Info "  SignalR backplane: none (single instance)" }
    return $true
}

function Test-EnvVars {
    $allOk = $true
    $vars = @("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT")
    foreach ($var in $vars) {
        if ([Environment]::GetEnvironmentVariable($var)) {
            Write-Info "  $var = $([Environment]::GetEnvironmentVariable($var))"
        } else {
            Write-Info "  $var: not set (using default)"
        }
    }
    return $allOk
}
