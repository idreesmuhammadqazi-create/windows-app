using System;
using System.Collections.Generic;
using System.IO;

namespace PseudoRun.Desktop.Services
{
    public class FileIOService : IFileIOService
    {
        private readonly string _sandboxPath;
        private readonly Dictionary<string, (StreamReader? reader, StreamWriter? writer, string mode)> _openFiles;

        public FileIOService()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _sandboxPath = Path.Combine(documentsPath, "PseudoRun", "FileIO");
            _openFiles = new Dictionary<string, (StreamReader?, StreamWriter?, string)>();

            // Ensure sandbox exists
            Directory.CreateDirectory(_sandboxPath);
        }

        public string GetSandboxPath() => _sandboxPath;

        public void OpenFile(string filename, string mode)
        {
            var filePath = Path.Combine(_sandboxPath, filename);

            if (_openFiles.ContainsKey(filename))
            {
                throw new Exception($"File '{filename}' is already open");
            }

            mode = mode.ToUpper();

            try
            {
                switch (mode)
                {
                    case "READ":
                        if (!File.Exists(filePath))
                        {
                            throw new Exception($"File not found: {filename}");
                        }
                        var reader = new StreamReader(filePath);
                        _openFiles[filename] = (reader, null, mode);
                        break;

                    case "WRITE":
                        var writer = new StreamWriter(filePath, false); // Overwrite
                        _openFiles[filename] = (null, writer, mode);
                        break;

                    case "APPEND":
                        var appendWriter = new StreamWriter(filePath, true); // Append
                        _openFiles[filename] = (null, appendWriter, mode);
                        break;

                    default:
                        throw new Exception($"Invalid file mode: {mode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open file '{filename}': {ex.Message}");
            }
        }

        public void CloseFile(string filename)
        {
            if (!_openFiles.TryGetValue(filename, out var fileHandle))
            {
                throw new Exception($"File '{filename}' is not open");
            }

            fileHandle.reader?.Close();
            fileHandle.writer?.Close();
            _openFiles.Remove(filename);
        }

        public string ReadLine(string filename)
        {
            if (!_openFiles.TryGetValue(filename, out var fileHandle))
            {
                throw new Exception($"File '{filename}' is not open");
            }

            if (fileHandle.mode != "READ")
            {
                throw new Exception($"File '{filename}' not opened for reading");
            }

            if (fileHandle.reader == null)
            {
                throw new Exception($"No reader available for file '{filename}'");
            }

            if (fileHandle.reader.EndOfStream)
            {
                throw new Exception($"Attempt to read past end of file '{filename}'");
            }

            return fileHandle.reader.ReadLine() ?? string.Empty;
        }

        public void WriteLine(string filename, string data)
        {
            if (!_openFiles.TryGetValue(filename, out var fileHandle))
            {
                throw new Exception($"File '{filename}' is not open");
            }

            if (fileHandle.mode == "READ")
            {
                throw new Exception($"File '{filename}' not opened for writing");
            }

            if (fileHandle.writer == null)
            {
                throw new Exception($"No writer available for file '{filename}'");
            }

            fileHandle.writer.WriteLine(data);
            fileHandle.writer.Flush(); // Ensure data is written
        }

        public bool IsEndOfFile(string filename)
        {
            if (!_openFiles.TryGetValue(filename, out var fileHandle))
            {
                throw new Exception($"File '{filename}' is not open");
            }

            if (fileHandle.mode != "READ")
            {
                throw new Exception($"EOF can only be used with files opened for reading");
            }

            if (fileHandle.reader == null)
            {
                throw new Exception($"No reader available for file '{filename}'");
            }

            return fileHandle.reader.EndOfStream;
        }

        public void ClearSandbox()
        {
            // Close all open files first
            foreach (var filename in new List<string>(_openFiles.Keys))
            {
                CloseFile(filename);
            }

            // Delete all files in sandbox
            if (Directory.Exists(_sandboxPath))
            {
                var files = Directory.GetFiles(_sandboxPath);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore errors
                    }
                }
            }
        }
    }
}
