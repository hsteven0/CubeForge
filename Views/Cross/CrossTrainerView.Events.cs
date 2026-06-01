using CubeForge.Cube.FastState;
using CubeForge.Trainers.Cross;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class CrossTrainerView
    {
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            cubeInput.UpdateCamera();
            target.UpdateSlotVisibility();
            ApplySelectedOrientation();
            ApplySelectedDisplayMode();

            solution.Hide();
            UpdatePracticePanel();
        }

        private void WireUiEvents()
        {
            TrainingTypeComboBox.SelectionChanged += OnTrainingChanged;
            GenerationModeComboBox.SelectionChanged += OnTrainerChanged;
            DifficultyComboBox.SelectionChanged += OnTrainerChanged;
            SolutionLengthComboBox.SelectionChanged += OnTrainerChanged;

            TargetSlot1ComboBox.SelectionChanged += OnTrainerChanged;
            TargetSlot2ComboBox.SelectionChanged += OnTrainerChanged;
            TargetSlot3ComboBox.SelectionChanged += OnTrainerChanged;
        }

        private void OnGenerateScramble(object sender, RoutedEventArgs e)
        {
            solution.Stop();

            currentTarget = target.CreateTarget();
            currentTrainingType = target.GetSelectedTrainingType();
            ApplySelectedDisplayMode();

            try
            {
                CrossScrambleResult result = scramble.Generate(currentTarget);
                currentScramble = result.Scramble;
                guaranteedSolution = result.GuaranteedSolution;
            }
            catch (InvalidOperationException ex)
            {
                ScrambleTextBlock.Text = ex.Message;
                currentScramble = "";
                solution.CurrentSolution.Clear();
                guaranteedSolution.Clear();
                solution.Hide();
                UpdatePracticePanel();
                return;
            }

            ScrambleTextBlock.Text = currentScramble;
            target.UpdatePracticePanel();

            scramble.Clear();

            solution.CurrentSolution.Clear();
            solution.Hide();
            UpdatePracticePanel();
        }

        private async void OnApplyScramble(object sender, RoutedEventArgs e)
        {
            solution.Stop();

            string scramble = ScrambleTextBlock.Text;

            if (string.IsNullOrWhiteSpace(scramble) ||
                scramble == "Generate a scramble to begin.")
            {
                MessageBox.Show("Generate a scramble first.");
                return;
            }

            await this.scramble.Apply(scramble);

            solution.CurrentSolution.Clear();
            solution.Hide(resetHints: false);
            UpdatePracticePanel();
        }

        private void OnResetCube(object sender, RoutedEventArgs e)
        {
            solution.Stop();
            scramble.ResetCube();

            solution.CurrentSolution.Clear();
            solution.Hide();
            UpdatePracticePanel();
        }

        private async void OnRevealSolution(object sender, RoutedEventArgs e)
        {
            if (!scramble.ScrambleApplied)
            {
                await scramble.ApplyCurrentInstantly(currentScramble);
            }

            try
            {
                currentTarget = target.CreateTarget();
                scramble.EnsurePruningTablesLoaded(currentTarget);

                List<SearchMove> solutionMoves;

                if (guaranteedSolution.Count > 0)
                {
                    solutionMoves = new List<SearchMove>(guaranteedSolution);
                }
                else
                {
                    int maxDepth = target.GetSelectedSolutionLength();

                    NCrossSearchResult result = NCrossSearcher.Solve(cube.FastCube.Clone(), currentTarget, maxDepth);

                    if (!result.Found)
                    {
                        solution.ShowMessage($"No solution found up to length {maxDepth}.");
                        solution.ShowRevealButton();
                        RefreshTrainerLayouts();
                        return;
                    }

                    solutionMoves = new List<SearchMove>(result.Solution);
                }

                solution.SetSolution(solutionMoves);
                UpdatePracticePanel();

                RefreshTrainerLayouts();
            }
            catch (InvalidOperationException ex)
            {
                solution.ShowMessage(ex.Message);
                RefreshTrainerLayouts();
            }
        }

        private async void OnApplySolution(object sender, RoutedEventArgs e)
        {
            await solution.ApplyAllMoves(currentTarget, scramble.ScrambledFastState, scramble.AppliedScrambleText);
        }

        private async void OnPreviousSolution(object sender, RoutedEventArgs e)
        {
            await solution.PrevMove(currentTarget);
        }

        private async void OnNextSolution(object sender, RoutedEventArgs e)
        {
            await solution.NextMove(currentTarget, scramble.ScrambledFastState, scramble.AppliedScrambleText);
        }

        private void OnNextHint(object sender, RoutedEventArgs e)
        {
            solution.ShowNextHint(guaranteedSolution);
        }

        private void OnTrainingChanged(object sender, SelectionChangedEventArgs e)
        {
            solution.Stop();
            target.UpdateSlotVisibility();
            ApplySelectedDisplayMode();

            solution.CurrentSolution.Clear();
            solution.Hide();
            UpdatePracticePanel();
        }

        private void OnTrainerChanged(object sender, SelectionChangedEventArgs e)
        {
            solution.Stop();
            ApplySelectedDisplayMode();

            solution.CurrentSolution.Clear();
            solution.Hide();
            UpdatePracticePanel();
        }
    }
}
