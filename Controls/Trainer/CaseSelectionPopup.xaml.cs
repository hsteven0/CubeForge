using CubeForge.Trainers.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Controls
{
    public partial class CaseSelectionPopup : UserControl
    {
        public event EventHandler? SelectionChanged;

        public ObservableCollection<TrainerCaseSelectionGroupViewModel> Groups { get; } = new();

        private bool isChangingSelection;

        public CaseSelectionPopup()
        {
            InitializeComponent();
            GroupsItemsControl.ItemsSource = Groups;
        }

        public void LoadCases(IEnumerable<CaseViewModel> cases, HashSet<int> selectedCaseNumbers)
        {
            isChangingSelection = true;

            try
            {
                Groups.Clear();

                foreach (IGrouping<string, CaseViewModel> group in cases.GroupBy(c => c.Group))
                {
                    TrainerCaseSelectionGroupViewModel groupViewModel = new TrainerCaseSelectionGroupViewModel(group.Key);

                    foreach (CaseViewModel trainerCase in group.OrderBy(c => c.Number))
                    {
                        groupViewModel.Cases.Add(new TrainerCaseSelectionItemViewModel
                        {
                            Number = trainerCase.Number,
                            Name = trainerCase.Name,
                            Group = trainerCase.Group,
                            IsKnown = trainerCase.IsKnown,
                            IsTrainerSelected = selectedCaseNumbers.Contains(trainerCase.Number)
                        });
                    }

                    Groups.Add(groupViewModel);
                }

                RefreshSummary();
            }
            finally
            {
                isChangingSelection = false;
            }
        }

        private void OnManualScroll(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            double scrollAmount = e.Delta > 0 ? -42 : 42;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);

            e.Handled = true;
        }

        public HashSet<int> GetSelectedCaseNumbers()
        {
            return GetAllItems().Where(c => c.IsTrainerSelected).Select(c => c.Number).ToHashSet();
        }

        public void SelectAll()
        {
            SetItemsSelected(GetAllItems(), true);
        }

        public void SelectUnknownOnly()
        {
            SetItemsSelected(GetAllItems(), item => !item.IsKnown);
        }

        public void SelectKnownOnly()
        {
            SetItemsSelected(GetAllItems(), item => item.IsKnown);
        }

        public void ClearSelection()
        {
            SetItemsSelected(GetAllItems(), false);
        }

        private List<TrainerCaseSelectionItemViewModel> GetAllItems()
        {
            return Groups.SelectMany(g => g.Cases).ToList();
        }

        private void SetItemsSelected(IEnumerable<TrainerCaseSelectionItemViewModel> items, bool selected)
        {
            SetItemsSelected(items, _ => selected);
        }

        private void SetItemsSelected(IEnumerable<TrainerCaseSelectionItemViewModel> items, Func<TrainerCaseSelectionItemViewModel, bool> shouldSelect)
        {
            List<TrainerCaseSelectionItemViewModel> itemList = items.ToList();

            isChangingSelection = true;

            try
            {
                foreach (TrainerCaseSelectionItemViewModel item in itemList)
                    item.IsTrainerSelected = shouldSelect(item);

                RefreshSummary();
            }
            finally
            {
                isChangingSelection = false;
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTrainAllClicked(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void OnTrainUnknownClicked(object sender, RoutedEventArgs e)
        {
            SelectUnknownOnly();
        }

        private void OnTrainKnownClicked(object sender, RoutedEventArgs e)
        {
            SelectKnownOnly();
        }

        private void OnClearSelection(object sender, RoutedEventArgs e)
        {
            ClearSelection();
        }

        private void OnEditCasesClicked(object sender, RoutedEventArgs e)
        {
            bool isOpen = ManualSelectionScrollViewer.Visibility != Visibility.Visible;

            ManualSelectionScrollViewer.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
            EditCasesButton.Content = isOpen ? "Hide individual cases" : "Edit individual cases";
        }

        private void OnSelectGroupClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string groupName)
                return;

            TrainerCaseSelectionGroupViewModel? group = Groups.FirstOrDefault(g => g.Name == groupName);

            if (group == null)
                return;

            SetItemsSelected(group.Cases, true);
        }

        private void OnClearGroupClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string groupName)
                return;

            TrainerCaseSelectionGroupViewModel? group = Groups.FirstOrDefault(g => g.Name == groupName);

            if (group == null)
                return;

            SetItemsSelected(group.Cases, false);
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (isChangingSelection)
                return;

            RefreshSummary();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshSummary()
        {
            int selected = GetSelectedCaseNumbers().Count;

            SummaryTextBlock.Text = selected switch
            {
                0 => "No cases are currently selected.",
                1 => "1 case can appear in trainer mode.",
                _ => $"{selected} cases can appear in trainer mode."
            };
        }
    }

    public class TrainerCaseSelectionGroupViewModel
    {
        public string Name { get; }
        public ObservableCollection<TrainerCaseSelectionItemViewModel> Cases { get; } = new();

        public TrainerCaseSelectionGroupViewModel(string name)
        {
            Name = name;
        }
    }

    public class TrainerCaseSelectionItemViewModel : INotifyPropertyChanged
    {
        private bool isTrainerSelected;

        public int Number { get; set; }
        public string Name { get; set; } = "";
        public string Group { get; set; } = "";
        public bool IsKnown { get; set; }
        public string DisplayName => Name;

        public bool IsTrainerSelected
        {
            get => isTrainerSelected;
            set
            {
                if (isTrainerSelected == value)
                    return;

                isTrainerSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}