using CubeForge.Trainers.Shared;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class PLLTrainerView : UserControl
    {
        private CubeManager cube = null!;
        private CubeInputManager cubeInput = null!;
        private AlgorithmTrainerManager<PllCase> trainer = null!;

        public PLLTrainerView()
        {
            InitializeComponent();

            CubeControlPanel.SetAvailableDisplayModes("Full Color", "PLL Trainer");
            CubeControlPanel.OrientationSettingsChanged += OnCubeSettingsChanged;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            cube = new CubeManager(ViewPort);
            cubeInput = new CubeInputManager(ViewPort);
            trainer = new AlgorithmTrainerManager<PllCase>(CreateOptions());
            DataContext = trainer.ViewModel;

            ApplySelectedOrientation();
            ApplySelectedMode();
            cubeInput.UpdateCamera();
            trainer.Init();
        }

        private AlgorithmTrainerOptions<PllCase> CreateOptions()
        {
            return new AlgorithmTrainerOptions<PllCase>
            {
                TrainerName = "PLL",
                JsonFileName = "pll_cases.json",
                JsonTitle = "PLL JSON",
                CaseHeader = "PLL Cases",
                AlgorithmDescription = "Study PLL cases by group, learn algorithms, and watch how each permutation affects the last layer pieces.",
                TrainerDescription = "Practice recognizing and solving PLL cases with generated scrambles, timed solves, and focused last layer training.",
                TargetRules = CaseTargetRules.Pll,
                Cube = cube,
                ModeToggle = ModeToggle,
                CaseBrowser = CaseBrowser,
                AlgorithmPlaybackPanel = AlgorithmPlaybackPanel,
                ScramblePanel = ScramblePanel,
                SessionPanel = SessionPanel,
                RightPaneCaseBrowser = RightPaneCaseBrowser,
                RightPaneTrainerStats = RightPaneTrainerStats,
                AlgorithmCaseImageBorder = AlgorithmCaseImageBorder,
                SelectedCaseImage = SelectedCaseImage,
                GetSetupMoves = c => c.SetupMoves,
                GetDefaultAlgorithmMoves = c => c.Algorithms.Count > 0 ? c.Algorithms[0].Moves : "",
                GetPreviewImage = c => c.PreviewImage,
                GetCaseName = c => c.Name,
                GetCaseNumber = c => c.Number,
                AnimationSpeed = GetAnimationSpeed,
                InstantMoves = () => CubeControlPanel.InstantMoves,
                AfterModeChanged = OnTrainerModeChanged
            };
        }
    }
}
