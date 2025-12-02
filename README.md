# PseudoRun Desktop - IGCSE Pseudocode Editor & Simulator

A comprehensive Windows desktop application for learning and practicing IGCSE Computer Science pseudocode.

## 🎯 Features

### Core Functionality
- **Complete IGCSE Pseudocode Interpreter** - Full support for Cambridge IGCSE pseudocode specification
- **Professional Code Editor** - AvalonEdit integration with syntax highlighting for IGCSE keywords
- **Native Windows Integration** - File dialogs, drag-and-drop support, recent files menu
- **Real-time Validation** - Syntax checking before execution with detailed error messages
- **Smart Error Display** - Professional error panel showing line/column information

### Educational Features
- **27 Comprehensive Examples** - From basic input/output to advanced algorithms
  - Categorized by difficulty (Beginner, Intermediate, Advanced)
  - Searchable and filterable
  - One-click load to editor

- **50 Practice Problems** - Complete IGCSE curriculum coverage
  - Variables, Loops, Arrays, Strings, Selection
  - Procedures, Functions, Algorithms
  - Validation, 2D Arrays, BYREF parameters
  - Hints and complete solutions included

- **Interactive Tutorial System** - Step-by-step learning
  - Progressive difficulty levels
  - Code examples for each concept
  - "Try This Code" functionality

- **Complete Syntax Reference** - Searchable IGCSE syntax guide
  - All language constructs documented
  - Examples for each syntax element
  - Category navigation

- **Exam Mode** - Timed practice sessions
  - Configurable duration (15, 30, 45, 60, 90 minutes)
  - Countdown timer with color coding
  - Pause/resume functionality
  - Completion notifications

### Advanced Tools
- **Code Explainer** - Analyzes code and generates plain English explanations
  - Complexity scoring
  - Construct identification
  - Improvement suggestions

- **Trace Table Generator** - Creates step-by-step variable trace tables
  - Export to CSV, Markdown, or formatted text
  - Essential for IGCSE exam preparation

- **Common Mistakes Detector** - Identifies typical student errors
  - Assignment operator mistakes (= vs <-)
  - Declaration syntax errors
  - Array syntax issues
  - Missing END statements
  - And 20+ more checks

### Debug Mode
- **Visual Debugging** - Step through code execution
  - Debug controls in toolbar (Continue, Step Over, Stop)
  - Variables panel showing current values
  - Current line highlighting
  - Keyboard shortcuts (F5, F10, Shift+F5)

### File Operations
- **Native Dialogs** - Windows Open/Save file dialogs
- **Recent Files** - Quick access to last 10 opened files
- **Export** - Save code to DOCX or PDF with formatting
- **Sandboxed I/O** - Safe file operations for FILE commands

## 🚀 Getting Started

### System Requirements
- Windows 10/11 (64-bit)
- .NET 8 Runtime
- 100 MB disk space

### Installation
1. Download the latest release
2. Extract to your desired location
3. Run `PseudoRun.Desktop.exe`

### First Steps
1. **Load an Example**: Help → Examples → Browse and load sample code
2. **Run Your First Program**: Click Run (F5) or use the toolbar
3. **Try Practice Problems**: Tools → Practice Problems
4. **Follow the Tutorial**: Tools → Tutorial for step-by-step guidance

## 📖 Usage Guide

### Writing Code
The editor supports all IGCSE pseudocode constructs:
- **Variables**: `DECLARE name : STRING`
- **Input/Output**: `INPUT name`, `OUTPUT "Hello ", name`
- **Selection**: `IF ... THEN ... ELSE ... ENDIF`
- **Iteration**: `FOR`, `WHILE`, `REPEAT ... UNTIL`
- **Arrays**: `DECLARE scores : ARRAY[1:10] OF INTEGER`
- **Procedures**: `PROCEDURE MyProc()`
- **Functions**: `FUNCTION Calculate() RETURNS INTEGER`
- **File I/O**: `OPENFILE`, `READFILE`, `WRITEFILE`, `CLOSEFILE`

### Running Code
1. Write your pseudocode in the editor
2. Click **Run** (F5) or use the toolbar button
3. View output in the right panel
4. If errors occur, see the ErrorPanel below output with line/column details

### Using Examples
1. Click **Help → Examples**
2. Filter by category or search
3. Read description and exam relevance
4. Click **Load to Editor** to try it

### Practicing Problems
1. Click **Tools → Practice Problems**
2. Filter by level (IGCSE) and topic
3. Read description and hints
4. Try solving it yourself
5. Click **Show Solution** when ready

### Exam Mode
1. Click **Tools → Exam Mode**
2. Choose duration (default: 45 minutes)
3. Click **Start Exam**
4. Write your solution
5. Timer shows remaining time with color coding
6. Click **End Exam** when complete

### Export Options
1. **Export to PDF**: File → Export → Export to PDF
2. **Export to Word**: File → Export → Export to Word
3. Code is formatted with Consolas font and includes timestamp

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New File |
| `Ctrl+O` | Open File |
| `Ctrl+S` | Save File |
| `Ctrl+Shift+S` | Save As |
| `F5` | Run Program |
| `Shift+F5` | Stop Program |
| `F10` | Step Over (Debug Mode) |

## 🛠️ Technical Details

### Architecture
- **Framework**: WPF (.NET 8)
- **Pattern**: MVVM with CommunityToolkit.Mvvm
- **DI**: Microsoft.Extensions.DependencyInjection
- **Editor**: AvalonEdit with custom IGCSE syntax highlighting
- **Export**: DocumentFormat.OpenXml (DOCX), PdfSharp (PDF)

### File Locations
- **Settings**: `%APPDATA%\PseudoRun\settings.json`
- **File I/O Sandbox**: `%USERPROFILE%\Documents\PseudoRun\FileIO\`
- **Examples**: `Data\Examples.json` (27 examples)
- **Practice Problems**: `Data\PracticeProblems.json` (50 problems)
- **Tutorials**: `Data\TutorialSteps.json`
- **Syntax Reference**: `Data\SyntaxReference.json`

### Supported File Formats
- **`.pseudo`** - Primary pseudocode file format
- **`.txt`** - Plain text files
- **`.pdf`** - Export format
- **`.docx`** - Export format

## 📚 Educational Content

### Coverage
- ✅ All IGCSE pseudocode syntax
- ✅ Cambridge specification compliant
- ✅ Paper 2 exam preparation
- ✅ Algorithm practice
- ✅ Real exam-style problems

### Learning Path
1. **Beginner** (10 examples + 15 problems)
   - Variables and data types
   - Input/output
   - Basic arithmetic

2. **Intermediate** (12 examples + 20 problems)
   - Selection (IF/CASE)
   - Iteration (FOR/WHILE/REPEAT)
   - Arrays

3. **Advanced** (5 examples + 15 problems)
   - Procedures and functions
   - File operations
   - Algorithms (sorting, searching)
   - 2D arrays

## 🐛 Troubleshooting

### Program Won't Run
- Check the ErrorPanel below output for syntax errors
- Ensure all variables are declared
- Verify ENDIF, ENDWHILE, NEXT statements match

### INPUT Not Working
- InputDialog should appear automatically
- If blocked, check Windows notification settings
- Ensure program isn't frozen

### Export Failed
- Check write permissions to target directory
- Ensure filename is valid
- Try a different location

### File Won't Open
- Check file exists and isn't corrupted
- Try opening in Notepad first to verify format
- Ensure file has read permissions

## 🔐 Security & Privacy

- **No Internet Connection Required** - Fully offline application
- **No Data Collection** - Your code stays on your computer
- **Sandboxed File I/O** - FILE operations limited to safe directory
- **Local Settings Only** - All preferences stored locally

## 📊 System Performance

- **Startup Time**: < 2 seconds
- **Execution Speed**: Comparable to web version
- **Memory Usage**: ~100 MB typical, ~200 MB max
- **File Size Support**: Up to 10,000 lines of code

## 🎓 For Teachers

### Classroom Use
- Install on school computers
- Use examples for demonstrations
- Assign practice problems as homework
- Use Exam Mode for timed assessments

### Assessment Features
- Trace Table Generator for marking
- Code Explainer for understanding student work
- Common Mistakes Detector for feedback
- Export to PDF for record keeping

## 🤝 Contributing

This is an educational project. Contributions welcome:
- Report bugs and issues
- Suggest new examples or problems
- Improve documentation
- Submit enhancements

## 📄 License

Educational use. Cambridge IGCSE syllabus compliance.

## 🏆 Acknowledgments

- Based on the PseudoRun web application
- IGCSE pseudocode specification from Cambridge Assessment
- AvalonEdit for the excellent text editor component
- Community feedback and testing

## 📮 Support

For questions, issues, or feedback:
- Check the Help → Syntax Reference
- Try the Help → Examples
- Follow the Tutorial for learning
- Review error messages in ErrorPanel

## 🔄 Version History

### Version 1.0.0 (Current)
- ✅ Complete IGCSE interpreter
- ✅ 27 examples, 50 practice problems
- ✅ Tutorial system
- ✅ Syntax reference
- ✅ Exam mode
- ✅ Error display with validation
- ✅ Debug mode UI
- ✅ Export to DOCX/PDF
- ✅ Code analysis utilities
- ✅ Recent files menu

## 🎯 Roadmap

Future enhancements (optional):
- [ ] Autocomplete with IntelliSense
- [ ] Find/Replace functionality
- [ ] Code folding
- [ ] Dark theme option
- [ ] More practice problems (100+)
- [ ] Additional language modes
- [ ] Plugin system

---

**PseudoRun Desktop** - Making IGCSE Pseudocode Learning Easy and Effective

*Built with ❤️ for Computer Science students*
