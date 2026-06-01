using CubeForge.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CubeForge.Trainers.Shared
{
    public sealed class CasePoolManager<TCase> where TCase : ICaseInfo
    {
        private readonly CaseSelection<TCase> caseSelection;
        private readonly Button selectedCasesButton;
        private readonly Popup selectionPopup;
        private readonly TextBlock summaryTextBlock;
        private readonly Button editSelectionButton;
        private readonly Border manualSelectionPanel;
        private readonly StackPanel manualCasesPanel;
        private readonly string trainerName;
        private readonly Func<TCase, string> getName;
        private readonly Func<TCase, string> getGroup;

        public CasePoolManager(CaseSelection<TCase> caseSelection, Button selectedCasesButton, Popup selectionPopup, TextBlock summaryTextBlock, Button editSelectionButton, Border manualSelectionPanel, StackPanel manualCasesPanel, string trainerName, Func<TCase, string> getName, Func<TCase, string> getGroup)
        {
            this.caseSelection = caseSelection;
            this.selectedCasesButton = selectedCasesButton;
            this.selectionPopup = selectionPopup;
            this.summaryTextBlock = summaryTextBlock;
            this.editSelectionButton = editSelectionButton;
            this.manualSelectionPanel = manualSelectionPanel;
            this.manualCasesPanel = manualCasesPanel;
            this.trainerName = trainerName;
            this.getName = getName;
            this.getGroup = getGroup;
        }

        public void OnCaseChanged(object sender)
        {
            if (sender is not CheckBox checkBox || checkBox.Tag is not int caseNumber)
                return;

            if (checkBox.IsChecked == true)
                caseSelection.SelectedTrainerCases.Add(caseNumber);
            else
                caseSelection.SelectedTrainerCases.Remove(caseNumber);

            UpdateSelectedCountText();
        }

        public void OnGroupButton(object sender)
        {
            if (sender is not Button button || button.Tag is not string groupName)
                return;

            bool shouldSelect = button.Content?.ToString() == "Select";

            foreach (TCase trainerCase in caseSelection.AllCases.Where(c => getGroup(c) == groupName))
            {
                if (shouldSelect)
                    caseSelection.SelectedTrainerCases.Add(trainerCase.Number);
                else
                    caseSelection.SelectedTrainerCases.Remove(trainerCase.Number);
            }

            RefreshSelections();
        }

        public void UpdateSelectedCountText()
        {
            int count = caseSelection.SelectedTrainerCases.Count;

            selectedCasesButton.Content = count switch
            {
                0 => "No cases selected",
                1 => "1 case selected",
                _ => $"{count} cases selected"
            };

            summaryTextBlock.Text = count switch
            {
                0 => "No cases are currently selected.",
                1 => $"1 {trainerName} case can appear in trainer mode.",
                _ => $"{count} {trainerName} cases can appear in trainer mode."
            };
        }

        public void BuildCaseList()
        {
            manualCasesPanel.Children.Clear();

            foreach (var group in caseSelection.AllCases.GroupBy(getGroup))
            {
                var groupPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                var headerRow = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };

                var title = new TextBlock
                {
                    Text = group.Key,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var groupButtons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                groupButtons.Children.Add(CreateGroupButton("Select", group.Key));
                groupButtons.Children.Add(CreateGroupButton("Clear", group.Key));

                DockPanel.SetDock(groupButtons, Dock.Right);
                headerRow.Children.Add(groupButtons);
                headerRow.Children.Add(title);

                groupPanel.Children.Add(headerRow);

                foreach (TCase trainerCase in group.OrderBy(c => c.Number))
                    groupPanel.Children.Add(CreateCaseCheckBox(trainerCase));

                manualCasesPanel.Children.Add(groupPanel);
            }

            UpdateSelectedCountText();
        }

        private void RefreshSelections()
        {
            UpdateSelectedCountText();

            if (manualSelectionPanel.Visibility == Visibility.Visible)
                BuildCaseList();
        }

        private Button CreateGroupButton(string text, string groupName)
        {
            var button = new Button
            {
                Content = text,
                Tag = groupName,
                FontSize = 11,
                Padding = new Thickness(7, 3, 7, 3),
                Margin = new Thickness(4, 0, 0, 0)
            };

            button.Click += (sender, _) => OnGroupButton(sender!);
            return button;
        }

        private CheckBox CreateCaseCheckBox(TCase trainerCase)
        {
            var checkBox = new CheckBox
            {
                Content = $"{trainerCase.Number}. {getName(trainerCase)}",
                Tag = trainerCase.Number,
                IsChecked = caseSelection.SelectedTrainerCases.Contains(trainerCase.Number),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 2, 0, 2)
            };

            checkBox.Checked += (sender, _) => OnCaseChanged(sender!);
            checkBox.Unchecked += (sender, _) => OnCaseChanged(sender!);

            return checkBox;
        }
    }
}
