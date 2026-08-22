@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [StockFlow] .NET 10 SDK nao foi encontrado.
  echo Instale o SDK indicado no README e tente novamente.
  pause
  exit /b 1
)

where npm >nul 2>&1
if errorlevel 1 (
  echo [StockFlow] Node.js e npm nao foram encontrados.
  echo Instale o Node.js indicado no README e tente novamente.
  pause
  exit /b 1
)

echo [StockFlow] Preparando interface e API...
echo [StockFlow] O navegador abrira automaticamente em http://localhost:5081
echo [StockFlow] Mantenha esta janela aberta enquanto estiver testando.
echo.

start "" /b powershell.exe -NoProfile -WindowStyle Hidden -Command "$stockflowDeadline = (Get-Date).AddMinutes(5); while ((Get-Date) -lt $stockflowDeadline) { try { $stockflowResponse = Invoke-WebRequest -Uri 'http://localhost:5081/health' -UseBasicParsing -TimeoutSec 2; if ($stockflowResponse.StatusCode -eq 200) { Start-Process 'http://localhost:5081'; break } } catch { } Start-Sleep -Seconds 1 }"

dotnet run --project src\StockFlow.Api
set "stockflowExitCode=%errorlevel%"

if not "%stockflowExitCode%"=="0" (
  echo.
  echo [StockFlow] Nao foi possivel iniciar a aplicacao.
  echo Consulte as mensagens acima ou siga o tutorial do README.
  pause
)

exit /b %stockflowExitCode%
