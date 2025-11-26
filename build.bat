@echo off
echo Building Rolus Launcher...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:EnableCompressionInSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo Executable location: bin\Release\net8.0-windows\win-x64\publish\RolusLauncher.exe
    echo.
    echo To sign the executable, run:
    echo signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com bin\Release\net8.0-windows\win-x64\publish\RolusLauncher.exe
) else (
    echo.
    echo Build failed!
    exit /b %ERRORLEVEL%
)

pause

