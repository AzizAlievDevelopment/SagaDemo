
$services = @(
    "OrderService",
    "PaymentService",
    "InventoryService"
)

foreach ($service in $services) {
    Get-Process -Name $service -ErrorAction SilentlyContinue |
        Stop-Process -Force
}

Write-Host "All Saga services stopped!"
