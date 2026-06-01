using MahApps.Metro.Controls;
using CubeForge.Trainers.Shared;
using CubeForge.Views;
using System;
using System.Diagnostics;
using System.Windows;

namespace CubeForge
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            TrainerSetup.LoadKociembaTables();

            ShowCrossTrainer();
        }

        private void CrossTrainer_Click(object sender, RoutedEventArgs e)
        {
            ShowCrossTrainer();
            DrawerHost.IsLeftDrawerOpen = false;
        }

        private void F2LTrainer_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "F2L Trainer";
            TrainerContentControl.Content = new F2LTrainerView();

            SetSelectedDrawerButton(AppPage.F2LTrainer);
            DrawerHost.IsLeftDrawerOpen = false;
        }

        private void OLLTrainer_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "OLL Trainer";
            TrainerContentControl.Content = new OLLTrainerView();

            SetSelectedDrawerButton(AppPage.OLLTrainer);
            DrawerHost.IsLeftDrawerOpen = false;
        }

        private void PLLTrainer_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "PLL Trainer";
            TrainerContentControl.Content = new PLLTrainerView();

            SetSelectedDrawerButton(AppPage.PLLTrainer);
            DrawerHost.IsLeftDrawerOpen = false;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Settings";

            // Placeholder until we make SettingsView.
            TrainerContentControl.Content = new PlaceholderView("Settings coming soon.");

            SetSelectedDrawerButton(AppPage.Settings);
            DrawerHost.IsLeftDrawerOpen = false;
        }

        private void ShowCrossTrainer()
        {
            PageTitleTextBlock.Text = "Cross / X-Cross Trainer";
            TrainerContentControl.Content = new CrossTrainerView();

            SetSelectedDrawerButton(AppPage.CrossTrainer);
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open GitHub.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSelectedDrawerButton(AppPage selectedPage)
        {
            Style selectedStyle = (Style)FindResource("PrimaryButtonStyle");
            Style normalStyle = (Style)FindResource("MaterialDesignOutlinedButton");

            CrossTrainerButton.Style = selectedPage == AppPage.CrossTrainer ? selectedStyle : normalStyle;
            F2LTrainerButton.Style = selectedPage == AppPage.F2LTrainer ? selectedStyle : normalStyle;
            OLLTrainerButton.Style = selectedPage == AppPage.OLLTrainer ? selectedStyle : normalStyle;
            PLLTrainerButton.Style = selectedPage == AppPage.PLLTrainer ? selectedStyle : normalStyle;
            SettingsButton.Style = selectedPage == AppPage.Settings ? selectedStyle : normalStyle;
        }

        private enum AppPage
        {
            CrossTrainer,
            F2LTrainer,
            OLLTrainer,
            PLLTrainer,
            Settings
        }
    }
}