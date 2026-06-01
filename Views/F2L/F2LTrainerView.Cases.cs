using CubeForge.Trainers.Shared;
using CubeForge.Trainers.Pairs;
using CubeForge.Trainers.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class F2LTrainerView : UserControl
    {
        private string GetAlgorithmForSlot(F2LCase f2lCase, PairSlot slot)
        {
            string key = slot.ToString();

            if (!f2lCase.AlgorithmsBySlot.TryGetValue(key, out List<F2LAlgorithm>? algorithms))
                return "";

            return algorithms.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Moves))?.Moves ?? "";
        }

        private string GetSetupForSlot(F2LCase f2lCase, PairSlot slot)
        {
            return f2lCase.SetupMoves;
        }

        private void OnCasePicked(object? sender, CaseViewModel trainerCase)
        {
            viewModel.SelectCase(trainerCase);
            DisplaySelectedCase(trainerCase);
        }

        private void OnAlgorithmPicked(object? sender, AlgorithmViewModel algorithm)
        {
            if (viewModel.SelectedCase == null || selectedCase == null)
                return;

            int algorithmIndex = viewModel.SelectedCase.Algorithms.IndexOf(algorithm);

            if (algorithmIndex >= 0)
                selectedAlgorithms[GetAlgorithmSelectionKey(selectedCase)] = algorithmIndex;

            ResetSelectedCase();
        }

        private void OnKnownChanged(object? sender, CaseViewModel trainerCase)
        {
            if (trainerCase.IsKnown)
                caseSelection.KnownCases.Add(trainerCase.Number);
            else
                caseSelection.KnownCases.Remove(trainerCase.Number);
        }

        private void OnBrowserSearch(object? sender, string searchText)
        {
            viewModel.SearchText = searchText;
        }

        private void OnSlotChanged(object? sender, string slotKey)
        {
            pairSlots.SelectSlot(slotKey);
        }

        private void OnPairSlotChanged(string slotKey)
        {
            viewModel.SelectedSetKey = slotKey;
        }

        private void DisplaySelectedCase(CaseViewModel trainerCase)
        {
            F2LCase? matchingCase = caseSelection.AllCases.FirstOrDefault(c => c.Number == trainerCase.Number);

            if (matchingCase == null)
                return;

            selectedCase = matchingCase;
            mode.SetAlgorithmImage(imageHelper.LoadImage(trainerCase.PreviewImage));
            ResetSelectedCase();
        }

        private void LoadF2LCases()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "f2l_cases.json");

            try
            {
                caseSelection.LoadFromJson(jsonPath, "F2L");
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Could not find F2L JSON:\n{jsonPath}");
            }
        }

        private string GetAlgorithmSelectionKey(F2LCase f2lCase)
        {
            return $"{f2lCase.Number}:{pairSlots.SelectedSlot}";
        }

        private List<F2LAlgorithm> GetAlgForSelectedSlot(F2LCase f2lCase)
        {
            string slotKey = pairSlots.SelectedSlot.ToString();

            if (f2lCase.AlgorithmsBySlot.TryGetValue(slotKey, out List<F2LAlgorithm>? algorithms))
                return algorithms;

            return new List<F2LAlgorithm>();
        }

        private void ResetSelectedCase()
        {
            if (selectedCase == null)
                return;

            string alg = viewModel.SelectedCase?.Number == selectedCase.Number
                ? viewModel.SelectedCase.SelectedAlgorithm?.Moves 
                ?? ""
                : "";

            string setupMoves = pairSlots.GetSetupForSelectedSlot(selectedCase.SetupMoves);
            playback.SetCase(selectedCase.Name, selectedCase.Group, setupMoves, alg);
        }
    }
}
