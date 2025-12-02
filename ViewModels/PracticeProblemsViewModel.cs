using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PseudoRun.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class PracticeProblemsViewModel : ViewModelBase
    {
        private readonly string _dataFilePath;

        [ObservableProperty]
        private ObservableCollection<PracticeProblem> _allProblems = new();

        [ObservableProperty]
        private ObservableCollection<PracticeProblem> _filteredProblems = new();

        [ObservableProperty]
        private PracticeProblem? _selectedProblem;

        [ObservableProperty]
        private bool _isSolutionVisible = false;

        [ObservableProperty]
        private string _selectedLevel = "All";

        [ObservableProperty]
        private string _selectedTopic = "All";

        [ObservableProperty]
        private ObservableCollection<string> _availableLevels = new();

        [ObservableProperty]
        private ObservableCollection<string> _availableTopics = new();

        public event EventHandler<string>? LoadToEditorRequested;

        public PracticeProblemsViewModel()
        {
            _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "PracticeProblems.json");
            LoadProblems();
        }

        private void LoadProblems()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var problems = JsonConvert.DeserializeObject<List<PracticeProblem>>(json);

                    if (problems != null)
                    {
                        AllProblems = new ObservableCollection<PracticeProblem>(problems);

                        // Extract unique levels and topics
                        var levels = new List<string> { "All" };
                        levels.AddRange(problems.Select(p => p.Level).Distinct().OrderBy(l => l));
                        AvailableLevels = new ObservableCollection<string>(levels);

                        var topics = new List<string> { "All" };
                        topics.AddRange(problems.Select(p => p.Topic).Distinct().OrderBy(t => t));
                        AvailableTopics = new ObservableCollection<string>(topics);

                        ApplyFilters();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error - in production, log this
                System.Diagnostics.Debug.WriteLine($"Error loading practice problems: {ex.Message}");
            }
        }

        partial void OnSelectedLevelChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedTopicChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedProblemChanged(PracticeProblem? value)
        {
            // Hide solution when switching problems
            IsSolutionVisible = false;
        }

        private void ApplyFilters()
        {
            var filtered = AllProblems.AsEnumerable();

            if (SelectedLevel != "All")
            {
                filtered = filtered.Where(p => p.Level == SelectedLevel);
            }

            if (SelectedTopic != "All")
            {
                filtered = filtered.Where(p => p.Topic == SelectedTopic);
            }

            FilteredProblems = new ObservableCollection<PracticeProblem>(filtered);

            // Select first problem if none selected or current selection is filtered out
            if (SelectedProblem == null || !FilteredProblems.Contains(SelectedProblem))
            {
                SelectedProblem = FilteredProblems.FirstOrDefault();
            }
        }

        [RelayCommand]
        private void ShowSolution()
        {
            IsSolutionVisible = true;
        }

        [RelayCommand]
        private void HideSolution()
        {
            IsSolutionVisible = false;
        }

        [RelayCommand]
        private void LoadToEditor()
        {
            if (SelectedProblem != null)
            {
                LoadToEditorRequested?.Invoke(this, SelectedProblem.Solution);
            }
        }
    }
}
