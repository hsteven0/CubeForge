using CubeForge.Controls;
using CubeForge.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Trainers.Shared
{
    public sealed class AlgorithmTrainerOptions<TCase> where TCase : ICaseInfo
    {
        public string TrainerName { get; init; } = "";
        public string JsonFileName { get; init; } = "";
        public string JsonTitle { get; init; } = "";
        public string CaseHeader { get; init; } = "";
        public string AlgorithmDescription { get; init; } = "";
        public string TrainerDescription { get; init; } = "";
        public CaseTargetRules? TargetRules { get; init; }
        public CubeManager Cube { get; init; } = null!;
        public ModeToggle ModeToggle { get; init; } = null!;
        public CaseBrowser CaseBrowser { get; init; } = null!;
        public AlgorithmPlaybackPanel AlgorithmPlaybackPanel { get; init; } = null!;
        public ScramblePanel ScramblePanel { get; init; } = null!;
        public SessionPanel SessionPanel { get; init; } = null!;
        public FrameworkElement RightPaneCaseBrowser { get; init; } = null!;
        public FrameworkElement RightPaneTrainerStats { get; init; } = null!;
        public FrameworkElement AlgorithmCaseImageBorder { get; init; } = null!;
        public Image SelectedCaseImage { get; init; } = null!;
        public Func<TCase, string> GetSetupMoves { get; init; } = _ => "";
        public Func<TCase, string> GetDefaultAlgorithmMoves { get; init; } = _ => "";
        public Func<TCase, string> GetPreviewImage { get; init; } = _ => "";
        public Func<TCase, string> GetCaseName { get; init; } = _ => "";
        public Func<TCase, int> GetCaseNumber { get; init; } = _ => 0;
        public Func<int> AnimationSpeed { get; init; } = () => 120;
        public Func<bool> InstantMoves { get; init; } = () => false;
        public string DefaultHintText { get; init; } = "Hint hidden.";
        public string ApplyBeforeTimerText { get; init; } = "Apply the scramble before starting the timer.";

        public Action? AfterInit { get; init; }
        public Action? AfterLoaded { get; init; }
        public Action? AfterCasesLoaded { get; init; }
        public Action<TrainerViewMode>? AfterModeChanged { get; init; }
        public Action<TCase>? AfterCasePicked { get; init; }
        public Action<TCase>? AfterAlgorithmPicked { get; init; }
        public Action<TCase>? AfterScrambleChanged { get; init; }
        public Action<TCase>? AfterScrambleApplied { get; init; }
        public Action<TCase>? AfterScrambleReset { get; init; }
    }
}
