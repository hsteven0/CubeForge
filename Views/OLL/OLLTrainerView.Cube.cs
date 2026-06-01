using CubeForge.Cube;
using CubeForge.Trainers.Shared;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class OLLTrainerView : UserControl
    {
        private void ApplySelectedOrientation()
        {
            if (cube == null || CubeControlPanel == null)
                return;

            try
            {
                CubeColor topColor = CubeControlPanel.TopColor;
                CubeColor frontColor = CubeControlPanel.FrontColor;
                CubeColor bottomColor = CubeColorHelper.GetOpposite(topColor);

                cube.SetOrientation(bottomColor, frontColor);
                ApplySelectedMode();

                CubeControlPanel.FrontColorEnabled = true;

                if (trainer?.Mode.IsTrainerMode == true)
                {
                    trainer.RestoreScramble(() =>
                    {
                        cube.SetOrientation(bottomColor, frontColor);
                        ApplySelectedMode();
                    });
                }
                else if (trainer?.CaseDisplay.SelectedCase != null && trainer.Playback != null && !trainer.Playback.IsBusy)
                {
                    trainer.Playback.Reset();
                }
            }
            catch (ArgumentException)
            {
                AlgorithmPlaybackPanel.SetEmptyAlgorithmText("Invalid orientation: front color cannot match or oppose top color.");
            }
        }

        private void ApplySelectedMode()
        {
            if (cube == null || CubeControlPanel == null)
                return;

            CubeColor topColor = CubeControlPanel.TopColor;
            CubeRenderMode renderMode = IsOllTrainerDisplayMode() ? CubeRenderMode.OllTrainer : CubeRenderMode.FullColor;

            cube.SetRenderMode(renderMode, topColor);
            CubeControlPanel.FrontColorEnabled = true;
        }

        private bool IsOllTrainerDisplayMode()
        {
            return CubeControlPanel != null && CubeControlPanel.DisplayMode == "OLL Trainer";
        }

        private void OnTrainerModeChanged(TrainerViewMode selectedMode)
        {
            ApplySelectedOrientation();
        }

        private void OnCubeSettingsChanged(object? sender, EventArgs e)
        {
            ApplySelectedOrientation();
        }

        private int GetAnimationSpeed()
        {
            return CubeControlPanel?.AnimationSpeed ?? 120;
        }

        private void OnCubeMouseDown(object sender, MouseButtonEventArgs e)
        {
            cubeInput.OnMouseDown(sender, e);
        }

        private void OnCubeMouseMove(object sender, MouseEventArgs e)
        {
            cubeInput.OnMouseMove(sender, e);
        }

        private void OnCubeMouseUp(object sender, MouseButtonEventArgs e)
        {
            cubeInput.OnMouseUp(sender, e);
        }

        private void OnCubeMouseWheel(object sender, MouseWheelEventArgs e)
        {
            cubeInput.OnMouseWheel(sender, e);
        }

        private void OnCubeTouchDown(object sender, TouchEventArgs e)
        {
            cubeInput.OnTouchDown(sender, e);
        }

        private void OnCubeTouchMove(object sender, TouchEventArgs e)
        {
            cubeInput.OnTouchMove(sender, e);
        }

        private void OnCubeTouchUp(object sender, TouchEventArgs e)
        {
            cubeInput.OnTouchUp(sender, e);
        }
    }
}
