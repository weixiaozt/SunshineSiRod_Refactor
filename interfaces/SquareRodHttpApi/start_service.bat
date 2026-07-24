@echo off
chcp 65001 >nul
cd /d "%~dp0"
python run_service.py --config api_config.local.json
