using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CubeForge.Trainers.Common
{
    public class CaseViewModel : INotifyPropertyChanged
    {
        private bool isSelected;
        private bool isKnown;
        private bool isExpanded;
        private AlgorithmViewModel? selectedAlgorithm;

        private readonly Dictionary<string, List<AlgorithmViewModel>> algorithmSets = new();

        public int Number { get; set; }
        public string Name { get; set; } = "";
        public string Group { get; set; } = "";
        public string SetupMoves { get; set; } = "";
        public string PreviewImage { get; set; } = "";

        public ObservableCollection<AlgorithmViewModel> Algorithms { get; } = new();

        public AlgorithmViewModel? SelectedAlgorithm
        {
            get => selectedAlgorithm;
            set
            {
                if (selectedAlgorithm == value)
                    return;

                if (selectedAlgorithm != null)
                    selectedAlgorithm.IsSelected = false;

                selectedAlgorithm = value;

                if (selectedAlgorithm != null)
                    selectedAlgorithm.IsSelected = true;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAlgorithmMoves));
            }
        }

        public string SelectedAlgorithmMoves => SelectedAlgorithm?.Moves ?? "No algorithm";

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value)
                    return;

                isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsKnown
        {
            get => isKnown;
            set
            {
                if (isKnown == value)
                    return;

                isKnown = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value)
                    return;

                isExpanded = value;
                OnPropertyChanged();
            }
        }

        public void SetAlgorithms(string type, IEnumerable<AlgorithmViewModel> algorithms)
        {
            algorithmSets[type] = algorithms.ToList();
        }

        public void UseAlgorithms(string type)
        {
            Algorithms.Clear();

            if (algorithmSets.TryGetValue(type, out List<AlgorithmViewModel>? algType))
            {
                foreach (AlgorithmViewModel algorithm in algType)
                    Algorithms.Add(algorithm);
            }

            SelectDefaultAlgorithm();
            OnPropertyChanged(nameof(Algorithms));
            OnPropertyChanged(nameof(SelectedAlgorithmMoves));
        }

        public void SelectAlgorithm(AlgorithmViewModel algorithm)
        {
            if (!Algorithms.Contains(algorithm))
                return;

            SelectedAlgorithm = algorithm;
        }

        public void SelectDefaultAlgorithm()
        {
            SelectedAlgorithm = Algorithms.FirstOrDefault();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
