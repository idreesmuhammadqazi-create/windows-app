using PseudoRun.Desktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Services
{
    public class ExamplesService : IExamplesService
    {
        private List<Example>? _examples;
        private readonly string _examplesFilePath;

        public ExamplesService()
        {
            _examplesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Examples.json");
        }

        private async Task EnsureExamplesLoadedAsync()
        {
            if (_examples == null)
            {
                await LoadExamplesAsync();
            }
        }

        private async Task LoadExamplesAsync()
        {
            try
            {
                if (!File.Exists(_examplesFilePath))
                {
                    _examples = new List<Example>();
                    return;
                }

                var json = await File.ReadAllTextAsync(_examplesFilePath);
                _examples = JsonSerializer.Deserialize<List<Example>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Example>();
            }
            catch (Exception)
            {
                _examples = new List<Example>();
            }
        }

        public async Task<List<Example>> GetAllExamplesAsync()
        {
            await EnsureExamplesLoadedAsync();
            return _examples ?? new List<Example>();
        }

        public async Task<List<Example>> GetExamplesByCategoryAsync(string category)
        {
            await EnsureExamplesLoadedAsync();
            if (string.IsNullOrEmpty(category) || category == "All")
            {
                return _examples ?? new List<Example>();
            }
            return _examples?.Where(e => e.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true).ToList()
                   ?? new List<Example>();
        }

        public async Task<List<Example>> GetExamplesByDifficultyAsync(string difficulty)
        {
            await EnsureExamplesLoadedAsync();
            if (string.IsNullOrEmpty(difficulty) || difficulty == "All")
            {
                return _examples ?? new List<Example>();
            }
            return _examples?.Where(e => e.Difficulty?.Equals(difficulty, StringComparison.OrdinalIgnoreCase) == true).ToList()
                   ?? new List<Example>();
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            await EnsureExamplesLoadedAsync();
            var categories = _examples?
                .Where(e => !string.IsNullOrEmpty(e.Category))
                .Select(e => e.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList() ?? new List<string>();

            categories.Insert(0, "All");
            return categories;
        }

        public async Task<List<string>> GetAllDifficultiesAsync()
        {
            await EnsureExamplesLoadedAsync();
            var difficulties = _examples?
                .Where(e => !string.IsNullOrEmpty(e.Difficulty))
                .Select(e => e.Difficulty!)
                .Distinct()
                .OrderBy(d => d)
                .ToList() ?? new List<string>();

            difficulties.Insert(0, "All");
            return difficulties;
        }
    }
}
