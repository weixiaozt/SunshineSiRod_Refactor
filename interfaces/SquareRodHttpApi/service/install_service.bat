@echo off
setlocal
cd /d "%~dp0"

net session >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Please run this script as Administrator.
  exit /b 5
)

if not exist "D:\Image1" mkdir "D:\Image1"
if not exist "D:\SquareRodApiData\logs" mkdir "D:\SquareRodApiData\logs"
if not exist "%~dp0service_logs" mkdir "%~dp0service_logs"

"%~dp0SquareRodServiceHost.exe" install
if errorlevel 1 exit /b %errorlevel%

"%~dp0SquareRodServiceHost.exe" start
if errorlevel 1 exit /b %errorlevel%

echo [OK] SquareRodAlgorithmService installed and started.
echo Check readiness with service_status.bat.
exit /b 0
