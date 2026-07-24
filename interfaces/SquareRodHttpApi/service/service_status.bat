@echo off
setlocal
cd /d "%~dp0"

"%~dp0SquareRodServiceHost.exe" status
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "try { Invoke-RestMethod 'http://127.0.0.1:8780/api/v1/ready' -TimeoutSec 5 | ConvertTo-Json -Depth 5 } catch { Write-Host ('HTTP readiness failed: ' + $_.Exception.Message); exit 2 }"
exit /b %errorlevel%
