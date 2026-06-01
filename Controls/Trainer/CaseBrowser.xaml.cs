using CubeForge.Trainers.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CubeForge.Controls
{
    public partial class CaseBrowser : UserControl
    {
        public event EventHandler<CaseViewModel>? CaseSelected;
        public event EventHandler<AlgorithmViewModel>? AlgorithmSelected;
        public event EventHandler<CaseViewModel>? KnownStateChanged;
        public event EventHandler<string>? SearchTextChanged;
        public event EventHandler<string>? SlotChanged;

        private bool suppressSlotChanged;

        public CaseBrowser()
        {
            InitializeComponent();
        }

        public string SearchText
        {
            get => CaseSearchTextBox.Text;
            set
            {
                if (CaseSearchTextBox.Text == value)
                    return;

                CaseSearchTextBox.Text = value ?? "";
            }
        }

        public void SetSearchText(string text)
        {
            SearchText = text;
        }

        public void SetHeader(string header, string description = "")
        {
            CaseBrowserHeaderTextBlock.Text = header;

            if (string.IsNullOrWhiteSpace(description))
            {
                CaseBrowserDescriptionTextBlock.Visibility = Visibility.Collapsed;
                CaseBrowserDescriptionTextBlock.Text = "";
            }
            else
            {
                CaseBrowserDescriptionTextBlock.Visibility = Visibility.Visible;
                CaseBrowserDescriptionTextBlock.Text = description;
            }
        }

        public void SetHeaderVisible(bool isVisible)
        {
            CaseBrowserHeaderTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            CaseBrowserDescriptionTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowSlotSelector(bool show)
        {
            SlotSelectorBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SelectSlot(string slotKey)
        {
            suppressSlotChanged = true;

            UpdateSlotButtonVisual(SlotFrButton, slotKey == "FR");
            UpdateSlotButtonVisual(SlotFlButton, slotKey == "FL");
            UpdateSlotButtonVisual(SlotBlButton, slotKey == "BL");
            UpdateSlotButtonVisual(SlotBrButton, slotKey == "BR");

            suppressSlotChanged = false;
        }

        private static void UpdateSlotButtonVisual(Button button, bool selected)
        {
            button.Background = selected
                ? new SolidColorBrush(Color.FromRgb(110, 255, 0))
                : new SolidColorBrush(Color.FromRgb(20, 20, 20));

            button.BorderBrush = selected
                ? new SolidColorBrush(Color.FromRgb(110, 255, 0))
                : new SolidColorBrush(Color.FromRgb(42, 42, 42));

            button.Foreground = selected
                ? Brushes.Black
                : Brushes.White;

            button.FontWeight = selected
                ? FontWeights.Bold
                : FontWeights.Normal;
        }

        private void OnSlotClicked(object sender, RoutedEventArgs e)
        {
            if (suppressSlotChanged)
                return;

            if (sender is not Button button || button.Tag is not string slotKey)
                return;

            SelectSlot(slotKey);
            SlotChanged?.Invoke(this, slotKey);
        }

        private void OnSearchChanged(object sender, TextChangedEventArgs e)
        {
            SearchTextChanged?.Invoke(this, CaseSearchTextBox.Text);
        }

        private void OnCaseSelected(object sender, CaseViewModel trainerCase)
        {
            CaseSelected?.Invoke(this, trainerCase);
        }

        private void OnAlgorithmSelected(object sender, AlgorithmViewModel algorithm)
        {
            AlgorithmSelected?.Invoke(this, algorithm);
        }

        private void OnKnownChanged(object sender, CaseViewModel trainerCase)
        {
            KnownStateChanged?.Invoke(this, trainerCase);
        }

        private void OnCaseScroll(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            double scrollAmount = e.Delta > 0 ? -42 : 42;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
            e.Handled = true;
        }
    }
}
