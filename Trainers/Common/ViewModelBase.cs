using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CubeForge.Trainers.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private CaseViewModel? selectedCase;
        private string selectedMode = "Algorithm";
        private string searchText = "";
        private bool showKnown = true;
        private bool showUnknown = true;

        public ObservableCollection<CaseViewModel> Cases { get; } = new();
        public ObservableCollection<CaseViewModel> FilteredCases { get; } = new();
        public ObservableCollection<CaseGroupViewModel> FilteredGroups { get; } = new();

        protected abstract IEnumerable<CaseViewModel> LoadTrainerCases();
        public event PropertyChangedEventHandler? PropertyChanged;

        public CaseViewModel? SelectedCase
        {
            get => selectedCase;
            set
            {
                if (selectedCase == value)
                    return;

                if (selectedCase != null)
                    selectedCase.IsSelected = false;

                selectedCase = value;

                if (selectedCase != null)
                    selectedCase.IsSelected = true;

                OnPropertyChanged();
            }
        }

        public string SelectedMode
        {
            get => selectedMode;
            set
            {
                if (selectedMode == value)
                    return;

                selectedMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAlgorithmMode));
                OnPropertyChanged(nameof(IsTrainerMode));
            }
        }

        public bool IsAlgorithmMode => SelectedMode == "Algorithm";
        public bool IsTrainerMode => SelectedMode == "Trainer";

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText == value)
                    return;

                searchText = value;
                OnPropertyChanged();
                RefreshFilteredCases();
            }
        }

        public bool ShowKnown
        {
            get => showKnown;
            set
            {
                if (showKnown == value)
                    return;

                showKnown = value;
                OnPropertyChanged();
                RefreshFilteredCases();
            }
        }

        public bool ShowUnknown
        {
            get => showUnknown;
            set
            {
                if (showUnknown == value)
                    return;

                showUnknown = value;
                OnPropertyChanged();
                RefreshFilteredCases();
            }
        }

        public void LoadCases()
        {
            Cases.Clear();
            FilteredCases.Clear();
            FilteredGroups.Clear();

            foreach (CaseViewModel trainerCase in LoadTrainerCases())
            {
                trainerCase.SelectDefaultAlgorithm();
                Cases.Add(trainerCase);
            }

            RefreshFilteredCases();

            SelectedCase = FilteredCases.FirstOrDefault();
        }

        public void RefreshFilteredCases()
        {
            FilteredCases.Clear();
            FilteredGroups.Clear();

            foreach (CaseViewModel trainerCase in Cases)
            {
                if (!ShouldShowCase(trainerCase))
                    continue;

                FilteredCases.Add(trainerCase);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredGroups.Add(new CaseGroupViewModel($"Search Results ({FilteredCases.Count})", FilteredCases));
                return;
            }

            foreach (IGrouping<string, CaseViewModel> group in FilteredCases.GroupBy(c => c.Group))
                FilteredGroups.Add(new CaseGroupViewModel(group.Key, group));
        }

        public void SelectCase(CaseViewModel trainerCase)
        {
            SelectedCase = trainerCase;
        }

        protected virtual bool ShouldShowCase(CaseViewModel trainerCase)
        {
            if (trainerCase.IsKnown && !ShowKnown)
                return false;

            if (!trainerCase.IsKnown && !ShowUnknown)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            string query = SearchText.Trim().ToLowerInvariant();

            return trainerCase.Name.ToLowerInvariant().Contains(query) ||
                   trainerCase.Group.ToLowerInvariant().Contains(query) ||
                   trainerCase.Number.ToString().Contains(query);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
