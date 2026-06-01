using CubeForge.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CubeForge.Trainers.Shared
{
    public enum TrainerViewMode
    {
        Algorithm,
        Trainer
    }

    public sealed class ModeManager
    {
        private readonly ModeToggle modeToggle;
        private readonly CaseBrowser caseBrowser;
        private readonly FrameworkElement algorithmPanel;
        private readonly FrameworkElement trainerPanel;
        private readonly FrameworkElement browserPane;
        private readonly FrameworkElement statsPane;
        private readonly FrameworkElement imageBorder;
        private readonly Image selectedCaseImage;
        private readonly string caseHeader;
        private readonly string algorithmDescription;
        private readonly string trainerDescription;
        private readonly Action showTrainerMode;

        private ImageSource? algorithmImage;
        private ImageSource? trainerImage;

        public TrainerViewMode CurrentMode { get; private set; } = TrainerViewMode.Algorithm;
        public bool IsTrainerMode => CurrentMode == TrainerViewMode.Trainer;

        public ModeManager(ModeToggle modeToggle, CaseBrowser caseBrowser, FrameworkElement algorithmPanel, FrameworkElement trainerPanel, FrameworkElement browserPane, FrameworkElement statsPane, FrameworkElement imageBorder, Image selectedCaseImage, string caseHeader, string algorithmDescription, string trainerDescription, Action onTrainerModeShown)
        {
            this.modeToggle = modeToggle;
            this.caseBrowser = caseBrowser;
            this.algorithmPanel = algorithmPanel;
            this.trainerPanel = trainerPanel;
            this.browserPane = browserPane;
            this.statsPane = statsPane;
            this.imageBorder = imageBorder;
            this.selectedCaseImage = selectedCaseImage;
            this.caseHeader = caseHeader;
            this.algorithmDescription = algorithmDescription;
            this.trainerDescription = trainerDescription;
            this.showTrainerMode = onTrainerModeShown;
        }

        public void SetMode(TrainerViewMode mode)
        {
            CurrentMode = mode;
            bool algorithmMode = mode == TrainerViewMode.Algorithm;

            algorithmPanel.Visibility = algorithmMode ? Visibility.Visible : Visibility.Collapsed;
            trainerPanel.Visibility = algorithmMode ? Visibility.Collapsed : Visibility.Visible;
            browserPane.Visibility = algorithmMode ? Visibility.Visible : Visibility.Collapsed;
            statsPane.Visibility = algorithmMode ? Visibility.Collapsed : Visibility.Visible; 
            imageBorder.Visibility = Visibility.Visible;

            selectedCaseImage.Source = algorithmMode ? algorithmImage : trainerImage;
            modeToggle.SetSelectedMode(algorithmMode ? "Algorithm" : "Trainer");
            caseBrowser.SetHeader(caseHeader, algorithmMode ? algorithmDescription : trainerDescription);

            if (mode == TrainerViewMode.Trainer)
                showTrainerMode();
        }

        public void SetAlgorithmImage(ImageSource? image)
        {
            algorithmImage = image;

            if (CurrentMode == TrainerViewMode.Algorithm)
                selectedCaseImage.Source = algorithmImage;
        }

        public void SetTrainerImage(ImageSource? image)
        {
            trainerImage = image;

            if (CurrentMode == TrainerViewMode.Trainer)
                selectedCaseImage.Source = trainerImage;
        }
    }
}
