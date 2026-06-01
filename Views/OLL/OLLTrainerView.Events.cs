using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class OLLTrainerView : UserControl
    {
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            trainer.OnLoaded();

            Window? window = Window.GetWindow(this);

            if (window != null)
            {
                window.PreviewKeyDown += OnKeyDown;
                window.PreviewKeyUp += OnKeyUp;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Window? window = Window.GetWindow(this);

            if (window != null)
            {
                window.PreviewKeyDown -= OnKeyDown;
                window.PreviewKeyUp -= OnKeyUp;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            trainer.HandleKeyDown(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            trainer.HandleKeyUp(e);
        }

        private void OnScrollWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            double scrollAmount = e.Delta > 0 ? -42 : 42;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
            e.Handled = true;
        }
    }
}
