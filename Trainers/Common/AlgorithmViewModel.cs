using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CubeForge.Trainers.Common
{
    public class AlgorithmViewModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public string Name { get; set; } = "";
        public string Moves { get; set; } = "";
        public string Source { get; set; } = "";

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}