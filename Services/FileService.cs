using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public class FileService : IFileService
    {
        private readonly ISettingsService _settingsService;
        private const int MaxRecentFiles = 10;

        public FileService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<string?> LoadProgramAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                AddRecentFile(filePath);
                return code;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load file: {ex.Message}", ex);
            }
        }

        public async Task SaveProgramAsync(string filePath, string code)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, code, Encoding.UTF8);
                AddRecentFile(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save file: {ex.Message}", ex);
            }
        }

        public List<string> GetRecentFiles()
        {
            var settings = _settingsService.LoadSettings();
            return settings.RecentFiles ?? new List<string>();
        }

        public void AddRecentFile(string filePath)
        {
            var settings = _settingsService.LoadSettings();

            // Remove if already exists
            settings.RecentFiles.Remove(filePath);

            // Add to front
            settings.RecentFiles.Insert(0, filePath);

            // Keep only last MaxRecentFiles
            if (settings.RecentFiles.Count > MaxRecentFiles)
            {
                settings.RecentFiles = settings.RecentFiles.GetRange(0, MaxRecentFiles);
            }

            _settingsService.SaveSettingsAsync(settings).Wait();
        }

        public void ClearRecentFiles()
        {
            var settings = _settingsService.LoadSettings();
            settings.RecentFiles.Clear();
            _settingsService.SaveSettingsAsync(settings).Wait();
        }
    }
}
