using HelixToolkit.Wpf;
using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Trainers.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CubeForge.Trainers.Cross
{
    public sealed class CrossSolutionManager
    {
        private readonly TextBlock solutionTextBlock;
        private readonly TextBlock hintStageTextBlock;
        private readonly TextBlock hintStatusTextBlock;
        private readonly Button revealSolutionButton;
        private readonly Button applySolutionButton;
        private readonly Button previousSolutionButton;
        private readonly Button nextSolutionButton;
        private readonly Func<string> getTargetText;
        private readonly Action refreshLayouts;
        private readonly CubeManager cube;
        private readonly HelixViewport3D viewPort;
        private readonly Func<int> animationSpeed;
        private readonly Action setOrientation;
        private readonly Action setDisplayMode;

        private int hintStage = 0;

        public CrossSolutionManager(TextBlock solutionTextBlock, TextBlock hintStageTextBlock, TextBlock hintStatusTextBlock, Button revealSolutionButton, Button applySolutionButton, Button previousSolutionButton, Button nextSolutionButton, Func<string> getTargetText, Action refreshLayouts, CubeManager cube, HelixViewport3D viewPort, Func<int> getAnimationSpeed, Action applyOrientation, Action applyDisplayMode)
        {
            this.solutionTextBlock = solutionTextBlock;
            this.hintStageTextBlock = hintStageTextBlock;
            this.hintStatusTextBlock = hintStatusTextBlock;
            this.revealSolutionButton = revealSolutionButton;
            this.applySolutionButton = applySolutionButton;
            this.previousSolutionButton = previousSolutionButton;
            this.nextSolutionButton = nextSolutionButton;
            this.getTargetText = getTargetText;
            this.refreshLayouts = refreshLayouts;
            this.cube = cube;
            this.viewPort = viewPort;
            this.animationSpeed = getAnimationSpeed;
            this.setOrientation = applyOrientation;
            this.setDisplayMode = applyDisplayMode;
        }

        public List<SearchMove> CurrentSolution { get; } = new();
        public int CurrentIndex { get; set; }
        public bool IsRevealed { get; private set; }
        public bool IsMoveRunning { get; set; }

        public void ClearSolution()
        {
            Stop();
            CurrentSolution.Clear();
            Hide();
        }

        public void Stop()
        {
            cube.StopMoves();
            IsMoveRunning = false;
        }

        public void Hide()
        {
            Hide(resetHints: true);
        }

        public void Hide(bool resetHints)
        {
            IsRevealed = false;
            CurrentIndex = 0;

            if (resetHints)
            {
                hintStage = 0;
                hintStageTextBlock.Text = "No hint revealed";
                hintStatusTextBlock.Text = "";
            }

            solutionTextBlock.Inlines.Clear();
            solutionTextBlock.Text = "";
            solutionTextBlock.Visibility = Visibility.Collapsed;

            ShowRevealButton();
            refreshLayouts();
        }

        public void ShowRevealButton()
        {
            revealSolutionButton.Visibility = Visibility.Visible;
            applySolutionButton.Visibility = Visibility.Collapsed;
            previousSolutionButton.Visibility = Visibility.Collapsed;
            nextSolutionButton.Visibility = Visibility.Collapsed;
        }

        public void ShowSolutionButtons()
        {
            revealSolutionButton.Visibility = Visibility.Collapsed;
            solutionTextBlock.Visibility = Visibility.Visible;

            applySolutionButton.Visibility = Visibility.Visible;
            previousSolutionButton.Visibility = Visibility.Visible;
            nextSolutionButton.Visibility = Visibility.Visible;
        }

        public void ShowMessage(string text)
        {
            revealSolutionButton.Visibility = Visibility.Collapsed;
            solutionTextBlock.Visibility = Visibility.Visible;
            solutionTextBlock.Inlines.Clear();
            solutionTextBlock.Text = text;
        }

        public void SetSolution(IEnumerable<SearchMove> moves)
        {
            CurrentSolution.Clear();
            CurrentSolution.AddRange(moves);
            CurrentIndex = 0;
            hintStage = 0;
            IsRevealed = true;
            UpdateDisplay();
            ShowSolutionButtons();
        }

        public void ShowNextHint(IReadOnlyList<SearchMove> guaranteedSolution)
        {
            List<SearchMove> hintSource = GetHintMoves(guaranteedSolution);

            if (hintSource.Count == 0)
            {
                hintStageTextBlock.Text = "Reveal a solution first.";
                return;
            }

            bool hasGuaranteedHint = guaranteedSolution.Count > 0;

            int maxHintStage = hasGuaranteedHint ? hintSource.Count : hintSource.Count + 1;

            if (hintStage >= maxHintStage)
            {
                hintStatusTextBlock.Text = "All hints revealed.";
                return;
            }

            hintStage++;

            if (!hasGuaranteedHint && hintStage == 1)
            {
                hintStageTextBlock.Text = getTargetText();
                return;
            }

            int revealCount = hasGuaranteedHint ? hintStage : hintStage - 1;

            revealCount = Math.Min(revealCount, hintSource.Count);

            string moves = string.Join(" ", hintSource.Take(revealCount));

            hintStageTextBlock.Text = $"Hint {hintStage}: {moves}";
        }

        public async Task ApplyAllMoves(NCrossTarget? target, FastCubeState? scrambledState, string scrambleText)
        {
            if (IsMoveRunning)
                return;

            if (!IsRevealed || CurrentSolution.Count == 0)
                return;

            try
            {
                IsMoveRunning = true;
                cube.StopMoves();
                await RestoreScrambledState(scrambledState, scrambleText, instant: true);

                for (int i = 0; i < CurrentSolution.Count; i++)
                {
                    SearchMove move = CurrentSolution[i];
                    await ApplyMove(target, move, activeIndex: i);

                    CurrentIndex = i + 1;
                    UpdateDisplay(completedIndex: i);
                }

                UpdateDisplay(completed: true);
            }
            catch (OperationCanceledException) {}
            finally
            {
                IsMoveRunning = false;
            }
        }

        public async Task PrevMove(NCrossTarget? target)
        {
            if (IsMoveRunning)
                return;

            if (!IsRevealed || CurrentSolution.Count == 0)
                return;

            if (CurrentIndex <= 0)
                return;

            try
            {
                IsMoveRunning = true;

                int previousIndex = CurrentIndex - 1;
                SearchMove moveToUndo = CurrentSolution[previousIndex];
                SearchMove inverseMove = InvertMove(moveToUndo);

                await ApplyMove(target, inverseMove, activeIndex: previousIndex);
                CurrentIndex--;

                if (CurrentIndex > 0)
                    UpdateDisplay(completedIndex: CurrentIndex - 1);
                else
                    UpdateDisplay();
            }
            catch (OperationCanceledException) {}
            finally
            {
                IsMoveRunning = false;
            }
        }

        public async Task NextMove(NCrossTarget? target, FastCubeState? scrambledState, string scrambleText)
        {
            if (IsMoveRunning)
                return;

            if (!IsRevealed || CurrentSolution.Count == 0)
                return;

            try
            {
                IsMoveRunning = true;

                if (CurrentIndex >= CurrentSolution.Count)
                    await RestoreScrambledState(scrambledState, scrambleText, instant: true);

                SearchMove move = CurrentSolution[CurrentIndex];
                int completedIndex = CurrentIndex;

                await ApplyMove(target, move, activeIndex: CurrentIndex);
                CurrentIndex++;

                if (CurrentIndex >= CurrentSolution.Count)
                    UpdateDisplay(completed: true);
                else
                    UpdateDisplay(completedIndex: completedIndex);
            }
            catch (OperationCanceledException) {}
            finally
            {
                IsMoveRunning = false;
            }
        }

        public async Task RestoreScrambledState(FastCubeState? scrambledState, string scrambleText, bool instant)
        {
            if (scrambledState == null)
                return;

            cube.StopMoves();
            cube.Reset();
            cube.FastCube.CopyFrom(scrambledState);

            setOrientation();
            setDisplayMode();

            List<ParsedMove> scrambleMoves = ScrambleParser.Parse(scrambleText);

            foreach (ParsedMove move in scrambleMoves)
            {
                if (instant)
                    cube.Cube.ApplyDisplayedMove(move, viewPort);
                else
                    await cube.ApplyVisualMoveAsync(move, animationSpeed());
            }

            CurrentIndex = 0;
        }

        public void UpdateDisplay(int activeIndex = -1, int completedIndex = -1, bool completed = false)
        {
            solutionTextBlock.Visibility = Visibility.Visible;
            solutionTextBlock.Inlines.Clear();

            if (CurrentSolution.Count == 0)
            {
                solutionTextBlock.Text = "";
                return;
            }

            string progress = $"Move {Math.Min(CurrentIndex, CurrentSolution.Count)}/{CurrentSolution.Count}";

            solutionTextBlock.Inlines.Add(new Run(getTargetText() + "\n")
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            });

            solutionTextBlock.Inlines.Add(new Run(progress + "\n")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175)),
                FontSize = 13
            });

            solutionTextBlock.Inlines.Add(new Run("Solution: ")
            {
                Foreground = Brushes.White
            });

            for (int i = 0; i < CurrentSolution.Count; i++)
            {
                bool isGreen = completed || i == activeIndex || i == completedIndex;

                Brush color = isGreen ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : Brushes.White;

                solutionTextBlock.Inlines.Add(new Run(CurrentSolution[i].ToString())
                {
                    Foreground = color,
                    FontWeight = isGreen ? FontWeights.Bold : FontWeights.Normal
                });

                if (i < CurrentSolution.Count - 1)
                    solutionTextBlock.Inlines.Add(new Run(" "));
            }
        }

        private async Task ApplyMove(NCrossTarget? target, SearchMove move, int activeIndex)
        {
            CubeOrientation orientation = target?.Orientation ?? cube.Cube.CurrentOrientation;
            cube.FastCube.ApplyMove(orientation.ToBaseMove(new ParsedMove(move.Move, move.Turns)));

            UpdateDisplay(activeIndex: activeIndex);

            await cube.ApplyVisualMove(move.Move, move.Turns, animationSpeed());
        }

        private static SearchMove InvertMove(SearchMove move)
        {
            int inverseTurns = move.Turns switch
            {
                1 => 3,
                3 => 1,
                2 => 2,
                _ => move.Turns
            };

            return new SearchMove(move.Move, inverseTurns);
        }

        private List<SearchMove> GetHintMoves(IReadOnlyList<SearchMove> guaranteedSolution)
        {
            if (guaranteedSolution.Count > 0)
                return guaranteedSolution.ToList();

            if (CurrentSolution.Count > 0)
                return CurrentSolution;

            return new List<SearchMove>();
        }
    }
}
