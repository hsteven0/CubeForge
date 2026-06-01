using CubeForge.Controls;
using CubeForge.Trainers.Common;
using CubeForge.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Trainers.Shared
{
    public sealed class AlgorithmTrainerManager<TCase> where TCase : ICaseInfo
    {
        private readonly AlgorithmTrainerOptions<TCase> options;
        private readonly CaseSelection<TCase> caseSelection = new();
        private readonly ImageHelper imageHelper = new();
        private readonly CaseScrambleHelper<TCase> scrambleHelper;

        private readonly AlgorithmCaseViewModel viewModel;
        private readonly TimerManager timer;
        private readonly AlgorithmPlaybackManager playback;
        private readonly ModeManager mode;
        private readonly CaseDisplayManager<TCase> caseDisplay;
        private readonly CaseBrowserManager<TCase> caseBrowserManager;
        private readonly CaseScrambleManager<TCase> scrambleManager;

        public AlgorithmCaseViewModel ViewModel => viewModel;
        public ModeManager Mode => mode;
        public TimerManager Timer => timer;
        public AlgorithmPlaybackManager Playback => playback;
        public CaseDisplayManager<TCase> CaseDisplay => caseDisplay;

        public AlgorithmTrainerManager(AlgorithmTrainerOptions<TCase> options)
        {
            this.options = options;
            viewModel = new AlgorithmCaseViewModel(options.JsonFileName, options.JsonTitle);
            scrambleHelper = new CaseScrambleHelper<TCase>(options.TargetRules, options.GetCaseName, options.GetCaseNumber, options.GetSetupMoves, options.GetDefaultAlgorithmMoves);
            timer = new TimerManager(options.SessionPanel);
            playback = new AlgorithmPlaybackManager(options.Cube, options.AlgorithmPlaybackPanel, options.InstantMoves, options.AnimationSpeed);
            caseBrowserManager = new CaseBrowserManager<TCase>(caseSelection, viewModel, options.ScramblePanel, options.CaseBrowser, options.TrainerName, options.JsonFileName, OnCasePicked, OnAlgorithmPicked);
            mode = new ModeManager(options.ModeToggle, options.CaseBrowser, options.AlgorithmPlaybackPanel, options.ScramblePanel, options.RightPaneCaseBrowser, options.RightPaneTrainerStats, options.AlgorithmCaseImageBorder, options.SelectedCaseImage, options.CaseHeader, options.AlgorithmDescription, options.TrainerDescription, caseBrowserManager.LoadPopup);
            caseDisplay = new CaseDisplayManager<TCase>(caseSelection, imageHelper, playback, mode, options.GetSetupMoves, options.GetDefaultAlgorithmMoves);
            scrambleManager = new CaseScrambleManager<TCase>(options.ScramblePanel, caseSelection, scrambleHelper, options.Cube, timer, playback, mode, imageHelper, options.GetPreviewImage, options.AnimationSpeed, options.InstantMoves, options.TrainerName, options.DefaultHintText, options.ApplyBeforeTimerText);
        }

        public void Init()
        {
            options.ModeToggle.ModeChanged += OnModeChanged;
            options.AlgorithmPlaybackPanel.ResetRequested += OnResetAlgorithm;
            options.AlgorithmPlaybackPanel.ApplyAlgorithmRequested += OnApplyAlgorithm;
            options.AlgorithmPlaybackPanel.PreviousMoveRequested += OnPreviousMove;
            options.AlgorithmPlaybackPanel.NextMoveRequested += OnNextMove;
            options.ScramblePanel.PreviousScrambleRequested += OnPreviousScramble;
            options.ScramblePanel.NextScrambleRequested += OnNextScramble;
            options.ScramblePanel.ApplyScrambleRequested += OnApplyScramble;
            options.ScramblePanel.ResetScrambleRequested += OnResetScramble;
            options.ScramblePanel.HintRequested += OnHintRequested;
            options.SessionPanel.SetHeader($"{options.TrainerName} Trainer", options.TrainerDescription);

            timer.Init();
            caseBrowserManager.Attach();
            caseBrowserManager.LoadCases();
            caseBrowserManager.UpdateSelectedCount();
            options.AfterCasesLoaded?.Invoke();
            mode.SetMode(TrainerViewMode.Algorithm);
            options.AfterModeChanged?.Invoke(TrainerViewMode.Algorithm);
            options.AfterInit?.Invoke();
        }

        public void OnLoaded()
        {
            caseBrowserManager.LoadPopup();

            if (viewModel.SelectedCase != null && caseDisplay.SelectedCase == null)
            {
                caseDisplay.ShowCase(viewModel.SelectedCase);
                InvokeCaseHook(options.AfterCasePicked);
            }

            options.AfterLoaded?.Invoke();
        }

        public void OnSearchChanged()
        {
            caseBrowserManager.SetSearchFromBrowser();
        }

        public void SetBrowserSearch(string searchText)
        {
            caseBrowserManager.SetSearchText(searchText);
        }

        public void ClearStats()
        {
            timer.ClearStats();
        }

        public void NextScramble()
        {
            scrambleManager.Next();
            InvokeScrambleHook(options.AfterScrambleChanged);
        }

        public void PreviousScramble()
        {
            scrambleManager.Previous();
            InvokeScrambleHook(options.AfterScrambleChanged);
        }

        public async void ApplyScramble()
        {
            await scrambleManager.ApplyScramble(animated: true);
            InvokeScrambleHook(options.AfterScrambleApplied);
        }

        public async void ResetScramble()
        {
            await scrambleManager.Reset();
            InvokeScrambleHook(options.AfterScrambleReset);
        }

        public void ShowHint()
        {
            scrambleManager.ShowHint();
        }

        public void RestoreScramble(Action? beforeReplay = null)
        {
            playback.Stop();
            scrambleManager.RestoreScramble(beforeReplay);
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (!mode.IsTrainerMode)
                return;

            if (e.Key == Key.Right)
            {
                e.Handled = true;

                if (!e.IsRepeat)
                    NextScramble();

                return;
            }

            if (e.Key == Key.Left)
            {
                e.Handled = true;

                if (!e.IsRepeat)
                    PreviousScramble();

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

            if (wasRunning && options.ScramblePanel.HintText == options.ApplyBeforeTimerText)
                options.ScramblePanel.SetHintText(options.DefaultHintText);
        }

        private void OnModeChanged(object? sender, ModeChangedArgs e)
        {
            playback.Stop();

            TrainerViewMode selectedMode = e.Mode == "Trainer" ? TrainerViewMode.Trainer : TrainerViewMode.Algorithm;
            mode.SetMode(selectedMode);
            options.AfterModeChanged?.Invoke(selectedMode);
        }

        private void OnCasePicked(CaseViewModel trainerCase)
        {
            viewModel.SelectCase(trainerCase);

            if (caseDisplay.ShowCase(trainerCase))
                InvokeCaseHook(options.AfterCasePicked);
        }

        private void OnAlgorithmPicked(AlgorithmViewModel algorithm)
        {
            if (viewModel.SelectedCase == null)
                return;

            playback.SetAlgorithm(algorithm.Moves);

            if (caseDisplay.ShowCase(viewModel.SelectedCase))
                InvokeCaseHook(options.AfterAlgorithmPicked);
        }

        private void OnResetAlgorithm(object? sender, EventArgs e)
        {
            playback.Reset();
        }

        private async void OnApplyAlgorithm(object? sender, EventArgs e)
        {
            await playback.ApplyAllMoves();
        }

        private async void OnPreviousMove(object? sender, EventArgs e)
        {
            await playback.PreviousMove();
        }

        private async void OnNextMove(object? sender, EventArgs e)
        {
            await playback.NextMove();
        }

        private void OnPreviousScramble(object? sender, EventArgs e)
        {
            PreviousScramble();
        }

        private void OnNextScramble(object? sender, EventArgs e)
        {
            NextScramble();
        }

        private void OnApplyScramble(object? sender, EventArgs e)
        {
            ApplyScramble();
        }

        private void OnResetScramble(object? sender, EventArgs e)
        {
            ResetScramble();
        }

        private void OnHintRequested(object? sender, EventArgs e)
        {
            ShowHint();
        }

        private void InvokeCaseHook(Action<TCase>? hook)
        {
            if (hook == null || caseDisplay.SelectedCase == null)
                return;

            hook(caseDisplay.SelectedCase);
        }

        private void InvokeScrambleHook(Action<TCase>? hook)
        {
            if (hook == null || scrambleHelper.CurrentCase == null)
                return;

            hook(scrambleHelper.CurrentCase);
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBox or ComboBox;
        }
    }
}
