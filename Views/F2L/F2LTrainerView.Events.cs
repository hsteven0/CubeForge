using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class F2LTrainerView : UserControl
    {
        private Window? keyboardWindow;
        private bool keyboardEventsAttached;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(AttachKeyboardEvents));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachKeyboardEvents();
        }

        private void AttachKeyboardEvents()
        {
            if (keyboardEventsAttached)
                return;

            keyboardWindow = Window.GetWindow(this);

            if (keyboardWindow == null)
                return;

            keyboardWindow.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnKeyDown), true);
            keyboardWindow.AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp), true);
            keyboardEventsAttached = true;
        }

        private void DetachKeyboardEvents()
        {
            if (!keyboardEventsAttached || keyboardWindow == null)
                return;

            keyboardWindow.RemoveHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnKeyDown));
            keyboardWindow.RemoveHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp));
            keyboardEventsAttached = false;
            keyboardWindow = null;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            scramble.HandleKeyDown(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            scramble.HandleKeyUp(e);
        }

        private void OnScrollWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            // Trackpads can report very different delta magnitudes depending on gesture speed, so
            // use only the sign of the delta so each wheel event moves a constant amount
            double scrollAmount = e.Delta > 0 ? -42 : 42;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);

            e.Handled = true;
        }
    }
}