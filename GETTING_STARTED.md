# Getting Started with PseudoRun Desktop

## Prerequisites

**Required:**
- **Windows 10 or 11** (64-bit)
- **.NET 8 SDK** (for development) or **.NET 8 Runtime** (for running only)

**Recommended:**
- Visual Studio 2022 (Community Edition is free)
- OR Visual Studio Code with C# extension

---

## Method 1: Using Visual Studio 2022 (Easiest)

### Step 1: Install Visual Studio 2022
1. Download from: https://visualstudio.microsoft.com/downloads/
2. Choose **Community Edition** (free)
3. During installation, select:
   - ✅ **.NET desktop development** workload
   - ✅ .NET 8.0 Runtime

### Step 2: Open the Project
1. Launch Visual Studio 2022
2. Click **Open a project or solution**
3. Navigate to: `windows-app/PseudoRun.Desktop.csproj`
4. Click **Open**

### Step 3: Restore Packages
Visual Studio will automatically restore NuGet packages. If not:
1. Right-click the solution in Solution Explorer
2. Select **Restore NuGet Packages**
3. Wait for completion (shown in status bar)

### Step 4: Build the Project
1. Press **Ctrl+Shift+B** or
2. Click **Build → Build Solution**
3. Wait for "Build succeeded" message

### Step 5: Run the Application
1. Press **F5** (with debugging) or **Ctrl+F5** (without debugging)
2. PseudoRun Desktop will launch!

---

## Method 2: Using Command Line (.NET SDK)

### Step 1: Install .NET 8 SDK
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run installer
3. Verify installation:
   ```bash
   dotnet --version
   ```
   Should show: `8.0.x`

### Step 2: Navigate to Project Directory
```bash
cd path\to\windows-app
```

### Step 3: Restore Dependencies
```bash
dotnet restore
```

### Step 4: Build the Application
```bash
dotnet build
```

### Step 5: Run the Application
```bash
dotnet run
```

Or run the compiled executable directly:
```bash
cd bin\Debug\net8.0-windows
PseudoRun.Desktop.exe
```

---

## Method 3: Build and Distribute (Create Standalone Executable)

### Create Self-Contained Build

For **x64 Windows** (includes .NET runtime):
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in:
```
bin\Release\net8.0-windows\win-x64\publish\PseudoRun.Desktop.exe
```

### Create Framework-Dependent Build

Smaller file size, but requires .NET 8 Runtime on target machine:
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

### Distribution Options

**Option A: Single Folder**
1. Copy the entire `publish` folder
2. Share with students/teachers
3. Run `PseudoRun.Desktop.exe`

**Option B: Installer (Advanced)**
Use tools like:
- WiX Toolset
- Inno Setup
- NSIS

---

## Troubleshooting

### "dotnet is not recognized"
**Solution**: Add .NET to PATH or reinstall .NET SDK
1. Go to System Properties → Environment Variables
2. Add to PATH: `C:\Program Files\dotnet\`
3. Restart command prompt

### "The target framework 'net8.0-windows' is not installed"
**Solution**: Install .NET 8 SDK
- Download: https://dotnet.microsoft.com/download/dotnet/8.0

### NuGet Package Restore Failed
**Solution**: Clear NuGet cache
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Build Errors about AvalonEdit or other packages
**Solution**: Manually restore packages
```bash
cd windows-app
dotnet restore --force
dotnet build
```

### Application Won't Start on Another Computer
**Possible causes:**
1. **.NET 8 Runtime not installed**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Install "Desktop Runtime"

2. **Missing dependencies**
   - Use `--self-contained true` when publishing

3. **Windows version too old**
   - Requires Windows 10 (1809) or newer

### "Could not load file or assembly" errors
**Solution**: Ensure all DLLs are in the same folder as the .exe

---

## First Run Checklist

After launching PseudoRun Desktop:

✅ **Verify Core Features:**
1. Type some code in the editor
2. Click **Run (F5)**
3. Check output appears
4. Try **Help → Examples**
5. Test **Tools → Practice Problems**

✅ **Test File Operations:**
1. Click **File → New** (Ctrl+N)
2. Click **File → Save As** (Ctrl+Shift+S)
3. Save a `.pseudo` file
4. Click **File → Open** (Ctrl+O)
5. Open the saved file

✅ **Verify Educational Features:**
1. **Help → Examples** - Should show 27 examples
2. **Tools → Practice Problems** - Should show 50 problems
3. **Tools → Tutorial** - Should show step-by-step guide
4. **Tools → Syntax Reference** - Should show complete syntax
5. **Tools → Exam Mode** - Should show timer configuration

✅ **Test Export:**
1. Write some code
2. **File → Export → Export to PDF**
3. Check PDF is created and formatted correctly

---

## Quick Test Program

Copy and paste this into the editor to test:

```pseudocode
DECLARE name : STRING
DECLARE age : INTEGER

OUTPUT "Welcome to PseudoRun Desktop!"
INPUT name
OUTPUT "Hello, ", name

age <- 16
OUTPUT "Your age is: ", age
```

Click **Run (F5)** - you should see:
1. Output panel shows "Welcome to PseudoRun Desktop!"
2. Input dialog appears asking for name
3. After entering name, see greeting
4. See age output

---

## Development Setup (For Contributors)

### Recommended Extensions for VS Code
- C# (Microsoft)
- C# Dev Kit
- .NET Extension Pack

### Project Structure
```
windows-app/
├── PseudoRun.Desktop.csproj    # Project file
├── App.xaml + App.xaml.cs       # Application entry
├── Interpreter/                 # Core engine (~4000 lines)
├── Services/                    # Business logic
├── ViewModels/                  # MVVM ViewModels
├── Views/                       # WPF UI
├── Models/                      # Data structures
├── Utilities/                   # Helper tools
├── Data/                        # JSON data files
└── Resources/                   # Styles and resources
```

### Running Tests (if available)
```bash
dotnet test
```

### Watch Mode (auto-rebuild on file changes)
```bash
dotnet watch run
```

---

## Performance Tips

### Startup Performance
- First run is slower (JIT compilation)
- Subsequent runs are faster
- Consider using ReadyToRun compilation:
  ```bash
  dotnet publish -c Release -p:PublishReadyToRun=true
  ```

### Memory Usage
- Typical: ~100 MB
- With large files: ~200 MB
- Monitor in Task Manager if needed

---

## Deployment Scenarios

### Scenario 1: School Computer Lab
**Setup:**
1. Build self-contained: `dotnet publish -c Release -r win-x64 --self-contained true`
2. Copy publish folder to network share: `\\server\apps\PseudoRun\`
3. Create desktop shortcuts pointing to .exe

**File Association:**
- Follow `FILE_ASSOCIATION_GUIDE.md`
- Use PowerShell script for all computers

### Scenario 2: Student Home Computers
**Setup:**
1. Create self-contained build (includes .NET runtime)
2. Package as ZIP file
3. Students extract and run .exe
4. No installation required

### Scenario 3: Teacher Demonstration
**Setup:**
1. Run from Visual Studio for easy debugging
2. Keep solution open for quick edits
3. Use F5 to run with debugger attached

---

## Advanced Configuration

### Custom Data Directory
By default, data files are in `Data/` folder. To use custom location:
1. Edit `App.xaml.cs`
2. Modify service registration to point to custom paths

### Settings Location
- User settings: `%APPDATA%\PseudoRun\settings.json`
- File I/O sandbox: `%USERPROFILE%\Documents\PseudoRun\FileIO\`

To change, modify `SettingsService.cs` and `FileIOService.cs`

---

## Support & Resources

### Documentation
- **README.md** - Complete feature guide
- **IMPLEMENTATION_STATUS.md** - Technical details
- **FILE_ASSOCIATION_GUIDE.md** - Windows integration

### Learning Resources
- Built-in Tutorial (Tools → Tutorial)
- 27 Examples (Help → Examples)
- 50 Practice Problems (Tools → Practice Problems)
- Syntax Reference (Tools → Syntax Reference)

### Troubleshooting
1. Check error messages in ErrorPanel (below output)
2. Review status bar for line/column info
3. Try running example programs first
4. Verify .NET 8 Runtime is installed

---

## Next Steps

After successfully running the application:

1. ✅ **Explore Examples**: Help → Examples
2. ✅ **Try Tutorial**: Tools → Tutorial
3. ✅ **Practice**: Tools → Practice Problems
4. ✅ **Learn Syntax**: Tools → Syntax Reference
5. ✅ **Test Features**: Try all menu items
6. ✅ **Create Programs**: Write your own code
7. ✅ **Use Exam Mode**: Tools → Exam Mode

---

## Common Commands Reference

### Build Commands
```bash
# Restore packages
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Run
dotnet run

# Publish self-contained
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Clean build artifacts
dotnet clean
```

### Visual Studio Shortcuts
- **F5** - Run with debugging
- **Ctrl+F5** - Run without debugging
- **Ctrl+Shift+B** - Build solution
- **Ctrl+Shift+F** - Find in all files
- **F12** - Go to definition

---

## Success! 🎉

If you see the PseudoRun Desktop window with:
- Code editor on the left
- Output panel on the right
- Menu bar at top (File, Run, Tools, Help)
- Toolbar with Run button
- Status bar at bottom

**You're all set!** Start writing IGCSE pseudocode and exploring the features.

---

**Need Help?**
- Check the built-in examples
- Review the README.md
- Test with the quick test program above
- Verify .NET 8 is installed correctly
