using CubeForge.Trainers.Shared;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class OLLTrainerView : UserControl
    {
        private CubeManager cube = null!;
        private CubeInputManager cubeInput = null!;
        private AlgorithmTrainerManager<OllCase> trainer = null!;

        public OLLTrainerView()
        {
            InitializeComponent();

            CubeControlPanel.SetAvailableDisplayModes("Full Color", "OLL Trainer");
            CubeControlPanel.OrientationSettingsChanged += OnCubeSettingsChanged;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            cube = new CubeManager(ViewPort);
            cubeInput = new CubeInputManager(ViewPort);
            trainer = new AlgorithmTrainerManager<OllCase>(CreateOptions());
            DataContext = trainer.ViewModel;

            ApplySelectedOrientation();
            ApplySelectedMode();
            cubeInput.UpdateCamera();
            trainer.Init();
        }

        private AlgorithmTrainerOptions<OllCase> CreateOptions()
        {
            return new AlgorithmTrainerOptions<OllCase>
            {
                TrainerName = "OLL",
                JsonFileName = "oll_cases.json",
                JsonTitle = "OLL JSON",
                CaseHeader = "OLL Cases",
                AlgorithmDescription = "Browse OLL cases by group. Algorithm mode lets you study each case, step through moves, and see how the pieces change on the cube.",
                TrainerDescription = "Trainer mode gives you a selected pool of OLL cases, timed recognition practice, progressive hints, and session stats.",
                TargetRules = CaseTargetRules.Oll,
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
