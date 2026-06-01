using System;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Controls
{
    public partial class ModeToggle : UserControl
    {
        public event EventHandler<ModeChangedArgs>? ModeChanged;

        public string SelectedMode { get; private set; } = "Algorithm";

        public ModeToggle()
        {
            InitializeComponent();
            SetSelectedMode("Algorithm", raiseEvent: false);
        }

        public void SetSelectedMode(string mode)
        {
            SetSelectedMode(mode, raiseEvent: false);
        }

        private void SetSelectedMode(string mode, bool raiseEvent)
        {
            if (mode != "Algorithm" && mode != "Trainer")
                mode = "Algorithm";

            SelectedMode = mode;

            AlgorithmModeSelectorButton.Opacity = mode == "Algorithm" ? 1.0 : 0.55;
            TrainerModeSelectorButton.Opacity = mode == "Trainer" ? 1.0 : 0.55;

            if (raiseEvent)
                ModeChanged?.Invoke(this, new ModeChangedArgs(mode));
        }

        private void OnAlgorithmModeClicked(object sender, RoutedEventArgs e)
        {
            SetSelectedMode("Algorithm", raiseEvent: true);
        }

        private void OnTrainerModeClicked(object sender, RoutedEventArgs e)
        {
            SetSelectedMode("Trainer", raiseEvent: true);
        }
    }

    public class ModeChangedArgs : EventArgs
    {
        public string Mode { get; }

        public ModeChangedArgs(string mode)
        {
            Mode = mode;
        }
    }
}