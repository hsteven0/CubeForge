using CubeForge.Trainers.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Controls
{
    public partial class ScramblePanel : UserControl
    {
        public event EventHandler? PreviousScrambleRequested;
        public event EventHandler? NextScrambleRequested;
        public event EventHandler? ApplyScrambleRequested;
        public event EventHandler? ResetScrambleRequested;
        public event EventHandler? HintRequested;
        public event EventHandler? CaseSelectionChanged;
        public event EventHandler? CaseSelectionOpeningRequested;

        public string HintText => HintTextBlock.Text;

        public ScramblePanel()
        {
            InitializeComponent();
        }

        public void LoadCaseSelection(IEnumerable<CaseViewModel> cases, HashSet<int> selectedCaseNumbers)
        {
            CaseSelectionPopupControl.LoadCases(cases, selectedCaseNumbers);
            SetSelectedCaseCount(selectedCaseNumbers.Count);
        }

        public HashSet<int> GetSelectedCaseNumbers()
        {
            return CaseSelectionPopupControl.GetSelectedCaseNumbers();
        }

        public void SetSelectedCaseCount(int count)
        {
            SelectedCasesButton.Content = count switch
            {
                0 => "No cases selected",
                1 => "1 case selected",
                _ => $"{count} cases selected"
            };
        }

        public void SetScrambleText(string text)
        {
            ScrambleTextBlock.Text = text;
        }

        public void SetHintText(string text)
        {
            HintTextBlock.Text = text;
        }

        private void OnSelectedCases(object sender, RoutedEventArgs e)
        {
            CaseSelectionOpeningRequested?.Invoke(this, EventArgs.Empty);
            SelectedCasesPopup.IsOpen = !SelectedCasesPopup.IsOpen;
        }

        private void OnPreviousScramble(object sender, RoutedEventArgs e)
        {
            PreviousScrambleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnNextScramble(object sender, RoutedEventArgs e)
        {
            NextScrambleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnApplyScramble(object sender, RoutedEventArgs e)
        {
            ApplyScrambleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnResetScramble(object sender, RoutedEventArgs e)
        {
            ResetScrambleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnHintClicked(object sender, RoutedEventArgs e)
        {
            HintRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            SetSelectedCaseCount(GetSelectedCaseNumbers().Count);
            CaseSelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
