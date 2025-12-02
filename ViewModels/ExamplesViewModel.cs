using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Models;
using PseudoRun.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class ExamplesViewModel : ViewModelBase
    {
        private readonly IExamplesService _examplesService;

        [ObservableProperty]
        private ObservableCollection<string> _categories = new();

        [ObservableProperty]
        private string? _selectedCategory;

        [ObservableProperty]
        private ObservableCollection<Example> _examples = new();

        [ObservableProperty]
        private ObservableCollection<Example> _filteredExamples = new();

        [ObservableProperty]
        private Example? _selectedExample;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public Action<string>? OnLoadExample { get; set; }

        public ExamplesViewModel(IExamplesService examplesService)
        {
            _examplesService = examplesService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var examples = await _examplesService.GetAllExamplesAsync();
                Examples = new ObservableCollection<Example>(examples);
                FilteredExamples = new ObservableCollection<Example>(examples);

                var categories = await _examplesService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<string>(categories);
                SelectedCategory = "All";
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error initializing examples: {ex.Message}");
            }
        }

        partial void OnSelectedCategoryChanged(string? value)
        {
            ApplyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = Examples.AsEnumerable();

            // Filter by category
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                filtered = filtered.Where(e => e.Category?.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Filter by search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(e =>
                    e.Title?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) == true ||
                    e.Description?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) == true ||
                    e.Tags?.Any(t => t.Contains(searchLower, StringComparison.OrdinalIgnoreCase)) == true
                );
            }

            FilteredExamples = new ObservableCollection<Example>(filtered);
        }

        [RelayCommand]
        private void LoadExample()
        {
            if (SelectedExample != null)
            {
                OnLoadExample?.Invoke(SelectedExample.Code);
            }
        }
    }
}
