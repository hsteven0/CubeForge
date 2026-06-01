using CubeForge.Cube;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CubeForge.Controls
{
    public partial class AlgorithmPlaybackPanel : UserControl
    {
        public event EventHandler? ResetRequested;
        public event EventHandler? ApplyAlgorithmRequested;
        public event EventHandler? PreviousMoveRequested;
        public event EventHandler? NextMoveRequested;

        public AlgorithmPlaybackPanel()
        {
            InitializeComponent();
        }

        public void SetCaseInfo(string title, string group)
        {
            AlgorithmCaseTitleTextBlock.Text = title;
            AlgorithmCaseGroupTextBlock.Text = group;
        }

        public void SetEmptyAlgorithmText(string text = "Algorithm will appear here.")
        {
            AlgorithmMovesTextBlock.Inlines.Clear();
            AlgorithmMovesTextBlock.Text = text;
        }

        public void SetAlgorithmDisplay(IReadOnlyList<ParsedMove> moves, int activeIndex = -1, int completedIndex = -1, bool completed = false)
        {
            AlgorithmMovesTextBlock.Inlines.Clear();

            if (moves.Count == 0)
            {
                AlgorithmMovesTextBlock.Text = "Algorithm will appear here.";
                return;
            }

            for (int i = 0; i < moves.Count; i++)
            {
                bool isGreen = completed || i == activeIndex || i == completedIndex;
                Brush color = isGreen ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : Brushes.White;

                AlgorithmMovesTextBlock.Inlines.Add(new Run(moves[i].ToString())
                {
                    Foreground = color,
                    FontWeight = isGreen ? FontWeights.Bold : FontWeights.Normal
                });

                if (i < moves.Count - 1)
                    AlgorithmMovesTextBlock.Inlines.Add(new Run(" "));
            }
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            ResetRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnApplyClicked(object sender, RoutedEventArgs e)
        {
            ApplyAlgorithmRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnPreviousMoveClick(object sender, RoutedEventArgs e)
        {
            PreviousMoveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnNextMoveClick(object sender, RoutedEventArgs e)
        {
            NextMoveRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
