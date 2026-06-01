using CubeForge.Cube;
using CubeForge.Trainers.Shared;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CubeForge.Views
{
    public partial class PLLTrainerView : UserControl
    {
        private Window? keyboardWindow;
        private bool keyboardEventsAttached;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            trainer.OnLoaded();
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
            trainer.HandleKeyDown(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            trainer.HandleKeyUp(e);
        }

        private void OnCaseSearchChanged(object sender, TextChangedEventArgs e)
        {
            trainer.OnSearchChanged();
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
