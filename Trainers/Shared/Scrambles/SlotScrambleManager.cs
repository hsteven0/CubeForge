using CubeForge.Controls;
using CubeForge.Cube;
using CubeForge.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CubeForge.Trainers.Shared
{
    public sealed class SlotScrambleManager<TCase, TContext> where TCase : ICaseInfo
    {
        private readonly ScramblePanel panel;
        private readonly CaseSelection<TCase> caseSelection;
        private readonly SlotScrambleHelper<TCase, TContext> scrambleHelper;
        private readonly CubeManager cube;
        private readonly TimerManager timer;
        private readonly AlgorithmPlaybackManager playback;
        private readonly ModeManager mode;
        private readonly ImageHelper imageHelper;
        private readonly Func<TCase, string> getPreviewImage;
        private readonly Func<int> animationSpeed;
        private readonly Func<bool> instantMoves;
        private readonly string trainerName;
        private readonly string noContextMessage;
        private readonly string defaultHintText;
        private readonly string applyBeforeTimerText;
        private bool isApplying;

        public SlotScrambleManager(ScramblePanel panel, CaseSelection<TCase> caseSelection, SlotScrambleHelper<TCase, TContext> scrambleService, CubeManager cube, TimerManager timer, AlgorithmPlaybackManager playback, ModeManager mode, ImageHelper imageService, Func<TCase, string> getPreviewImage, Func<int> getAnimationSpeed, Func<bool> useInstantMoves, string trainerName, string noContextMessage, string defaultHintText, string applyBeforeTimerText)
        {
            this.panel = panel;
            this.caseSelection = caseSelection;
            this.scrambleHelper = scrambleService;
            this.cube = cube;
            this.timer = timer;
            this.playback = playback;
            this.mode = mode;
            this.imageHelper = imageService;
            this.getPreviewImage = getPreviewImage;
            this.animationSpeed = getAnimationSpeed;
            this.instantMoves = useInstantMoves;
            this.trainerName = trainerName;
            this.noContextMessage = noContextMessage;
            this.defaultHintText = defaultHintText;
            this.applyBeforeTimerText = applyBeforeTimerText;
        }

        public bool HasAppliedScramble => scrambleHelper.ScrambleApplied && scrambleHelper.AppliedScrambleMoves.Count > 0;

        public bool TryGetCurrentContext(out TContext context)
        {
            if (scrambleHelper.CurrentCase == null)
            {
                context = default!;
                return false;
            }

            context = scrambleHelper.CurrentContext!;
            return true;
        }

        public void RestoreScramble(Action? beforeReplay = null)
        {
            if (!HasAppliedScramble)
                return;

            cube.Reset();
            beforeReplay?.Invoke();

            foreach (ParsedMove move in scrambleHelper.AppliedScrambleMoves)
                cube.ApplyMove(move);

            scrambleHelper.ScrambledFastState = cube.CloneFastState();
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (!mode.IsTrainerMode)
                return;

            if (e.Key == Key.Right)
            {
                e.Handled = true;

                if (!e.IsRepeat)
                    Next();

                return;
            }

            if (e.Key == Key.Left)
            {
                e.Handled = true;

                if (!e.IsRepeat)
                    Previous();

                return;
            }

            if (IsTextInputFocused() || e.Key != Key.Space)
                return;

            e.Handled = true;
            timer.HandleSpaceDown();
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (!mode.IsTrainerMode || e.Key != Key.Space || IsTextInputFocused())
                return;

            e.Handled = true;
            bool wasRunning = timer.IsRunning;
            timer.HandleSpaceUp();

            if (wasRunning)
                scrambleHelper.ScrambleApplied = false;

            if (wasRunning && panel.HintText == applyBeforeTimerText)
                SetHintText(defaultHintText);
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBox or ComboBox;
        }

        public void Next()
        {
            if (scrambleHelper.CanMoveNextHistory)
            {
                LoadEntry(scrambleHelper.MoveNextHistory());
                return;
            }

            GenerateNext();
        }

        public void Previous()
        {
            if (!scrambleHelper.HasHistory)
            {
                SetScrambleText("No previous scramble yet.");
                return;
            }

            LoadEntry(scrambleHelper.MovePreviousHistory());
        }

        public void GenerateNext()
        {
            if (caseSelection.SelectedTrainerCases.Count == 0)
            {
                SetScrambleText($"Select at least one {trainerName} case first.");
                return;
            }

            StopTimer(recordSolve: false);

            List<TCase> pool = caseSelection.AllCases.Where(c => caseSelection.SelectedTrainerCases.Contains(c.Number)).ToList();
            SlotScrambleResult<TCase, TContext> result = scrambleHelper.GenerateNext(pool);

            if (!result.Found)
            {
                SetScrambleText(string.IsNullOrWhiteSpace(result.Message) ? noContextMessage : result.Message);
                SetHintText(defaultHintText);
                return;
            }

            LoadEntry(result.Entry);
        }

        public void LoadEntry(SlotScrambleEntry<TCase, TContext> entry)
        {
            StopTimer(recordSolve: false);
            playback.Stop();

            cube.Reset();
            scrambleHelper.LoadEntry(entry);

            SetScrambleText(entry.ScrambleText);
            SetHintText(defaultHintText);
            mode.SetTrainerImage(null);
        }

        public async Task ApplyAsync(bool animated)
        {
            if (scrambleHelper.CurrentScrambleMoves.Count == 0)
            {
                SetScrambleText("Generate a trainer scramble first.");
                return;
            }

            if (isApplying)
                return;

            try
            {
                isApplying = true;

                StopTimer(recordSolve: false);
                playback.Stop();
                cube.Reset();

                foreach (ParsedMove move in scrambleHelper.CurrentScrambleMoves)
                {
                    if (animated)
                        await cube.ApplyMoveAsync(move, instantMoves(), animationSpeed());
                    else
                        cube.ApplyMove(move);
                }

                scrambleHelper.ScrambledFastState = cube.CloneFastState();
                scrambleHelper.AppliedScrambleMoves = new List<ParsedMove>(scrambleHelper.CurrentScrambleMoves);
                scrambleHelper.ScrambleApplied = true;
                scrambleHelper.HintIndex = 0;
            }
            catch (OperationCanceledException) { }
            finally
            {
                isApplying = false;
            }
        }

        public async Task ResetAsync()
        {
            if (scrambleHelper.CurrentScrambleMoves.Count == 0)
            {
                SetScrambleText("Generate a trainer scramble first.");
                return;
            }

            StopTimer(recordSolve: false);
            await ApplyAsync(animated: false);
        }

        public void ShowHint()
        {
            if (scrambleHelper.CurrentCase == null)
            {
                SetHintText("Generate a trainer scramble first.");
                return;
            }

            ImageSource? image = imageHelper.LoadImage(getPreviewImage(scrambleHelper.CurrentCase));
            mode.SetTrainerImage(image);

            if (scrambleHelper.CurrentAlgorithmMoves.Count == 0)
            {
                SetHintText("Case image revealed.");
                return;
            }

            scrambleHelper.HintIndex = Math.Min(scrambleHelper.HintIndex + 1, scrambleHelper.CurrentAlgorithmMoves.Count);
            string revealedMoves = string.Join(" ", scrambleHelper.CurrentAlgorithmMoves.Take(scrambleHelper.HintIndex).Select(m => m.ToString()));
            SetHintText(string.IsNullOrWhiteSpace(revealedMoves) ? "Case image revealed." : $"Hint: {revealedMoves}");
        }

        public bool StopTimer(bool recordSolve)
        {
            bool stopped = timer.Stop(recordSolve);

            if (stopped && panel.HintText == applyBeforeTimerText)
                SetHintText(defaultHintText);

            return stopped;
        }

        private void SetScrambleText(string text)
        {
            panel.SetScrambleText(text);
        }

        private void SetHintText(string text)
        {
            panel.SetHintText(text);
        }
    }
}
