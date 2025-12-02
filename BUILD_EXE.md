# Building PseudoRun Desktop Executable

This guide shows you how to create a standalone executable (.exe) for the PseudoRun Windows application.

## Prerequisites

1. **Install .NET 8 SDK** (if not already installed)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose the SDK (not just Runtime)
   - Verify installation: Open Command Prompt and run `dotnet --version`

## Build Methods

### Method 1: Self-Contained Single-File Executable (RECOMMENDED)

This creates one .exe file that works on any Windows machine (no .NET installation required on target).

**Command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

**Result:**
- Location: `./publish/PseudoRun.exe`
- Size: ~80-100 MB
- Portable: Can copy to any Windows machine and run

---

### Method 2: Framework-Dependent (Smaller Size)

Creates a smaller .exe that requires .NET 8 Runtime on target machine.

**Command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish-small
```

**Result:**
- Location: `./publish-small/PseudoRun.exe`
- Size: ~5-10 MB
- Requirement: Target machine needs [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

### Method 3: Self-Contained with DLLs

Creates .exe with supporting files (easier to debug if issues occur).

**Command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish-full
```

**Result:**
- Location: `./publish-full/PseudoRun.exe` + DLLs
- Size: ~80-100 MB (across multiple files)
- Distribution: Share entire folder

---

## Step-by-Step Instructions

1. **Open Command Prompt or PowerShell**

2. **Navigate to project directory:**
   ```bash
   cd path\to\windows-app
   ```

3. **Run the build command** (choose Method 1, 2, or 3 above)

4. **Find your executable:**
   - Method 1: `publish\PseudoRun.exe`
   - Method 2: `publish-small\PseudoRun.exe`
   - Method 3: `publish-full\PseudoRun.exe`

5. **Test the executable:**
   - Double-click PseudoRun.exe
   - Or run from command line: `.\publish\PseudoRun.exe`

---

## Platform-Specific Builds

For different Windows architectures:

| Target Platform | Runtime Identifier | Command Flag |
|----------------|-------------------|--------------|
| Windows 64-bit (most common) | win-x64 | `-r win-x64` |
| Windows 32-bit | win-x86 | `-r win-x86` |
| Windows ARM64 | win-arm64 | `-r win-arm64` |

**Example for 32-bit Windows:**
```bash
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o ./publish-x86
```

---

## Optimization Options

### Trim Unused Code (Smaller Size)
Add these flags for ~20-30% size reduction:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -o ./publish-trimmed
```

**Warning:** Test thoroughly after trimming - may remove reflection-based code.

### Enable ReadyToRun (Faster Startup)
Add this flag for faster application startup:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish-r2r
```

### Combine Both (Optimized Build)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish-optimized
```

---

## Troubleshooting

### Error: "dotnet command not found"
- Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
- Restart Command Prompt after installation

### Error: "Unable to find package AvalonEdit"
- Run `dotnet restore` first
- Check internet connection (NuGet packages need to download)

### Error: Build warnings about nullable references
- These are warnings, not errors
- Executable still builds successfully
- Can be ignored or fixed by updating code

### Data files not included in output
- The .csproj file already includes this config:
  ```xml
  <Content Include="Data\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  ```
- Data folder automatically copied to output

---

## Distribution

### For Single Users
- Share the .exe file from Method 1 (self-contained single file)
- No installation required - just run it

### For Multiple Users
- Create a ZIP file containing:
  - PseudoRun.exe
  - Data folder (examples, practice problems, etc.)
  - Optional: README.txt with usage instructions

### For Professional Distribution
- Consider creating an installer with tools like:
  - **Inno Setup** (free): https://jrsoftware.org/isinfo.php
  - **WiX Toolset** (free): https://wixtoolset.org/
  - **Advanced Installer** (paid): https://www.advancedinstaller.com/

---

## File Association Setup

To make .pseudo files open with PseudoRun automatically:

1. Right-click any .pseudo file
2. Choose "Open with" → "Choose another app"
3. Click "More apps" → "Look for another app on this PC"
4. Navigate to PseudoRun.exe
5. Check "Always use this app to open .pseudo files"

---

## Quick Reference

**Recommended command for most users:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

**Output:** `publish\PseudoRun.exe` (ready to distribute)
