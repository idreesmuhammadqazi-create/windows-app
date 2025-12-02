#!/bin/bash
# PseudoRun Desktop - Build Executable Script (Linux/Mac)
# This script builds a self-contained single-file executable for Windows

echo "================================================"
echo " PseudoRun Desktop - Executable Builder"
echo "================================================"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is not installed or not in PATH"
    echo ""
    echo "Please download and install .NET 8 SDK from:"
    echo "https://dotnet.microsoft.com/download/dotnet/8.0"
    echo ""
    exit 1
fi

echo "[1/4] Checking .NET SDK version..."
dotnet --version
echo ""

echo "[2/4] Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore packages"
    exit 1
fi
echo ""

echo "[3/4] Building self-contained executable..."
echo "This may take 1-2 minutes..."
echo ""

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Build failed!"
    echo "Check error messages above for details."
    exit 1
fi

echo ""
echo "[4/4] Build completed successfully!"
echo ""
echo "================================================"
echo " Build Results"
echo "================================================"
echo ""
echo "Executable location: publish/PseudoRun.exe"
echo ""

# Get file size
if [ -f "publish/PseudoRun.exe" ]; then
    size=$(ls -lh publish/PseudoRun.exe | awk '{print $5}')
    echo "File size: $size"
else
    echo "Warning: Could not find output executable"
fi

echo ""
echo "The executable is ready to distribute."
echo "It includes .NET runtime and works on any Windows machine."
echo ""
echo "================================================"
echo ""
echo "Done!"
