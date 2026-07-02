$Script:ROOT_DIR = Resolve-Path "$PSScriptRoot/.."
$Script:BACK_DIR = "$Script:ROOT_DIR/back"
$Script:FRONT_DIR = "$Script:ROOT_DIR/front"
$Script:API_DIR = "$Script:BACK_DIR/src/API"
$Script:INFRASTRUCTURE_DIR = "$Script:BACK_DIR/src/Infrastructure"
$Script:SEEDER_DIR = "$Script:BACK_DIR/tools/Seeder"
$Script:SOLUTION_FILE = "$Script:BACK_DIR/back.slnx"

$Script:DEFAULT_ENV = "development"
$Script:SUPPORTED_ENVS = @("development", "staging", "production")

$Script:BACKEND_PORT = 5000
$Script:BACKEND_HTTPS_PORT = 5001
$Script:FRONTEND_PORT = 4200
$Script:SWAGGER_URL = "http://localhost:5000/swagger"
$Script:FRONTEND_URL = "http://localhost:4200"

$Script:SEED_DEFAULTS = @{
    Workers = 50
    Customers = 200
    Bookings = 500
    Reviews = 300
    Payments = 400
    Invoices = 400
    Conversations = 200
    Messages = 10000
    Attachments = 500
    Notifications = 1000
}

function Get-ConfigValue {
    param([string]$Key)
    $appSettingsPath = "$Script:API_DIR/appsettings.json"
    if (Test-Path $appSettingsPath) {
        $config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $keys = $Key -split ':'
        $value = $config
        foreach ($k in $keys) { if ($value.$k) { $value = $value.$k } else { return $null } }
        return $value
    }
    return $null
}

function Get-ConnectionString {
    return Get-ConfigValue "ConnectionStrings:DefaultConnection"
}

function Set-Environment {
    param([string]$Env)
    $env:ASPNETCORE_ENVIRONMENT = $Env
    $env:DOTNET_ENVIRONMENT = $Env
    Write-Info "Environment set to: $Env"
}

function Get-Environment {
    if ($env:ASPNETCORE_ENVIRONMENT) { return $env:ASPNETCORE_ENVIRONMENT }
    if ($env:DOTNET_ENVIRONMENT) { return $env:DOTNET_ENVIRONMENT }
    return $Script:DEFAULT_ENV
}
