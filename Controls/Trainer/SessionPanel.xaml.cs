using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CubeForge.Controls
{
    public partial class SessionPanel : UserControl
    {
        public event EventHandler? SelectedSessionChanged;
        public event EventHandler? CreateSessionRequested;
        public event EventHandler? ConfirmRenameSessionRequested;
        public event EventHandler? ConfirmDeleteSessionRequested;

        public SessionPanel()
        {
            InitializeComponent();
        }

        public string SelectedSessionName => (SessionSelectorComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        public string RenameInputText => SessionRenameInputTextBox.Text.Trim();
        public int SessionCount => SessionSelectorComboBox.Items.Count;

        public void SetHeader(string header, string description)
        {
            TrainerPanelHeaderTextBlock.Text = header;
            TrainerPanelDescriptionTextBlock.Text = description;
        }

        public void AddSessionName(string name)
        {
            SessionSelectorComboBox.Items.Add(new ComboBoxItem
            {
                Content = name
            });
        }

        public void SelectLastSession()
        {
            if (SessionSelectorComboBox.Items.Count == 0)
                return;

            SessionSelectorComboBox.SelectedIndex = SessionSelectorComboBox.Items.Count - 1;
        }

        public void SelectFirstSession()
        {
            if (SessionSelectorComboBox.Items.Count == 0)
                return;

            SessionSelectorComboBox.SelectedIndex = 0;
        }

        public void RenameSelectedSession(string newName)
        {
            if (SessionSelectorComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            selectedItem.Content = newName;
        }

        public void RemoveSelectedSession()
        {
            if (SessionSelectorComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            SessionSelectorComboBox.Items.Remove(selectedItem);
        }

        public void CloseRenamePopup()
        {
            SessionRenamePopup.IsOpen = false;
        }

        public void CloseDeletePopup()
        {
            SessionDeletePopup.IsOpen = false;
        }

        public void SetTimerText(string text)
        {
            TrainerTimerValueTextBlock.Text = text;
        }

        public void SetTimerColor(Brush foreground)
        {
            TrainerTimerValueTextBlock.Foreground = foreground;
        }

        public void SetTimerDelta(string text)
        {
            TimerDeltaTextBlock.Text = text;
        }

        public void SetTimerDelta(string text, Brush foreground)
        {
            TimerDeltaTextBlock.Text = text;
            TimerDeltaTextBlock.Foreground = foreground;
        }

        public void SetTimerStatus(string text, Brush foreground)
        {
            TimerStatusTextBlock.Text = text;
            TimerStatusTextBlock.Foreground = foreground;
        }

        public void ResetTimerDisplay()
        {
            SetTimerText("0.00");
            SetTimerColor(Brushes.White);
            SetTimerDelta("");
            SetTimerStatus("Hold Space to arm timer", new SolidColorBrush(Color.FromRgb(156, 163, 175)));
        }

        public void SetStats(
            string currentTime,
            string bestTime,
            string currentMo3,
            string bestMo3,
            string currentAo3,
            string bestAo3,
            string currentAo5,
            string bestAo5,
            string currentAo12,
            string bestAo12,
            string summary)
        {
            CurrentTimeValueTextBlock.Text = currentTime;
            BestTimeValueTextBlock.Text = bestTime;
            CurrentMo3ValueTextBlock.Text = currentMo3;
            BestMo3ValueTextBlock.Text = bestMo3;
            CurrentAo3ValueTextBlock.Text = currentAo3;
            BestAo3ValueTextBlock.Text = bestAo3;
            CurrentAo5ValueTextBlock.Text = currentAo5;
            BestAo5ValueTextBlock.Text = bestAo5;
            CurrentAo12ValueTextBlock.Text = currentAo12;
            BestAo12ValueTextBlock.Text = bestAo12;
            SolveHistorySummaryValueTextBlock.Text = summary;
            TimerAveragesTextBlock.Text = $"ao5: {currentAo5}    ao12: {currentAo12}";
        }

        public void SetSolveHistory(IEnumerable itemsSource)
        {
            SolveHistoryDataGrid.ItemsSource = itemsSource;
        }

        private void OnSessionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionSelectorComboBox.SelectedItem == null)
                return;

            SelectedSessionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnCreateSession(object sender, RoutedEventArgs e)
        {
            CreateSessionRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnRenameSession(object sender, RoutedEventArgs e)
        {
            if (SessionSelectorComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SessionRenameInputTextBox.Text = selectedItem.Content?.ToString() ?? "";
                SessionRenameInputTextBox.SelectAll();
            }

            SessionRenamePopup.IsOpen = true;
            SessionRenameInputTextBox.Focus();
        }

        private void OnDeleteSession(object sender, RoutedEventArgs e)
        {
            if (SessionSelectorComboBox.Items.Count <= 1)
                return;

            SessionDeletePopup.IsOpen = true;
        }

        private void OnRenameCancel(object sender, RoutedEventArgs e)
        {
            CloseRenamePopup();
        }

        private void OnRenameConfirm(object sender, RoutedEventArgs e)
        {
            ConfirmRenameSessionRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeleteCancel(object sender, RoutedEventArgs e)
        {
            CloseDeletePopup();
        }

        private void OnDeleteConfirm(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteSessionRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
