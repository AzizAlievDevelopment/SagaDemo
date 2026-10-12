$ErrorActionPreference = "Stop"

# Корень проекта SagaDemo
$root = Split-Path -Parent $PSScriptRoot

# Запускаем RabbitMQ
docker compose --project-directory $root up -d

$services = @(
    "OrderService",
    "PaymentService",
    "InventoryService"
)

foreach ($service in $services) {

    $projectPath = Join-Path $root "src\$service"

    Write-Host "Starting $service..." -ForegroundColor Cyan

    $command = "Set-Location '$projectPath'; dotnet run --launch-profile http"

    Start-Process powershell.exe -ArgumentList @(
        "-NoExit",
        "-Command",
        $command
    )
}

Write-Host "Service launch commands executed." -ForegroundColor Green

