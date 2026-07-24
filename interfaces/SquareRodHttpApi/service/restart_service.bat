@echo off
setlocal
cd /d "%~dp0"

net session >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Please run this script as Administrator.
  exit /b 5
)

"%~dp0SquareRodServiceHost.exe" restart
exit /b %errorlevel%
