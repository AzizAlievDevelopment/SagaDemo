$ErrorActionPreference = "Stop"

# Корень проекта SagaDemo
$root = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $root "logs"
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

# Запускаем RabbitMQ
docker compose --project-directory $root up -d

$services = @(
    "SagaOrchestrator",
    "OrderService",
    "PaymentService",
    "InventoryService"
)

foreach ($service in $services) {

    $projectPath = Join-Path $root "src\$service"

    Write-Host "Starting $service..." -ForegroundColor Cyan

    $escapedProjectPath = $projectPath.Replace("'", "''")
    $logPath = (Join-Path $logDirectory "$service.log").Replace("'", "''")
    $command = "Set-Location '$escapedProjectPath'; dotnet run --launch-profile http *> '$logPath'"

    Start-Process powershell.exe -WindowStyle Hidden -ArgumentList @(
        "-NoProfile",
        "-Command",
        $command
    )
}

Write-Host "Service launch commands executed." -ForegroundColor Green

