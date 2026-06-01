using CubeForge.Cube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Controls
{
    public partial class CubeControlPanel : UserControl
    {
        public event EventHandler? SettingsChanged;

        public event EventHandler? OrientationSettingsChanged;
        public event EventHandler? PlaybackSettingsChanged;

        private readonly CubeColor[] AllColors =
        {
            CubeColor.White,
            CubeColor.Yellow,
            CubeColor.Red,
            CubeColor.Orange,
            CubeColor.Blue,
            CubeColor.Green
        };

        private bool isUpdatingColorOptions;

        public CubeControlPanel()
        {
            InitializeComponent();
            RefreshColorOptions(CubeColor.White, CubeColor.Red);
        }

        public CubeColor SelectedTopColor => GetSelectedColor(TopColorSelectorComboBox, CubeColor.White);
        public CubeColor SelectedFrontColor => GetSelectedColor(FrontColorSelectorComboBox, CubeColor.Red);
        public string SelectedDisplayMode => GetComboBoxText(DisplayModeSelectorComboBox, "Full Color");
        public bool InstantMoves => InstantMoveToggleCheckBox.IsChecked == true;

        public CubeColor TopColor
        {
            get => SelectedTopColor;
            set => SetTopColor(value);
        }

        public CubeColor FrontColor
        {
            get => SelectedFrontColor;
            set => SetFrontColor(value);
        }

        public string DisplayMode
        {
            get => SelectedDisplayMode;
            set => SetDisplayMode(value);
        }

        public bool FrontColorEnabled
        {
            get => FrontColorSelectorComboBox.IsEnabled;
            set => SetFrontColorEnabled(value);
        }

        public int AnimationSpeed
        {
            get
            {
                double min = MoveAnimationSpeedSlider.Minimum;
                double max = MoveAnimationSpeedSlider.Maximum;
                double value = MoveAnimationSpeedSlider.Value;

                return (int)(max + min - value);
            }
        }

        public void SetAvailableDisplayModes(params string[] displayModes)
        {
            string currentMode = SelectedDisplayMode;

            DisplayModeSelectorComboBox.Items.Clear();

            foreach (string displayMode in displayModes)
            {
                DisplayModeSelectorComboBox.Items.Add(new ComboBoxItem
                {
                    Content = displayMode
                });
            }

            if (displayModes.Contains(currentMode))
            {
                SetDisplayMode(currentMode);
                return;
            }

            if (displayModes.Length > 0)
                DisplayModeSelectorComboBox.SelectedIndex = 0;
        }

        public void SetDisplayMode(string displayMode)
        {
            SelectComboBoxItem(DisplayModeSelectorComboBox, displayMode);
        }

        public void SetTopColor(CubeColor color)
        {
            CubeColor frontColor = SelectedFrontColor;

            if (!IsValidOrientation(color, frontColor))
                frontColor = GetFirstValidColor(color);

            RefreshColorOptions(color, frontColor);
        }

        public void SetFrontColor(CubeColor color)
        {
            CubeColor topColor = SelectedTopColor;

            if (!IsValidOrientation(topColor, color))
                topColor = GetFirstValidColor(color);

            RefreshColorOptions(topColor, color);
        }

        public void SetFrontColorEnabled(bool isEnabled)
        {
            FrontColorSelectorComboBox.IsEnabled = isEnabled;
            FrontColorSelectorComboBox.Opacity = isEnabled ? 1.0 : 0.55;
        }

        public void SetInstantMoves(bool instantMoves)
        {
            InstantMoveToggleCheckBox.IsChecked = instantMoves;
        }

        public void SetAnimationSliderValue(double value)
        {
            MoveAnimationSpeedSlider.Value = Math.Clamp(value, MoveAnimationSpeedSlider.Minimum, MoveAnimationSpeedSlider.Maximum);
        }

        private void OnCubeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingColorOptions)
                return;

            if (TopColorSelectorComboBox == null || FrontColorSelectorComboBox == null || DisplayModeSelectorComboBox == null)
                return;

            CubeColor topColor = SelectedTopColor;
            CubeColor frontColor = SelectedFrontColor;

            if (sender == TopColorSelectorComboBox)
            {
                if (!IsValidOrientation(topColor, frontColor))
                    frontColor = GetFirstValidColor(topColor);

                RefreshColorOptions(topColor, frontColor);
            }
            else if (sender == FrontColorSelectorComboBox)
            {
                if (!IsValidOrientation(topColor, frontColor))
                    topColor = GetFirstValidColor(frontColor);

                RefreshColorOptions(topColor, frontColor);
            }

            OrientationSettingsChanged?.Invoke(this, EventArgs.Empty);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PlaybackSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnInstantChanged(object sender, RoutedEventArgs e)
        {
            PlaybackSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshColorOptions(CubeColor selectedTopColor, CubeColor selectedFrontColor)
        {
            if (!IsValidOrientation(selectedTopColor, selectedFrontColor))
            {
                selectedFrontColor = GetFirstValidColor(selectedTopColor);
            }

            List<CubeColor> validTopColors = AllColors.Where(color => IsValidOrientation(color, selectedFrontColor)).ToList();

            List<CubeColor> validFrontColors = AllColors.Where(color => IsValidOrientation(selectedTopColor, color)).ToList();

            if (!validTopColors.Contains(selectedTopColor))
                selectedTopColor = validTopColors.First();

            if (!validFrontColors.Contains(selectedFrontColor))
                selectedFrontColor = validFrontColors.First();

            isUpdatingColorOptions = true;

            try
            {
                TopColorSelectorComboBox.Items.Clear();

                foreach (CubeColor color in validTopColors)
                {
                    TopColorSelectorComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = color.ToString()
                    });
                }

                FrontColorSelectorComboBox.Items.Clear();

                foreach (CubeColor color in validFrontColors)
                {
                    FrontColorSelectorComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = color.ToString()
                    });
                }

                SelectComboBoxItem(TopColorSelectorComboBox, selectedTopColor.ToString());
                SelectComboBoxItem(FrontColorSelectorComboBox, selectedFrontColor.ToString());
            }
            finally
            {
                isUpdatingColorOptions = false;
            }
        }

        private static bool IsValidOrientation(CubeColor topColor, CubeColor frontColor)
        {
            if (topColor == frontColor)
                return false;

            if (CubeColorHelper.GetOpposite(topColor) == frontColor)
                return false;

            return true;
        }

        private CubeColor GetFirstValidColor(CubeColor fixedColor)
        {
            return AllColors.First(color => IsValidOrientation(fixedColor, color));
        }

        private static CubeColor GetSelectedColor(ComboBox comboBox, CubeColor fallback)
        {
            string color = GetComboBoxText(comboBox, fallback.ToString());

            return color switch
            {
                "White" => CubeColor.White,
                "Yellow" => CubeColor.Yellow,
                "Red" => CubeColor.Red,
                "Orange" => CubeColor.Orange,
                "Blue" => CubeColor.Blue,
                "Green" => CubeColor.Green,
                _ => fallback
            };
        }

        private static string GetComboBoxText(ComboBox comboBox, string fallback)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? fallback;
        }

        private static void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (object item in comboBox.Items)
            {
                if (item is not ComboBoxItem comboBoxItem)
                    continue;

                string text = comboBoxItem.Content?.ToString() ?? "";

                if (text != value)
                    continue;

                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }
}