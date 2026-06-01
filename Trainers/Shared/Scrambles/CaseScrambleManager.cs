using CubeForge.Controls;
using CubeForge.Cube;
using CubeForge.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CubeForge.Trainers.Shared
{
    public class CaseScrambleManager<TCase> where TCase : ICaseInfo
    {
        private readonly ScramblePanel panel;
        private readonly CaseSelection<TCase> caseSelection;
        private readonly CaseScrambleHelper<TCase> scrambleHelper;
        private readonly CubeManager cube;
        private readonly TimerManager timer;
        private readonly AlgorithmPlaybackManager playback;
        private readonly ModeManager mode;
        private readonly ImageHelper imageHelper;
        private readonly Func<TCase, string> getPreviewImage;
        private readonly Func<int> animationSpeed;
        private readonly Func<bool> instantMoves;
        private readonly string trainerName;
        private readonly string defaultHintText;
        private readonly string applyBeforeTimerText;
        private bool isApplying;

        public CaseScrambleManager(ScramblePanel panel, CaseSelection<TCase> caseSelection, CaseScrambleHelper<TCase> scrambleService, CubeManager cube, TimerManager timer, AlgorithmPlaybackManager playback, ModeManager mode, ImageHelper imageService, Func<TCase, string> getPreviewImage, Func<int> getAnimationSpeed, Func<bool> useInstantMoves, string trainerName, string defaultHintText, string applyBeforeTimerText)
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
            this.defaultHintText = defaultHintText;
            this.applyBeforeTimerText = applyBeforeTimerText;
        }

        public bool HasAppliedScramble => scrambleHelper.ScrambleApplied && scrambleHelper.AppliedScrambleMoves.Count > 0;

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
                panel.SetScrambleText("No previous scrambleManager yet.");
                return;
            }

            LoadEntry(scrambleHelper.MovePreviousHistory());
        }

        public void GenerateNext()
        {
            if (caseSelection.SelectedTrainerCases.Count == 0)
            {
                panel.SetScrambleText($"Select at least one {trainerName} case first.");
                return;
            }

            StopTimer(recordSolve: false);

            List<TCase> pool = caseSelection.AllCases.Where(c => caseSelection.SelectedTrainerCases.Contains(c.Number)).ToList();
            CaseScrambleResult<TCase> result = scrambleHelper.GenerateNext(pool);

            if (!result.Found)
            {
                panel.SetScrambleText(result.Message);
                panel.SetHintText(defaultHintText);
                return;
            }

            LoadEntry(result.Entry);
        }

        public void LoadEntry(ScrambleEntry<TCase> entry)
        {
            StopTimer(recordSolve: false);
            playback.Stop();

            cube.Reset();
            scrambleHelper.LoadEntry(entry);

            panel.SetScrambleText(entry.ScrambleText);
            panel.SetHintText(defaultHintText);
            mode.SetTrainerImage(null);
        }

        public async Task ApplyScramble(bool animated)
        {
            if (scrambleHelper.CurrentScrambleMoves.Count == 0)
            {
                panel.SetScrambleText("Generate a trainer scramble first.");
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
            catch (OperationCanceledException) {}
            finally
            {
                isApplying = false;
            }
        }

        public async Task Reset()
        {
            if (scrambleHelper.CurrentScrambleMoves.Count == 0)
            {
                panel.SetScrambleText("Generate a trainer scrambleManager first.");
                return;
            }

            StopTimer(recordSolve: false);
            await ApplyScramble(animated: false);
        }

        public void ToggleTimer()
        {
            if (timer.IsRunning)
            {
                StopTimer(recordSolve: true);
                scrambleHelper.ScrambleApplied = false;
                return;
            }

            if (scrambleHelper.CurrentScrambleMoves.Count == 0)
            {
                panel.SetScrambleText("Generate a trainer scrambleManager first.");
                return;
            }

            if (!scrambleHelper.ScrambleApplied)
                scrambleHelper.ScrambleApplied = true;

            timer.Toggle();
        }

        public void ShowHint()
        {
            if (scrambleHelper.CurrentCase == null)
            {
                panel.SetHintText("Generate a trainer scrambleManager first.");
                return;
            }

            ImageSource? image = imageHelper.LoadImage(getPreviewImage(scrambleHelper.CurrentCase));
            mode.SetTrainerImage(image);

            if (scrambleHelper.CurrentAlgorithmMoves.Count == 0)
            {
                panel.SetHintText("Case image revealed.");
                return;
            }

            scrambleHelper.HintIndex = Math.Min(scrambleHelper.HintIndex + 1, scrambleHelper.CurrentAlgorithmMoves.Count);

            string revealedMoves = string.Join(" ", scrambleHelper.CurrentAlgorithmMoves.Take(scrambleHelper.HintIndex).Select(m => m.ToString()));
            panel.SetHintText(string.IsNullOrWhiteSpace(revealedMoves) ? "Case image revealed." : $"Hint: {revealedMoves}");
        }

        public bool StopTimer(bool recordSolve)
        {
            bool stopped = timer.Stop(recordSolve);

            if (stopped && panel.HintText == applyBeforeTimerText)
                panel.SetHintText(defaultHintText);

            return stopped;
        }
    }
}
