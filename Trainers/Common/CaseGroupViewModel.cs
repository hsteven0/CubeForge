using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CubeForge.Trainers.Common
{
    public class CaseGroupViewModel
    {
        public string Name { get; }
        public ObservableCollection<CaseViewModel> Cases { get; } = new();

        public CaseGroupViewModel(string name, IEnumerable<CaseViewModel> cases)
        {
            Name = name;

            foreach (CaseViewModel trainerCase in cases)
                Cases.Add(trainerCase);
        }
    }
}