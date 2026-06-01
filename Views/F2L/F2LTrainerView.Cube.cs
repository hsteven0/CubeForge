using CubeForge.Cube;
using CubeForge.Trainers.Pairs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class F2LTrainerView : UserControl
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

                if (mode?.IsTrainerMode == true)
                {
                    scramble?.RestoreScramble(() =>
                    {
                        cube.SetOrientation(bottomColor, frontColor);
                        ApplySelectedMode();
                    });
                }
                else if (selectedCase != null && !playback.IsBusy)
                {
                    ResetSelectedCase();
                }
            }
            catch (ArgumentException)
            {
                F2LAlgorithmPlaybackPanel.SetEmptyAlgorithmText("Invalid orientation: front color cannot match or oppose top color.");
            }
        }

        private void ApplySelectedMode()
        {
            if (cube == null || CubeControlPanel == null)
                return;

            CubeColor topColor = CubeControlPanel.TopColor;
            CubeRenderMode renderMode = IsF2LTrainerDisplayMode() ? CubeRenderMode.F2LTrainer : CubeRenderMode.FullColor;

            PairSlot renderSlot = pairSlots.SelectedSlot;

            if (this.mode?.IsTrainerMode == true && scramble != null && scramble.TryGetCurrentContext(out PairSlot trainerSlot))
                renderSlot = trainerSlot;

            cube.Cube.SetTrainerPairSlot(renderSlot.ToString());
            cube.SetRenderMode(renderMode, topColor);

            CubeControlPanel.FrontColorEnabled = true;
        }

        private bool IsF2LTrainerDisplayMode()
        {
            return CubeControlPanel != null && CubeControlPanel.DisplayMode == "F2L Trainer";
        }

        private void OnCubeSettingsChanged(object? sender, EventArgs e)
        {
            ApplySelectedOrientation();
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

        private bool UseInstantMoves()
        {
            return CubeControlPanel?.InstantMoves == true;
        }

        private int GetAnimationSpeed()
        {
            return CubeControlPanel?.AnimationSpeed ?? 250;
        }
    }
}
