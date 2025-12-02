# Windows File Association Guide for PseudoRun Desktop

This guide explains how to associate `.pseudo` files with PseudoRun Desktop on Windows, so you can double-click `.pseudo` files to open them directly.

## Method 1: Windows Settings (Recommended)

### Windows 11
1. Right-click any `.pseudo` file
2. Select **Open with → Choose another app**
3. Click **More apps**
4. Scroll down and click **Look for another app on this PC**
5. Navigate to where you installed PseudoRun Desktop
6. Select `PseudoRun.Desktop.exe`
7. Check **Always use this app to open .pseudo files**
8. Click **OK**

### Windows 10
1. Right-click any `.pseudo` file
2. Select **Open with → Choose another app**
3. Click **More apps**
4. Scroll down and click **Look for another app on this PC**
5. Navigate to where you installed PseudoRun Desktop
6. Select `PseudoRun.Desktop.exe`
7. Check **Always use this app to open .pseudo files**
8. Click **OK**

## Method 2: Registry Editor (Advanced)

**⚠️ Warning**: Only use this method if you're comfortable editing the Windows Registry. Incorrect changes can cause system issues.

### Step 1: Create `.pseudo` File Type Entry

1. Press `Win + R`, type `regedit`, and press Enter
2. Navigate to `HKEY_CLASSES_ROOT`
3. Right-click → New → Key
4. Name it `.pseudo`
5. Right-click the `.pseudo` key → New → String Value
6. Name it `(Default)`
7. Double-click it and set value to `PseudoRunFile`

### Step 2: Create File Type Association

1. In `HKEY_CLASSES_ROOT`, create a new key named `PseudoRunFile`
2. Set its `(Default)` value to `IGCSE Pseudocode File`
3. Create a subkey `DefaultIcon`
4. Set its `(Default)` value to: `"C:\Path\To\PseudoRun.Desktop.exe",0`
5. Create a subkey `shell\open\command`
6. Set its `(Default)` value to: `"C:\Path\To\PseudoRun.Desktop.exe" "%1"`

**Note**: Replace `C:\Path\To\` with your actual installation path.

### Example Registry Structure
```
HKEY_CLASSES_ROOT
├── .pseudo
│   └── (Default) = "PseudoRunFile"
└── PseudoRunFile
    ├── (Default) = "IGCSE Pseudocode File"
    ├── DefaultIcon
    │   └── (Default) = "C:\Program Files\PseudoRun\PseudoRun.Desktop.exe",0
    └── shell
        └── open
            └── command
                └── (Default) = "C:\Program Files\PseudoRun\PseudoRun.Desktop.exe" "%1"
```

## Method 3: Registry File (Quick Setup)

### Create a Registration Script

1. Create a new text file named `register-pseudo-files.reg`
2. Copy the following content (update paths as needed):

```reg
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\.pseudo]
@="PseudoRunFile"

[HKEY_CLASSES_ROOT\PseudoRunFile]
@="IGCSE Pseudocode File"

[HKEY_CLASSES_ROOT\PseudoRunFile\DefaultIcon]
@="\"C:\\Program Files\\PseudoRun\\PseudoRun.Desktop.exe\",0"

[HKEY_CLASSES_ROOT\PseudoRunFile\shell]

[HKEY_CLASSES_ROOT\PseudoRunFile\shell\open]

[HKEY_CLASSES_ROOT\PseudoRunFile\shell\open\command]
@="\"C:\\Program Files\\PseudoRun\\PseudoRun.Desktop.exe\" \"%1\""
```

3. **Important**: Update the paths to match your installation location
4. Save the file
5. Right-click `register-pseudo-files.reg`
6. Select **Merge**
7. Click **Yes** to confirm
8. Click **OK** when complete

### Unregister (Remove Association)

To remove the file association:

1. Create a file named `unregister-pseudo-files.reg`
2. Copy this content:

```reg
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\.pseudo]
[-HKEY_CLASSES_ROOT\PseudoRunFile]
```

3. Save and merge it the same way

## Method 4: PowerShell Script (For IT Administrators)

For deploying to multiple computers:

```powershell
# Register .pseudo file association
$progPath = "C:\Program Files\PseudoRun\PseudoRun.Desktop.exe"

# Create file type
New-Item -Path "Registry::HKEY_CLASSES_ROOT\.pseudo" -Force
Set-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\.pseudo" -Name "(Default)" -Value "PseudoRunFile"

# Create association
New-Item -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile" -Force
Set-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile" -Name "(Default)" -Value "IGCSE Pseudocode File"

# Create icon
New-Item -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile\DefaultIcon" -Force
Set-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile\DefaultIcon" -Name "(Default)" -Value "`"$progPath`",0"

# Create open command
New-Item -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile\shell\open\command" -Force
Set-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\PseudoRunFile\shell\open\command" -Name "(Default)" -Value "`"$progPath`" `"%1`""

Write-Host "File association registered successfully!"
```

Run PowerShell as Administrator and execute this script.

## Verification

### Test the Association

1. Create a test file: `test.pseudo`
2. Add some code:
   ```pseudocode
   DECLARE x : INTEGER
   x <- 5
   OUTPUT "Value: ", x
   ```
3. Double-click the file
4. PseudoRun Desktop should open with the file loaded

### Check Properties

1. Right-click a `.pseudo` file
2. Select **Properties**
3. Under **Opens with**, you should see **PseudoRun Desktop**
4. If not, click **Change** and select it

## Troubleshooting

### File Won't Open
- Verify the path to `PseudoRun.Desktop.exe` is correct
- Ensure you have permissions to the installation directory
- Try Method 1 (Windows Settings) instead

### Wrong Program Opens
- Check file association in Properties
- Use **Open with** and select PseudoRun Desktop
- Re-apply the registration using Method 1

### Registry Changes Don't Work
- Ensure you ran Registry Editor or PowerShell as Administrator
- Restart Windows Explorer: `taskkill /f /im explorer.exe && start explorer.exe`
- Log out and log back in

### Permission Denied
- Run Registry Editor as Administrator (right-click → Run as administrator)
- Check that you have write permissions to HKEY_CLASSES_ROOT
- Contact your IT administrator if on a managed computer

## For IT Departments

### Mass Deployment

Use Group Policy or SCCM to deploy the registry script:

1. Create a Group Policy Object
2. Go to **Computer Configuration → Preferences → Windows Settings → Registry**
3. Create new registry items matching the structure above
4. Link the GPO to appropriate OUs
5. Run `gpupdate /force` on client machines

### Network Installation

For network drives:

```reg
[HKEY_CLASSES_ROOT\PseudoRunFile\shell\open\command]
@="\"\\\\server\\share\\PseudoRun\\PseudoRun.Desktop.exe\" \"%1\""
```

### User-Level Association (Non-Admin)

If users don't have admin rights, they can only use Method 1 (Windows Settings), which registers under:
`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pseudo`

## Security Considerations

- Always download PseudoRun from trusted sources
- Verify the executable before creating associations
- Be cautious with registry modifications
- Keep Windows Defender enabled
- Regular security updates

## Additional Features

### Context Menu Integration (Optional)

To add "Open with PseudoRun" to right-click menu:

```reg
[HKEY_CLASSES_ROOT\*\shell\PseudoRun]
@="Open with PseudoRun"
"Icon"="\"C:\\Program Files\\PseudoRun\\PseudoRun.Desktop.exe\",0"

[HKEY_CLASSES_ROOT\*\shell\PseudoRun\command]
@="\"C:\\Program Files\\PseudoRun\\PseudoRun.Desktop.exe\" \"%1\""
```

This adds the option to all file types (use cautiously).

## Uninstallation

### Remove All Associations

1. Use the unregister script from Method 3
2. Or delete these registry keys:
   - `HKEY_CLASSES_ROOT\.pseudo`
   - `HKEY_CLASSES_ROOT\PseudoRunFile`
3. Restart Windows Explorer

### Reset to Default

1. Right-click a `.pseudo` file
2. Properties → Change
3. Select Notepad or another text editor
4. Click OK

## Support

If you encounter issues:
1. Try Method 1 (simplest and safest)
2. Ensure paths are correct
3. Run as Administrator
4. Restart Windows Explorer or your computer
5. Check Windows Event Viewer for errors

---

**Note**: File associations are specific to each user profile on Windows. If multiple users use the same computer, each needs to set up their own associations (unless using Method 4 with Computer Configuration).
