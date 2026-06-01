using CubeForge.Cube;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class CrossTrainerView
    {
        private CubeColor GetBottomColor()
        {
            return CubeColorHelper.GetOpposite(CubeControlPanel.TopColor);
        }

        private void ApplySelectedOrientation()
        {
            if (cube == null || target == null)
                return;

            target.ApplyOrientation();
        }

        private void ApplySelectedDisplayMode()
        {
            if (cube == null || target == null)
                return;

            target.ApplyDisplayMode();
        }

        private bool UseInstantMoves()
        {
            return CubeControlPanel?.InstantMoves == true;
        }

        private int GetAnimationSpeed()
        {
            return CubeControlPanel?.AnimationSpeed ?? 120;
        }

        private void OnCubeSettingsChanged(object? sender, System.EventArgs e)
        {
            ApplySelectedOrientation();
            ApplySelectedDisplayMode();

            solution?.CurrentSolution.Clear();
            solution?.Hide();
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