using CubeForge.Controls;
using CubeForge.Cube;
using CubeForge.Trainers.Shared;
using CubeForge.Trainers.Pairs;
using CubeForge.Trainers.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CubeForge.Views
{
    public partial class F2LTrainerView : UserControl
    {
        private CubeManager cube = null!;
        private CubeInputManager cubeInput = null!;

        private readonly CaseSelection<F2LCase> caseSelection = new();
        private readonly SlotCaseViewModel viewModel = new("f2l_cases.json", "algorithmsBySlot", PairSlot.FR.ToString());
        private F2LCase? selectedCase = null;
        private PairSlotManager pairSlots = null!;

        private AlgorithmPlaybackManager playback = null!;

        private readonly Dictionary<string, int> selectedAlgorithms = new();
        private ModeManager mode = null!;
        private SlotScrambleManager<F2LCase, PairSlot> scramble = null!;

        private const string DefaultTrainerHintText = "Hint hidden.";
        private const string ApplyBeforeTimerText = "ApplyScramble the scramble before starting the timer.";

        private SlotScrambleHelper<F2LCase, PairSlot> scrambleHelper = null!;
        private readonly ImageHelper imageHelper = new();
        private TimerManager trainerTimer = null!;

        public F2LTrainerView()
        {
            InitializeComponent();

            CubeControlPanel.SetAvailableDisplayModes("Full Color", "F2L Trainer");
            CubeControlPanel.OrientationSettingsChanged += OnCubeSettingsChanged;

            CaseBrowser.CaseSelected += OnCasePicked;
            CaseBrowser.AlgorithmSelected += OnAlgorithmPicked;
            CaseBrowser.KnownStateChanged += OnKnownChanged;
            CaseBrowser.SearchTextChanged += OnBrowserSearch;
            CaseBrowser.SlotChanged += OnSlotChanged;
            CaseBrowser.DataContext = viewModel;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            cube = new CubeManager(ViewPort);
            cubeInput = new CubeInputManager(ViewPort);
            pairSlots = new PairSlotManager(CaseBrowser, cube, ViewPort, OnPairSlotChanged, ApplySelectedMode, ResetSelectedCase);
            pairSlots.Init();
            ApplySelectedOrientation();
            ApplySelectedMode();
            cubeInput.UpdateCamera();

            playback = new AlgorithmPlaybackManager(cube, F2LAlgorithmPlaybackPanel, UseInstantMoves, GetAnimationSpeed);
            playback.AttachToPanel();

            mode = new ModeManager(
                ModeToggle, 
                CaseBrowser, 
                F2LAlgorithmPlaybackPanel, 
                TrainerPanel, 
                RightPaneCaseBrowser, 
                RightPaneTrainerStats, 
                AlgorithmCaseImageBorder, 
                SelectedCaseImage, 
                "F2L Cases", "Browse F2L cases by group. Algorithm mode lets you study each case, step through moves, and see how the pieces change on the cube.", "Trainer mode gives you a selected pool of F2L cases, timed recognition practice, progressive hints, and session stats.",
                UpdateCaseSelection
            );
            ModeToggle.ModeChanged += OnModeChanged;

            TrainerScramblePanel.PreviousScrambleRequested += OnPrevScramble;
            TrainerScramblePanel.NextScrambleRequested += OnNextScramble;
            TrainerScramblePanel.ApplyScrambleRequested += OnApplyScramble;
            TrainerScramblePanel.ResetScrambleRequested += OnTrainerReset;
            TrainerScramblePanel.HintRequested += OnHintClicked;
            TrainerScramblePanel.CaseSelectionChanged += OnCaseSelectChanged;
            TrainerScramblePanel.CaseSelectionOpeningRequested += OnCaseSelectOpen;

            F2LSessionPanel.SetHeader("F2L Trainer", "Track your F2L progress, averages, solve history, and session improvement over time.");
            trainerTimer = new TimerManager(F2LSessionPanel);
            trainerTimer.Init();

            scrambleHelper = new SlotScrambleHelper<F2LCase, PairSlot>(() => pairSlots.EnabledSlots, c => c.Name, GetAlgorithmForSlot, GetSetupForSlot, PairTargetRules.Create, (_, slot, setup) => PairTargetRules.BuildTargetState(slot, setup));
            scramble = new SlotScrambleManager<F2LCase, PairSlot>(TrainerScramblePanel, caseSelection, scrambleHelper, cube, trainerTimer, playback, mode, imageHelper, c => c.PreviewImage, GetAnimationSpeed, UseInstantMoves, "F2L", "Select at least one F2L slot first.", DefaultTrainerHintText, ApplyBeforeTimerText);

            LoadF2LCases();
            viewModel.LoadCases();
            viewModel.SelectedSetKey = pairSlots.SelectedSlot.ToString();

            if (viewModel.SelectedCase != null)
                DisplaySelectedCase(viewModel.SelectedCase);

            UpdateCaseSelection();
            pairSlots.AttachTrainerSlotCheckBoxes(TrainerSlotFrCheckBox, TrainerSlotFlCheckBox, TrainerSlotBlCheckBox, TrainerSlotBrCheckBox);

            mode.SetMode(TrainerViewMode.Algorithm);
        }
    }
}
