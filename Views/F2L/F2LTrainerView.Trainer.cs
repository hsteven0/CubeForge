using CubeForge.Controls;
using CubeForge.Trainers.Shared;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class F2LTrainerView : UserControl
    {
        private void OnModeChanged(object? sender, ModeChangedArgs e)
        {
            TrainerViewMode selectedMode = e.Mode == "Trainer" ? TrainerViewMode.Trainer : TrainerViewMode.Algorithm;
            mode.SetMode(selectedMode);

            TrainerSlotPanel.Visibility = selectedMode == TrainerViewMode.Trainer ? Visibility.Visible : Visibility.Collapsed;

            if (selectedMode == TrainerViewMode.Trainer)
            {
                ApplySelectedOrientation();
            }
            else if (selectedCase != null && !playback.IsBusy)
            {
                ResetSelectedCase();
            }
        }

        private void OnCaseSelectOpen(object? sender, EventArgs e)
        {
            UpdateCaseSelection();
        }

        private void OnCaseSelectChanged(object? sender, EventArgs e)
        {
            caseSelection.SelectedTrainerCases.Clear();

            foreach (int caseNumber in TrainerScramblePanel.GetSelectedCaseNumbers())
                caseSelection.SelectedTrainerCases.Add(caseNumber);

            UpdateCaseSelection();
        }

        private void UpdateCaseSelection()
        {
            if (TrainerScramblePanel == null || viewModel == null)
                return;

            TrainerScramblePanel.LoadCaseSelection(viewModel.Cases, caseSelection.SelectedTrainerCases);
        }

        private void OnNextScramble(object? sender, EventArgs e)
        {
            scramble.Next();
            ApplySelectedMode();
        }

        private void OnPrevScramble(object? sender, EventArgs e)
        {
            scramble.Previous();
            ApplySelectedMode();
        }

        private async void OnApplyScramble(object? sender, EventArgs e)
        {
            await scramble.ApplyAsync(animated: true);
            ApplySelectedMode();
        }

        private async void OnTrainerReset(object? sender, EventArgs e)
        {
            await scramble.ResetAsync();
            ApplySelectedMode();
        }

        private void OnHintClicked(object? sender, EventArgs e)
        {
            scramble.ShowHint();
        }
    }
}
