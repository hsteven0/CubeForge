using CubeForge.Controls;
using CubeForge.Cube;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CubeForge.Trainers.Shared
{
    public sealed class AlgorithmPlaybackManager
    {
        private readonly CubeManager cube;
        private readonly AlgorithmPlaybackPanel panel;
        private readonly Func<bool> instantMoves;
        private readonly Func<int> animationSpeed;

        private List<ParsedMove> moves = new();
        private string setupMoves = "";
        private int moveIndex = 0;

        public bool IsBusy { get; private set; }
        public bool HasCase { get; private set; }

        public AlgorithmPlaybackManager(CubeManager cube, AlgorithmPlaybackPanel panel, Func<bool> getInstantMoves, Func<int> getAnimationSpeed)
        {
            this.cube = cube;
            this.panel = panel;
            this.instantMoves = getInstantMoves;
            this.animationSpeed = getAnimationSpeed;
        }

        public void AttachToPanel()
        {
            panel.ResetRequested += OnResetRequested;
            panel.ApplyAlgorithmRequested += OnApplyAlgorithmRequested;
            panel.PreviousMoveRequested += OnPreviousMoveRequested;
            panel.NextMoveRequested += OnNextMoveRequested;
        }

        public void Stop()
        {
            cube.StopMoves();
            IsBusy = false;
        }

        private void OnResetRequested(object? sender, EventArgs e)
        {
            Reset();
        }

        private async void OnApplyAlgorithmRequested(object? sender, EventArgs e)
        {
            await ApplyAllMoves();
        }

        private async void OnPreviousMoveRequested(object? sender, EventArgs e)
        {
            await PreviousMove();
        }

        private async void OnNextMoveRequested(object? sender, EventArgs e)
        {
            await NextMove();
        }

        public void SetCase(string title, string group, string setupMoves, string algorithmMoves)
        {
            Stop();

            HasCase = true;
            this.setupMoves = setupMoves ?? "";
            moves = ScrambleParser.Parse(algorithmMoves ?? "");
            moveIndex = 0;

            panel.SetCaseInfo(title, group);
            Reset();
        }

        public void SetAlgorithm(string algorithmMoves)
        {
            Stop();

            moves = ScrambleParser.Parse(algorithmMoves ?? "");
            moveIndex = 0;
            Reset();
        }

        public void Reset()
        {
            if (!HasCase)
                return;

            Stop();
            ResetForPlayback();
            UpdateDisplay();
        }

        public async Task ApplyAllMoves()
        {
            if (!CanStep())
                return;

            try
            {
                IsBusy = true;
                ResetForPlayback();

                for (int i = 0; i < moves.Count; i++)
                {
                    UpdateDisplay(activeIndex: i);
                    await cube.ApplyMoveAsync(moves[i], instantMoves(), animationSpeed());
                    UpdateDisplay(completedIndex: i);
                }

                moveIndex = moves.Count;
                UpdateDisplay(completed: true);
            }
            catch (OperationCanceledException)
            {
                UpdateDisplay();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task NextMove()
        {
            if (!CanStep())
                return;

            try
            {
                IsBusy = true;

                if (moveIndex >= moves.Count)
                {
                    ResetForPlayback();
                    UpdateDisplay();
                }

                int index = moveIndex;
                UpdateDisplay(activeIndex: index);

                await cube.ApplyMoveAsync(moves[index], instantMoves(), animationSpeed());

                moveIndex++;

                if (moveIndex >= moves.Count)
                    UpdateDisplay(completed: true);
                else
                    UpdateDisplay(completedIndex: index);
            }
            catch (OperationCanceledException)
            {
                UpdateDisplay();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task PreviousMove()
        {
            if (!CanStep() || moveIndex <= 0)
                return;

            try
            {
                IsBusy = true;

                int previousIndex = moveIndex - 1;
                ParsedMove inverseMove = Invert(moves[previousIndex]);

                UpdateDisplay(activeIndex: previousIndex);
                await cube.ApplyMoveAsync(inverseMove, instantMoves(), animationSpeed());

                moveIndex--;

                if (moveIndex > 0)
                    UpdateDisplay(completedIndex: moveIndex - 1);
                else
                    UpdateDisplay();
            }
            catch (OperationCanceledException)
            {
                UpdateDisplay();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanStep()
        {
            return HasCase && !IsBusy && moves.Count > 0;
        }

        private void ResetForPlayback()
        {
            cube.Reset();
            cube.ApplyMoves(ScrambleParser.Parse(setupMoves), renderImmediately: true);
            moveIndex = 0;
        }

        private void UpdateDisplay(int activeIndex = -1, int completedIndex = -1, bool completed = false)
        {
            panel.SetAlgorithmDisplay(moves, activeIndex, completedIndex, completed);
        }

        private static ParsedMove Invert(ParsedMove move)
        {
            if (move.Turns == 1)
                return new ParsedMove(move.Move, 3);

            if (move.Turns == 3)
                return new ParsedMove(move.Move, 1);

            if (move.Turns == 2)
                return new ParsedMove(move.Move, 2, !move.DoublePrime);

            return move;
        }
    }
}
