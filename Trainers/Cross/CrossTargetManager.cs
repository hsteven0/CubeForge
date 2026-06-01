using HelixToolkit.Wpf;
using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Trainers.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Trainers.Cross
{
    public sealed class CrossTargetManager
    {
        private readonly ComboBox trainingTypeComboBox;
        private readonly ComboBox solutionLengthComboBox;
        private readonly TextBlock targetSlotsHelpTextBlock;
        private readonly TextBlock targetSlot1Label;
        private readonly ComboBox targetSlot1ComboBox;
        private readonly TextBlock targetSlot2Label;
        private readonly ComboBox targetSlot2ComboBox;
        private readonly TextBlock targetSlot3Label;
        private readonly ComboBox targetSlot3ComboBox;
        private readonly Action refreshLayout;
        private readonly CubeManager cube;
        private readonly HelixViewport3D viewPort;
        private readonly TextBlock currentTargetSummaryTextBlock;
        private readonly Func<NCrossTarget?> getTarget;
        private readonly Func<string> getTrainingType;
        private readonly Func<CubeColor> getBottomColor;
        private readonly Func<CubeColor> getFrontColor;
        private readonly Func<string> getDisplayMode;
        private readonly Action<string> showError;
        private readonly Random random = new();

        public CrossTargetManager(ComboBox trainingTypeComboBox, ComboBox solutionLengthComboBox, TextBlock targetSlotsHelpTextBlock, TextBlock targetSlot1Label, ComboBox targetSlot1ComboBox, TextBlock targetSlot2Label, ComboBox targetSlot2ComboBox, TextBlock targetSlot3Label, ComboBox targetSlot3ComboBox, Action refreshLayout, CubeManager cube, HelixViewport3D viewPort, TextBlock currentTargetSummaryTextBlock, Func<NCrossTarget?> getResolvedTarget, Func<string> getResolvedTrainingType, Func<CubeColor> getBottomColor, Func<CubeColor> getFrontColor, Func<string> getDisplayMode, Action<string> showError)
        {
            this.trainingTypeComboBox = trainingTypeComboBox;
            this.solutionLengthComboBox = solutionLengthComboBox;
            this.targetSlotsHelpTextBlock = targetSlotsHelpTextBlock;
            this.targetSlot1Label = targetSlot1Label;
            this.targetSlot1ComboBox = targetSlot1ComboBox;
            this.targetSlot2Label = targetSlot2Label;
            this.targetSlot2ComboBox = targetSlot2ComboBox;
            this.targetSlot3Label = targetSlot3Label;
            this.targetSlot3ComboBox = targetSlot3ComboBox;
            this.refreshLayout = refreshLayout;
            this.cube = cube;
            this.viewPort = viewPort;
            this.currentTargetSummaryTextBlock = currentTargetSummaryTextBlock;
            this.getTarget = getResolvedTarget;
            this.getTrainingType = getResolvedTrainingType;
            this.getBottomColor = getBottomColor;
            this.getFrontColor = getFrontColor;
            this.getDisplayMode = getDisplayMode;
            this.showError = showError;
        }

        public void UpdateSlotVisibility()
        {
            string type = GetSelectedTrainingType();

            bool showSlot1 = type == "X-Cross" || type == "XX-Cross";
            bool showSlot2 = type == "XX-Cross";

            targetSlotsHelpTextBlock.Visibility = type == "Cross" ? Visibility.Collapsed : Visibility.Visible;

            targetSlot1Label.Visibility = showSlot1 ? Visibility.Visible : Visibility.Collapsed;
            targetSlot1ComboBox.Visibility = showSlot1 ? Visibility.Visible : Visibility.Collapsed;

            targetSlot2Label.Visibility = showSlot2 ? Visibility.Visible : Visibility.Collapsed;
            targetSlot2ComboBox.Visibility = showSlot2 ? Visibility.Visible : Visibility.Collapsed;

            targetSlot3Label.Visibility = Visibility.Collapsed;
            targetSlot3ComboBox.Visibility = Visibility.Collapsed;

            refreshLayout();
        }

        public string GetSelectedTrainingType()
        {
            return GetComboBoxText(trainingTypeComboBox, "X-Cross");
        }

        public CubeOrientation GetSelectedOrientation()
        {
            return new CubeOrientation(GetSelectedBottomColor(), GetSelectedFrontColor());
        }

        public CubeColor GetSelectedBottomColor()
        {
            return getBottomColor();
        }

        public CubeColor GetSelectedFrontColor()
        {
            return getFrontColor();
        }

        public bool IsCrossTrainerDisplayMode()
        {
            return getDisplayMode() == "Cross Trainer";
        }

        public int GetSelectedSolutionLength()
        {
            if (solutionLengthComboBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Content.ToString(), out int length))
                return length;

            return 8;
        }

        public NCrossTarget CreateTarget()
        {
            string type = GetSelectedTrainingType();
            CubeOrientation orientation = GetSelectedOrientation();

            return type switch
            {
                "Cross" => new NCrossTarget(orientation),
                "X-Cross" => new NCrossTarget(orientation, GetUniqueSlots(1).ToArray()),
                "XX-Cross" => new NCrossTarget(orientation, GetUniqueSlots(2).ToArray()),
                _ => new NCrossTarget(orientation)
            };
        }

        public IEnumerable<string> GetRenderSlots(NCrossTarget? target, string trainingType)
        {
            string selectedType = GetSelectedTrainingType();

            if (target != null && trainingType == selectedType)
                return target.Slots.Select(ToRenderSlotKey);

            return selectedType switch
            {
                "X-Cross" => GetVisibleSlots(1),
                "XX-Cross" => GetVisibleSlots(2),
                _ => Enumerable.Empty<string>()
            };
        }

        public void ApplyOrientation()
        {
            try
            {
                CubeColor bottom = GetSelectedBottomColor();
                CubeColor front = GetSelectedFrontColor();

                cube.SetOrientation(bottom, front);
            }
            catch (ArgumentException)
            {
                showError("Invalid orientation: front color cannot match or oppose bottom color.");
            }
        }

        public void ApplyDisplayMode()
        {
            CubeColor bottomColor = GetSelectedBottomColor();
            CubeColor topColor = CubeColorHelper.GetOpposite(bottomColor);

            CubeRenderMode mode = IsCrossTrainerDisplayMode() ? CubeRenderMode.CrossTrainer : CubeRenderMode.FullColor;

            CubeRenderFilter filter = CubeRenderFilter.ForMode(mode, topColor, GetRenderSlots(getTarget(), getTrainingType()));

            cube.Cube.SetRenderFilter(filter, viewPort);
        }

        public string GetTargetDisplayText()
        {
            NCrossTarget? target = getTarget();
            string trainingType = getTrainingType();

            if (target == null)
                return $"Target: {GetSelectedTrainingType()}";

            if (trainingType == "Cross")
                return "Target: Cross";

            return $"Target: {trainingType} - {string.Join(", ", target.Slots)}";
        }

        public void UpdatePracticePanel()
        {
            currentTargetSummaryTextBlock.Text = GetTargetDisplayText();
        }

        public static string GetComboBoxText(ComboBox comboBox, string fallback)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? fallback;
        }

        private List<F2LSlot> GetUniqueSlots(int count)
        {
            List<F2LSlot> available = new()
            {
                F2LSlot.FrontRight,
                F2LSlot.FrontLeft,
                F2LSlot.BackLeft,
                F2LSlot.BackRight
            };

            List<F2LSlot> chosen = new();
            ComboBox[] boxes = { targetSlot1ComboBox, targetSlot2ComboBox, targetSlot3ComboBox };

            for (int i = 0; i < count; i++)
            {
                string selected = GetComboBoxText(boxes[i], "Random");
                F2LSlot slot;

                if (selected == "Random")
                {
                    int index = random.Next(available.Count);
                    slot = available[index];
                }
                else
                {
                    slot = selected switch
                    {
                        "Front Right" => F2LSlot.FrontRight,
                        "Front Left" => F2LSlot.FrontLeft,
                        "Back Left" => F2LSlot.BackLeft,
                        "Back Right" => F2LSlot.BackRight,
                        _ => F2LSlot.BackRight
                    };

                    if (!available.Contains(slot))
                        throw new InvalidOperationException("Please choose different target slots.");
                }

                chosen.Add(slot);
                available.Remove(slot);
            }

            return chosen;
        }

        private IEnumerable<string> GetVisibleSlots(int count)
        {
            ComboBox[] boxes = { targetSlot1ComboBox, targetSlot2ComboBox, targetSlot3ComboBox };
            List<string> slots = new();

            for (int i = 0; i < count; i++)
            {
                string selected = GetComboBoxText(boxes[i], "Random");

                if (selected == "Random")
                    continue;

                string slotKey = selected switch
                {
                    "Front Right" => "FR",
                    "Front Left" => "FL",
                    "Back Left" => "BL",
                    "Back Right" => "BR",
                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(slotKey) && !slots.Contains(slotKey))
                    slots.Add(slotKey);
            }

            return slots;
        }

        private static string ToRenderSlotKey(F2LSlot slot)
        {
            return slot switch
            {
                F2LSlot.FrontRight => "FR",
                F2LSlot.FrontLeft => "FL",
                F2LSlot.BackLeft => "BL",
                F2LSlot.BackRight => "BR",
                _ => ""
            };
        }
    }
}