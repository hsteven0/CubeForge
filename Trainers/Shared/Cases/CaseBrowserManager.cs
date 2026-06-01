using CubeForge.Controls;
using CubeForge.Trainers.Common;
using CubeForge.Views;
using System;
using System.IO;
using System.Windows;

namespace CubeForge.Trainers.Shared
{
    public sealed class CaseBrowserManager<TCase> where TCase : ICaseInfo
    {
        private readonly CaseSelection<TCase> caseSelection;
        private readonly AlgorithmCaseViewModel viewModel;
        private readonly ScramblePanel scramblePanel;
        private readonly CaseBrowser caseBrowser;
        private readonly string trainerName;
        private readonly string jsonFileName;
        private readonly Action<CaseViewModel> onCasePicked;
        private readonly Action<AlgorithmViewModel> onAlgorithmPicked;

        public CaseBrowserManager(CaseSelection<TCase> caseSelection, AlgorithmCaseViewModel viewModel, ScramblePanel scramblePanel, CaseBrowser caseBrowser, string trainerName, string jsonFileName, Action<CaseViewModel> onCasePicked, Action<AlgorithmViewModel> onAlgorithmPicked)
        {
            this.caseSelection = caseSelection;
            this.viewModel = viewModel;
            this.scramblePanel = scramblePanel;
            this.caseBrowser = caseBrowser;
            this.trainerName = trainerName;
            this.jsonFileName = jsonFileName;
            this.onCasePicked = onCasePicked;
            this.onAlgorithmPicked = onAlgorithmPicked;
        }

        public void Attach()
        {
            scramblePanel.CaseSelectionChanged += OnCasePoolChanged;
            scramblePanel.CaseSelectionOpeningRequested += OnCasePickerOpening;
            caseBrowser.CaseSelected += OnCasePicked;
            caseBrowser.AlgorithmSelected += OnAlgorithmPicked;
            caseBrowser.KnownStateChanged += OnKnownChanged;
            caseBrowser.SearchTextChanged += OnSearchChanged;
        }

        public void LoadCases()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", jsonFileName);

            try
            {
                caseSelection.LoadFromJson(jsonPath, trainerName);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Could not find {trainerName} data file:\n{jsonPath}");
            }
        }

        public void LoadPopup()
        {
            if (viewModel.Cases.Count == 0)
                viewModel.LoadCases();

            if (caseSelection.SelectedTrainerCases.Count == 0 && caseSelection.AllCases.Count > 0)
                caseSelection.SelectAll();

            scramblePanel.LoadCaseSelection(viewModel.Cases, caseSelection.SelectedTrainerCases);
        }

        public void UpdateSelectedCount()
        {
            scramblePanel.SetSelectedCaseCount(caseSelection.SelectedTrainerCases.Count);
        }

        public void SetSearchText(string searchText)
        {
            viewModel.SearchText = searchText;
        }

        public void SetSearchFromBrowser()
        {
            viewModel.SearchText = caseBrowser.SearchText;
        }

        private void RefreshPopup()
        {
            LoadPopup();
            UpdateSelectedCount();
        }

        private void OnCasePickerOpening(object? sender, EventArgs e)
        {
            LoadPopup();
        }

        private void OnCasePoolChanged(object? sender, EventArgs e)
        {
            caseSelection.SelectedTrainerCases.Clear();

            foreach (int number in scramblePanel.GetSelectedCaseNumbers())
                caseSelection.SelectedTrainerCases.Add(number);

            UpdateSelectedCount();
        }

        private void OnCasePicked(object? sender, CaseViewModel trainerCase)
        {
            onCasePicked(trainerCase);
        }

        private void OnAlgorithmPicked(object? sender, AlgorithmViewModel algorithm)
        {
            onAlgorithmPicked(algorithm);
        }

        private void OnKnownChanged(object? sender, CaseViewModel trainerCase)
        {
            if (trainerCase.IsKnown)
                caseSelection.KnownCases.Add(trainerCase.Number);
            else
                caseSelection.KnownCases.Remove(trainerCase.Number);

            LoadPopup();
        }

        private void OnSearchChanged(object? sender, string searchText)
        {
            SetSearchText(searchText);
        }
    }
}
