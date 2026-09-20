@echo off
echo Generando cobertura de pruebas...

REM Limpiar resultados anteriores
if exist TestResults rmdir /s /q TestResults

REM Ejecutar pruebas y generar archivo de cobertura
dotnet test ELearning.Tests/ELearning.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults

REM Generar reporte HTML
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

echo.
echo Reporte de cobertura generado en: TestResults\CoverageReport\index.html
echo Abriendo reporte en navegador...
start TestResults\CoverageReport\index.html
