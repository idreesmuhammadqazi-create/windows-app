using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using PseudoRun.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class SyntaxReferenceViewModel : ViewModelBase
    {
        private readonly string _dataFilePath;
        private List<SyntaxCategory> _allCategories = new();

        [ObservableProperty]
        private ObservableCollection<SyntaxCategory> _categories = new();

        [ObservableProperty]
        private SyntaxCategory? _selectedCategory;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        public SyntaxReferenceViewModel()
        {
            _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SyntaxReference.json");
            LoadSyntaxData();
        }

        private void LoadSyntaxData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var categories = JsonConvert.DeserializeObject<List<SyntaxCategory>>(json);

                    if (categories != null && categories.Any())
                    {
                        _allCategories = categories;
                        ApplySearch();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading syntax reference: {ex.Message}");
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                // No search term - show all categories
                Categories = new ObservableCollection<SyntaxCategory>(_allCategories);
            }
            else
            {
                // Filter categories and items based on search term
                var searchLower = SearchTerm.ToLower();
                var filtered = _allCategories
                    .Select(cat => new SyntaxCategory
                    {
                        Category = cat.Category,
                        Items = cat.Items
                            .Where(item =>
                                item.Title.ToLower().Contains(searchLower) ||
                                item.Description.ToLower().Contains(searchLower) ||
                                item.Syntax.ToLower().Contains(searchLower))
                            .ToList()
                    })
                    .Where(cat => cat.Items.Any())
                    .ToList();

                Categories = new ObservableCollection<SyntaxCategory>(filtered);
            }

            // Auto-select first category if none selected or current selection is filtered out
            if (SelectedCategory == null || !Categories.Contains(SelectedCategory))
            {
                SelectedCategory = Categories.FirstOrDefault();
            }
        }
    }
}
