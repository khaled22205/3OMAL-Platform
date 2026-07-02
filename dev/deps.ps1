. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-DepsCmd {
    param(
        [string]$Action,
        [switch]$Ci
    )
    switch ($Action) {
        "" { Check-AllDependencies -Ci:$Ci }
        "check" { Check-AllDependencies -Ci:$Ci }
        "outdated" { Check-OutdatedPackages }
        "update" { Update-Dependencies }
        default { Write-Error "Unknown deps action: $Action"; exit 1 }
    }
}

function Check-AllDependencies {
    param([switch]$Ci)
    Write-Section "Dependency Check"
    $allOk = $true
    Start-Timer

    if (-not (Check-DotNetSdk)) { $allOk = $false }
    if (-not (Check-NodeJs)) { $allOk = $false }
    if (-not (Check-Npm)) { $allOk = $false }
    if (-not (Check-AngularCli)) { $allOk = $false }
    if (-not (Check-DotNetTools)) { $allOk = $false }
    if (-not (Check-SqlServer)) { $allOk = $false }

    Stop-Timer
    if ($allOk) { Write-Success "All dependencies satisfied" }
    else { Write-Warn "Some dependencies need attention" }
    return $allOk
}

function Check-DotNetSdk {
    Write-Info "Checking .NET SDK..."
    try {
        $version = dotnet --version 2>$null
        if (-not $version) { throw "not found" }
        $major = $version.Split('.')[0]
        if ($major -ne "10") {
            Write-Warn ".NET SDK v$version found, v10.x recommended"
            return $false
        }
        Write-Success ".NET SDK v$version"
        return $true
    } catch {
        Write-Fail ".NET SDK not installed. Install from: https://dotnet.microsoft.com/download"
        return $false
    }
}

function Check-NodeJs {
    Write-Info "Checking Node.js..."
    try {
        $version = node --version 2>$null
        if (-not $version) { throw "not found" }
        $verNum = $version -replace 'v',''
        $major = $verNum.Split('.')[0]
        if ([int]$major -lt 22) {
            Write-Warn "Node.js $version found, v22+ recommended"
            return $false
        }
        Write-Success "Node.js $version"
        return $true
    } catch {
        Write-Fail "Node.js not installed. Install from: https://nodejs.org"
        return $false
    }
}

function Check-Npm {
    Write-Info "Checking npm..."
    try {
        $version = npm --version 2>$null
        if (-not $version) { throw "not found" }
        Write-Success "npm v$version"
        return $true
    } catch {
        Write-Fail "npm not installed"
        return $false
    }
}

function Check-AngularCli {
    Write-Info "Checking Angular CLI..."
    try {
        $version = ng version 2>$null | Select-String "Angular CLI:"
        if (-not $version) { throw "not found" }
        Write-Success "Angular CLI: $version"
        return $true
    } catch {
        $localVer = & npx ng version 2>$null | Select-String "Angular CLI:"
        if ($localVer) {
            Write-Success "Angular CLI (local): $localVer"
            return $true
        }
        Write-Warn "Angular CLI not found globally. Install: npm install -g @angular/cli"
        return $false
    }
}

function Check-DotNetTools {
    Write-Info "Checking .NET tools..."
    try {
        $tools = dotnet tool list --global 2>$null
        $hasEf = $tools | Select-String "dotnet-ef"
        if (-not $hasEf) {
            Write-Warn "dotnet-ef not installed globally. Install: dotnet tool install --global dotnet-ef"
            return $false
        }
        Write-Success "dotnet-ef found"
        return $true
    } catch {
        Write-Warn "Could not check .NET tools. Install: dotnet tool install --global dotnet-ef"
        return $false
    }
}

function Check-SqlServer {
    Write-Info "Checking SQL Server connectivity..."
    try {
        $cs = Get-ConnectionString
        if (-not $cs) { throw "No connection string configured" }
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        $conn.Close()
        Write-Success "SQL Server reachable"
        return $true
    } catch {
        Write-Warn "SQL Server not reachable: $_"
        return $false
    }
}

function Check-OutdatedPackages {
    Write-Section "Outdated Packages"
    Start-Timer

    Write-Info "Checking NuGet packages..."
    Push-Location $Script:BACK_DIR
    dotnet list package --outdated 2>$null
    Pop-Location

    Write-Info "Checking npm packages..."
    Push-Location $Script:FRONT_DIR
    npm outdated 2>$null
    Pop-Location

    Stop-Timer
}

function Update-Dependencies {
    Write-Section "Update Dependencies"

    Write-Info "Updating NuGet packages..."
    Push-Location $Script:BACK_DIR
    dotnet restore 2>$null
    dotnet list package --outdated 2>$null
    Pop-Location

    Write-Info "Updating npm packages..."
    Push-Location $Script:FRONT_DIR
    npm update 2>$null
    Pop-Location

    Write-Success "Dependencies updated"
}
