@echo off
REM PseudoRun Desktop - Build Executable Script
REM This script builds a self-contained single-file executable for Windows

echo ================================================
echo  PseudoRun Desktop - Executable Builder
echo ================================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo.
    echo Please download and install .NET 8 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [1/4] Checking .NET SDK version...
dotnet --version
echo.

echo [2/4] Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

echo [3/4] Building self-contained executable...
echo This may take 1-2 minutes...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    echo Check error messages above for details.
    pause
    exit /b 1
)

echo.
echo [4/4] Build completed successfully!
echo.
echo ================================================
echo  Build Results
echo ================================================
echo.
echo Executable location: publish\PseudoRun.exe
echo.

REM Get file size
for %%A in (publish\PseudoRun.exe) do (
    set size=%%~zA
)
echo File size: %size% bytes
echo.

echo The executable is ready to distribute.
echo It includes .NET runtime and works on any Windows machine.
echo.
echo ================================================
echo.

REM Ask if user wants to open the folder
set /p OPEN="Open output folder? (y/n): "
if /i "%OPEN%"=="y" (
    start explorer publish
)

echo.
echo Done! Press any key to exit...
pause >nul
