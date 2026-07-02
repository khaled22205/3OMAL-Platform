. "$PSScriptRoot/config.ps1"
. "$PSScriptRoot/logging.ps1"

function Invoke-SeedCmd {
    param(
        [hashtable]$Counts = @{},
        [int]$RandomSeed = -1,
        [switch]$Ci
    )

    Write-Section "Database Seeding"

    if (-not (Test-Path "$Script:SEEDER_DIR/Seeder.csproj")) {
        Write-Warn "Seeder project not found. Run 'dev setup' first."
        if ($Ci) { exit 1 }
        $confirm = Read-Host "Create Seeder project now? (Y/n)"
        if ($confirm -ne "n") {
            Initialize-SeederProject
        } else {
            Write-Error "Seeder project required"; exit 1
        }
    }

    Start-Timer
    $seederArgs = @("run", "--project", "`"$Script:SEEDER_DIR`"", "--")

    $merged = $Script:SEED_DEFAULTS.Clone()
    foreach ($key in $Counts.Keys) { $merged[$key] = $Counts[$key] }

    foreach ($entry in $merged.GetEnumerator()) {
        $seederArgs += "--$($entry.Key)", "$($entry.Value)"
    }
    if ($RandomSeed -ge 0) { $seederArgs += "--seed", "$RandomSeed" }
    if ($Ci) { $seederArgs += "--ci" }

    Write-Info "Seeding with: $($merged | ConvertTo-Json -Compress)"
    & dotnet $seederArgs 2>&1
    $exitCode = $LASTEXITCODE

    Stop-Timer
    if ($exitCode -eq 0) {
        Write-Success "Database seeded successfully"
    } else {
        Write-Error "Seeding failed (exit code: $exitCode)"
        exit 1
    }
}

function Parse-SeedArgs {
    param([string[]]$Args)
    $counts = @{}
    $seed = -1
    $i = 0
    while ($i -lt $Args.Count) {
        $arg = $Args[$i]
        if ($arg -match "^--(\w+)$") {
            $key = $matches[1]
            $i++
            if ($i -lt $Args.Count -and $Args[$i] -notmatch "^-") {
                $val = $Args[$i]
                if ($key -eq "seed") { $seed = [int]$val }
                else { $counts[$key] = [int]$val }
            }
        }
        $i++
    }
    return @{ Counts = $counts; Seed = $seed }
}

function Initialize-SeederProject {
    if (-not (Test-Path "$Script:SEEDER_DIR")) { New-Item -ItemType Directory -Path "$Script:SEEDER_DIR" -Force | Out-Null }
    Write-Info "Seeder project will be created during setup"
}
