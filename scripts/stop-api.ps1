$targets = @()

Get-Process -ErrorAction SilentlyContinue |
    Where-Object { $_.ProcessName -like "RentCollection.API" } |
    ForEach-Object { $targets += $_ }

Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq "dotnet.exe" -and $_.CommandLine -match "RentCollection.API" } |
    ForEach-Object {
        try {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction Stop
            Write-Host "Stopped process dotnet.exe (PID $($_.ProcessId))"
        } catch {
            Write-Host "Failed to stop process dotnet.exe (PID $($_.ProcessId))"
        }
    }

foreach ($process in $targets) {
    try {
        Stop-Process -Id $process.Id -Force -ErrorAction Stop
        Write-Host "Stopped process $($process.ProcessName) (PID $($process.Id))"
    } catch {
        Write-Host "Failed to stop process $($process.ProcessName) (PID $($process.Id))"
    }
}
