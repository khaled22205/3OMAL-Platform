#!/usr/bin/env pwsh
<#
.SYNOPSIS
    3OMAL-Platform Developer CLI — Automate build, test, database, seed, lint, and dev workflows.
.DESCRIPTION
    Unified command-line tool for managing the 3OMAL full-stack application (ASP.NET Core + Angular).
    Supports interactive and CI mode.
.PARAMETER Command
    The command to execute (setup, build, test, seed, clean, reset, db, run, lint, health, env, deps, audit, config).
.PARAMETER Subcommand
    The subcommand or target.
.PARAMETER Help
    Show help for a command.
.PARAMETER Ci
    Non-interactive mode for CI/CD pipelines.
.PARAMETER Verbose
    Enable verbose logging.
.PARAMETER Debug
    Enable debug logging.
.PARAMETER Quiet
    Suppress non-essential output.
.EXAMPLE
    .\dev.ps1 setup
    .\dev.ps1 build back --configuration Release
    .\dev.ps1 test all --coverage
    .\dev.ps1 db add-migration InitialCreate
    .\dev.ps1 seed --workers 100 --customers 500 --seed 42
    .\dev.ps1 run all
    .\dev.ps1 health
    .\dev.ps1 clean all --ci
#>

param(
    [string]$Command = "",
    [string]$Subcommand = "",
    [string[]]$Args = @(),
    [switch]$Help,
    [switch]$Ci,
    [switch]$Verbose,
    [switch]$Debug,
    [switch]$Quiet
)

$Script:RootDir = Split-Path $MyInvocation.MyCommand.Path -Parent

. "$Script:RootDir/dev/config.ps1"
. "$Script:RootDir/dev/logging.ps1"
. "$Script:RootDir/dev/env.ps1"
. "$Script:RootDir/dev/deps.ps1"
. "$Script:RootDir/dev/build.ps1"
. "$Script:RootDir/dev/database.ps1"
. "$Script:RootDir/dev/seed.ps1"
. "$Script:RootDir/dev/test.ps1"
. "$Script:RootDir/dev/lint.ps1"
. "$Script:RootDir/dev/health.ps1"
. "$Script:RootDir/dev/cleanup.ps1"
. "$Script:RootDir/dev/dev-server.ps1"
. "$Script:RootDir/dev/config-mgmt.ps1"
. "$Script:RootDir/dev/audit.ps1"

# Set log level based on flags
if ($Debug) { Set-LogLevel "DEBUG"; Set-DebugMode }
if ($Verbose) { Set-LogLevel "DEBUG" }
if ($Quiet) { Set-QuietMode }
if ($Ci) { $env:CI = "true" }

Init-LogFile

# --- Parse positional arguments ---
$remainingArgs = @()
$commandArgs = @()
$parsedCommand = ""
$parsedSubcommand = ""

# Collect all positional args from bound parameters + extra args
$allArgs = @()
if ($Command) { $allArgs += $Command }
if ($Subcommand) { $allArgs += $Subcommand }
$allArgs += $Args

# Also re-parse $args to handle "dev build back" style
# Actually PowerShell binds first two positional params to $Command and $Subcommand
# Remaining go to $Args array
$i = 0
$tokens = @()
if ($Command) { $tokens += $Command }
if ($Subcommand) { $tokens += $Subcommand }
$tokens += $Args

if ($tokens.Count -gt 0) {
    $parsedCommand = $tokens[0]
    if ($tokens.Count -gt 1 -and $tokens[1] -notmatch "^-") {
        $parsedSubcommand = $tokens[1]
        $commandArgs = $tokens[2..($tokens.Count-1)]
    } else {
        $commandArgs = $tokens[1..($tokens.Count-1)]
    }
}

# --- Show help ---
if ($Help -or $parsedCommand -eq "help" -or ($parsedCommand -eq "" -and !$Help)) {
    Show-Help $parsedSubcommand
    exit 0
}

# --- Dispatch ---
Set-Environment (Get-Environment)
Write-Banner
Write-Info "Running: $parsedCommand $parsedSubcommand $($commandArgs -join ' ')"

try {
    switch ($parsedCommand) {
        "setup"    { Invoke-Setup -Ci:$Ci }
        "build"    { Invoke-BuildCmd -Target $parsedSubcommand -Ci:$Ci @ParseBuildArgs($commandArgs) }
        "test"     { Invoke-TestCmd -Target $parsedSubcommand -Ci:$Ci @ParseTestArgs($commandArgs) }
        "seed"     { $seedParams = Parse-SeedArgs $commandArgs; Invoke-SeedCmd -Counts $seedParams.Counts -RandomSeed $seedParams.Seed -Ci:$Ci }
        "clean"    { Invoke-CleanCmd -Scope $parsedSubcommand -Ci:$Ci }
        "reset"    { Reset-Database -Ci:$Ci; $seedParams = Parse-SeedArgs $commandArgs; if (-not $Ci) { $s = Read-Host "Seed database now? (Y/n)"; if ($s -ne "n") { Invoke-SeedCmd -Ci:$Ci } } }
        "db"       { Invoke-DbCmd -Action $parsedSubcommand -Name ($commandArgs[0]) -Ci:$Ci }
        "run"      { Invoke-RunCmd -Target $parsedSubcommand -Ci:$Ci @ParseRunArgs($commandArgs) }
        "start"    { Invoke-RunCmd -Target $parsedSubcommand -Ci:$Ci @ParseRunArgs($commandArgs) }
        "lint"     { Invoke-LintCmd -Ci:$Ci @ParseLintArgs($commandArgs) }
        "health"   { Invoke-HealthCmd -Ci:$Ci }
        "status"   { Invoke-HealthCmd -Ci:$Ci }
        "doctor"   { Invoke-HealthCmd -Ci:$Ci }
        "env"      { Invoke-EnvCmd -Action $parsedSubcommand -EnvName ($commandArgs[0]) -Ci:$Ci }
        "deps"     { Invoke-DepsCmd -Action $parsedSubcommand -Ci:$Ci }
        "audit"    { Invoke-AuditCmd -Ci:$Ci }
        "config"   { Invoke-ConfigCmd -Action $parsedSubcommand -Key ($commandArgs[0]) -Value ($commandArgs[1]) -Ci:$Ci }
        default {
            Write-Error "Unknown command: '$parsedCommand'"
            Show-Help
            exit 1
        }
    }
} catch {
    Write-Error "Command failed: $_"
    Write-Hint $_.ScriptStackTrace
    exit 1
}

exit 0

# ==================== Helper Functions ====================

function Show-Help {
    param([string]$Topic)

    if ($Topic) {
        Show-CommandHelp $Topic
        return
    }

    Write-Host @"
$($Script:Colors.Bold)$($Script:Colors.Cyan)3OMAL-Platform Developer CLI$($Script:Colors.Reset)
$($Script:Colors.Dim)Usage: dev <command> [subcommand] [options]$($Script:Colors.Reset)

$($Script:Colors.Bold)Commands:$($Script:Colors.Reset)
  $($Script:Colors.Green)setup$($Script:Colors.Reset)         First-time environment setup (deps, DB, seed)
  $($Script:Colors.Green)build$($Script:Colors.Reset)         Build [back|front|all] (default: all)
  $($Script:Colors.Green)test$($Script:Colors.Reset)          Run tests [back|front|all] (default: all)
  $($Script:Colors.Green)seed$($Script:Colors.Reset)          Seed database with fake data
  $($Script:Colors.Green)clean$($Script:Colors.Reset)         Clean [all|back|front|packages|artifacts]
  $($Script:Colors.Green)reset$($Script:Colors.Reset)         Drop DB → migrate → seed
  $($Script:Colors.Green)db$($Script:Colors.Reset)            Database management (see below)
  $($Script:Colors.Green)run$($Script:Colors.Reset)           Start dev servers [back|front|all]
  $($Script:Colors.Green)lint$($Script:Colors.Reset)          Lint and format code
  $($Script:Colors.Green)health$($Script:Colors.Reset)        System health check
  $($Script:Colors.Green)env$($Script:Colors.Reset)           Environment management
  $($Script:Colors.Green)deps$($Script:Colors.Reset)          Check dependencies
  $($Script:Colors.Green)audit$($Script:Colors.Reset)         Package vulnerability scan
  $($Script:Colors.Green)config$($Script:Colors.Reset)        Configuration management

$($Script:Colors.Bold)Database subcommands:$($Script:Colors.Reset)
  dev db create             Create database and apply migrations
  dev db drop               Drop database (with confirmation)
  dev db migrate            Apply pending migrations
  dev db add <name>         Add a new migration
  dev db remove             Remove the last migration
  dev db list               List all migrations
  dev db pending            Show pending migrations

$($Script:Colors.Bold)Options:$($Script:Colors.Reset)
  --ci            Non-interactive mode (no prompts, fail-fast)
  --verbose       Verbose logging
  --debug         Debug logging
  --quiet         Suppress non-essential output
  --help, -h      Show help

$($Script:Colors.Bold)Examples:$($Script:Colors.Reset)
  dev setup                          # Full first-time setup
  dev build back --release           # Build backend only
  dev test all --coverage            # Run all tests with coverage
  dev seed --workers 100 --seed 42   # Seed with deterministic data
  dev db add InitialCreate           # Create a migration
  dev run all                        # Start both servers
  dev health                         # Full system diagnostic
  dev clean all --ci                 # Clean everything (no prompts)
"@
}

function Show-CommandHelp {
    param([string]$Cmd)
    $helps = @{
        "setup" = @"
$($Script:Colors.Bold)dev setup$($Script:Colors.Reset)
First-time environment setup. Runs dependency checks, installs required tools,
creates the database, applies migrations, and seeds initial data.
$($Script:Colors.Dim)Options: --ci$($Script:Colors.Reset)
"@
        "build" = @"
$($Script:Colors.Bold)dev build [back|front|all] [options]$($Script:Colors.Reset)
Build the application.
$($Script:Colors.Dim)Targets: back, front, all (default)$($Script:Colors.Reset)
$($Script:Colors.Dim)Options: --configuration <Debug|Release>, --watch, --ci$($Script:Colors.Reset)
"@
        "test" = @"
$($Script:Colors.Bold)dev test [back|front|all] [options]$($Script:Colors.Reset)
Run tests.
$($Script:Colors.Dim)Options: --filter <expression>, --coverage, --ci$($Script:Colors.Reset)
"@
        "seed" = @"
$($Script:Colors.Bold)dev seed [options]$($Script:Colors.Reset)
Seed database with comprehensive fake data.
$($Script:Colors.Dim)Options:$($Script:Colors.Reset)
  --workers <n>       Worker profiles (default: 50)
  --customers <n>     Customer accounts (default: 200)
  --bookings <n>      Bookings (default: 500)
  --reviews <n>       Reviews (default: 300)
  --payments <n>      Payments (default: 400)
  --invoices <n>      Invoices (default: 400)
  --conversations <n> Conversations (default: 200)
  --messages <n>      Messages (default: 10000)
  --attachments <n>   Attachments (default: 500)
  --notifications <n> Notifications (default: 1000)
  --seed <n>          Random seed for deterministic data (default: random)
  --ci                Non-interactive mode
"@
    }
    if ($helps.ContainsKey($Cmd)) { Write-Host $helps[$Cmd] }
    else { Write-Host "No help available for '$Cmd'. Try 'dev --help'." }
}

function Invoke-Setup {
    param([switch]$Ci)
    Write-Section "First-Time Setup"
    Start-Timer
    Write-Info "This will set up your development environment"

    # Check dependencies
    Write-Info "Step 1: Checking dependencies..."
    $depsOk = Check-AllDependencies -Ci:$Ci
    if (-not $depsOk -and -not $Ci) {
        $continue = Read-Host "Some dependencies are missing. Continue anyway? (Y/n)"
        if ($continue -eq "n") { Write-Info "Setup aborted"; return }
    }

    # Restore packages
    Write-Info "Step 2: Restoring packages..."
    Push-Location $Script:BACK_DIR
    & dotnet restore 2>&1 | Out-Null
    Pop-Location
    Push-Location $Script:FRONT_DIR
    & npm install 2>&1 | Out-Null
    Pop-Location
    Write-Success "Packages restored"

    # Install dotnet-ef if missing
    $tools = dotnet tool list --global 2>$null
    if (-not ($tools | Select-String "dotnet-ef")) {
        Write-Info "Installing dotnet-ef..."
        & dotnet tool install --global dotnet-ef 2>&1 | Out-Null
    }

    # Create database and migrate
    Write-Info "Step 3: Creating database..."
    & dotnet ef database update --project "$Script:INFRASTRUCTURE_DIR" --startup-project "$Script:API_DIR" 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Warn "Database creation failed — you may need to configure the connection string" }

    # Verify HTTPS cert
    & dotnet dev-certs https --check 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Info "Trusting HTTPS certificate..."
        & dotnet dev-certs https --trust 2>$null
    }

    # Build solution
    Write-Info "Step 4: Building solution..."
    Build-Backend -Configuration "Debug" -Ci:$Ci | Out-Null

    Stop-Timer
    Write-Success "Setup complete! Run 'dev run' to start the development servers."
}

function ParseBuildArgs {
    param([string[]]$Args)
    $result = @{}
    for ($i = 0; $i -lt $Args.Count; $i++) {
        if ($Args[$i] -eq "--configuration" -and $i+1 -lt $Args.Count) { $result["Configuration"] = $Args[++$i] }
        if ($Args[$i] -eq "--watch") { $result["Watch"] = $true }
        if ($Args[$i] -eq "--release") { $result["Configuration"] = "Release" }
        if ($Args[$i] -eq "--debug") { $result["Configuration"] = "Debug" }
    }
    return $result
}

function ParseTestArgs {
    param([string[]]$Args)
    $result = @{}
    for ($i = 0; $i -lt $Args.Count; $i++) {
        if ($Args[$i] -eq "--filter" -and $i+1 -lt $Args.Count) { $result["Filter"] = $Args[++$i] }
        if ($Args[$i] -eq "--coverage") { $result["Coverage"] = $true }
    }
    return $result
}

function ParseRunArgs {
    param([string[]]$Args)
    $result = @{}
    for ($i = 0; $i -lt $Args.Count; $i++) {
        if ($Args[$i] -eq "--no-browser") { $result["NoBrowser"] = $true }
    }
    return $result
}

function ParseLintArgs {
    param([string[]]$Args)
    $result = @{}
    for ($i = 0; $i -lt $Args.Count; $i++) {
        if ($Args[$i] -eq "--fix") { $result["Fix"] = $true }
    }
    return $result
}
