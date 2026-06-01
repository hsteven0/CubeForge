using CubeForge.Cube.FastState;
using CubeForge.Trainers.Shared;
using CubeForge.Trainers.Cross;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class CrossTrainerView : UserControl
    {
        private CubeManager cube = null!;
        private CubeInputManager cubeInput = null!;

        private string currentScramble = "";
        private CrossSolutionManager solution = null!;

        private NCrossTarget? currentTarget = null;
        private string currentTrainingType = "";

        private CrossTargetManager target = null!;
        private CrossScrambleManager scramble = null!;
        private List<SearchMove> guaranteedSolution = new();

        public CrossTrainerView()
        {
            InitializeComponent();
            CubeControlPanel.SetAvailableDisplayModes("Full Color", "Cross Trainer");
            CubeControlPanel.OrientationSettingsChanged += OnCubeSettingsChanged;

            cube = new CubeManager(ViewPort);
            cubeInput = new CubeInputManager(ViewPort);

            target = new CrossTargetManager(
                TrainingTypeComboBox,
                SolutionLengthComboBox,
                TargetSlotsHelpTextBlock,
                TargetSlot1Label,
                TargetSlot1ComboBox,
                TargetSlot2Label,
                TargetSlot2ComboBox,
                TargetSlot3Label,
                TargetSlot3ComboBox,
                RefreshTrainerLayouts,
                cube,
                ViewPort,
                CurrentTargetSummaryTextBlock,
                () => currentTarget,
                () => currentTrainingType,
                GetBottomColor,
                () => CubeControlPanel.FrontColor,
                () => CubeControlPanel.DisplayMode,
                text => solution?.ShowMessage(text)
            );

            scramble = new CrossScrambleManager(
                cube,
                target,
                GenerationModeComboBox,
                DifficultyComboBox,
                ViewPort,
                UseInstantMoves,
                GetAnimationSpeed,
                ApplySelectedOrientation,
                ApplySelectedDisplayMode
            );

            solution = new CrossSolutionManager(SolutionTextBlock, HintStageTextBlock, HintStatusTextBlock, RevealSolutionButton, ApplySolutionButton, PreviousSolutionButton, NextSolutionButton, target.GetTargetDisplayText, RefreshTrainerLayouts, cube, ViewPort, GetAnimationSpeed, ApplySelectedOrientation, ApplySelectedDisplayMode);

            WireUiEvents();

            Loaded += OnLoaded;
        }
    }
}
