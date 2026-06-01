using CubeForge.Trainers.Common;
using CubeForge.Trainers.Shared;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CubeForge.Controls
{
    public partial class CaseCard : UserControl
    {
        public event EventHandler<CaseViewModel>? CaseSelected;
        public event EventHandler<AlgorithmViewModel>? AlgorithmSelected;
        public event EventHandler<CaseViewModel>? KnownStateChanged;

        private CaseViewModel? attachedCase;

        public CaseCard()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachToCurrentViewModel();
            RefreshVisualState();
            LoadPreviewImage();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachFromCurrentViewModel();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DetachFromCurrentViewModel();
            AttachToCurrentViewModel();
            RefreshVisualState();
            LoadPreviewImage();
        }

        private void AttachToCurrentViewModel()
        {
            if (DataContext is not CaseViewModel trainerCase)
                return;

            if (attachedCase == trainerCase)
                return;

            attachedCase = trainerCase;
            attachedCase.PropertyChanged += OnCaseChanged;
        }

        private void DetachFromCurrentViewModel()
        {
            if (attachedCase != null)
                attachedCase.PropertyChanged -= OnCaseChanged;

            attachedCase = null;
        }

        private void OnCaseChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CaseViewModel.IsSelected))
                RefreshVisualState();
        }

        private void OnCardClicked(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not CaseViewModel trainerCase)
                return;

            CaseSelected?.Invoke(this, trainerCase);
        }

        private void OnAlgorithmHeaderClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            AlgorithmOptionsPopup.IsOpen = !AlgorithmOptionsPopup.IsOpen;
            AlgorithmArrowTextBlock.Text = AlgorithmOptionsPopup.IsOpen ? "▴" : "▾";
        }

        private void OnAlgMenuClosed(object sender, EventArgs e)
        {
            AlgorithmArrowTextBlock.Text = "▾";
        }

        private void OnAlgOptionClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (DataContext is not CaseViewModel trainerCase)
                return;

            if (sender is not Button button)
                return;

            if (button.DataContext is not AlgorithmViewModel algorithm)
                return;

            trainerCase.SelectAlgorithm(algorithm);
            AlgorithmOptionsPopup.IsOpen = false;

            AlgorithmSelected?.Invoke(this, algorithm);
        }

        private void OnKnownClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (DataContext is not CaseViewModel trainerCase)
                return;

            KnownStateChanged?.Invoke(this, trainerCase);
        }

        private void RefreshVisualState()
        {
            if (DataContext is not CaseViewModel trainerCase)
                return;

            if (trainerCase.IsSelected)
            {
                RootBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                RootBorder.Background = new SolidColorBrush(Color.FromRgb(18, 18, 18));
            }
            else
            {
                RootBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                RootBorder.Background = new SolidColorBrush(Color.FromRgb(18, 18, 18));
            }
        }

        private void LoadPreviewImage()
        {
            if (DataContext is not CaseViewModel trainerCase)
                return;

            PreviewImage.Source = new ImageHelper().LoadImage(trainerCase.PreviewImage);
        }
    }
}
