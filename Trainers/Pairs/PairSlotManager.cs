using CubeForge.Controls;
using CubeForge.Cube;
using CubeForge.Trainers.Shared;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CubeForge.Trainers.Pairs
{
    public sealed class PairSlotManager
    {
        private readonly CaseBrowser caseBrowser;
        private readonly CubeManager cube;
        private readonly HelixToolkit.Wpf.HelixViewport3D viewport;
        private readonly Action<string> onSlotChanged;
        private readonly Action setDisplayMode;
        private readonly Action resetCase;
        private readonly HashSet<PairSlot> enabledSlots = new()
        {
            PairSlot.FR,
            PairSlot.FL,
            PairSlot.BL,
            PairSlot.BR
        };

        private CheckBox? frCheckBox;
        private CheckBox? flCheckBox;
        private CheckBox? blCheckBox;
        private CheckBox? brCheckBox;

        public PairSlotManager(CaseBrowser caseBrowser, CubeManager cube, HelixToolkit.Wpf.HelixViewport3D viewport, Action<string> afterSlotChanged, Action applyDisplayMode, Action resetCase)
        {
            this.caseBrowser = caseBrowser;
            this.cube = cube;
            this.viewport = viewport;
            this.onSlotChanged = afterSlotChanged;
            this.setDisplayMode = applyDisplayMode;
            this.resetCase = resetCase;
        }

        public PairSlot SelectedSlot { get; private set; } = PairSlot.FR;
        public IReadOnlyCollection<PairSlot> EnabledSlots => enabledSlots;

        public void Init()
        {
            caseBrowser.ShowSlotSelector(true);
            caseBrowser.SelectSlot(SelectedSlot.ToString());
            ApplySlotToCube();
        }

        public void AttachTrainerSlotCheckBoxes(CheckBox fr, CheckBox fl, CheckBox bl, CheckBox br)
        {
            frCheckBox = fr;
            flCheckBox = fl;
            blCheckBox = bl;
            brCheckBox = br;

            fr.Checked += OnSlotToggled;
            fr.Unchecked += OnSlotToggled;
            fl.Checked += OnSlotToggled;
            fl.Unchecked += OnSlotToggled;
            bl.Checked += OnSlotToggled;
            bl.Unchecked += OnSlotToggled;
            br.Checked += OnSlotToggled;
            br.Unchecked += OnSlotToggled;

            UpdateCheckboxes();
        }

        public bool SelectSlot(string slotKey)
        {
            if (!Enum.TryParse(slotKey, out PairSlot slot))
                return false;

            if (SelectedSlot == slot)
            {
                caseBrowser.SelectSlot(slotKey);
                return true;
            }

            SelectedSlot = slot;
            caseBrowser.SelectSlot(slotKey);
            ApplySlotToCube();
            onSlotChanged(slotKey);
            setDisplayMode();
            resetCase();
            return true;
        }

        public string GetSetupForSelectedSlot(string setupMoves)
        {
            return GetSetupForSlot(setupMoves, SelectedSlot);
        }

        public static string GetSetupForSlot(string setupMoves, PairSlot slot)
        {
            int yTurns = slot switch
            {
                PairSlot.FR => 0,
                PairSlot.FL => 3,
                PairSlot.BL => 2,
                PairSlot.BR => 1,
                _ => 0
            };

            if (yTurns == 0)
                return setupMoves;

            string prefix = new ParsedMove(FaceMove.Y, yTurns).ToString();
            string suffix = new ParsedMove(FaceMove.Y, 4 - yTurns).ToString();

            return string.IsNullOrWhiteSpace(setupMoves)
                ? $"{prefix} {suffix}"
                : $"{prefix} {setupMoves} {suffix}";
        }

        private void ApplySlotToCube()
        {
            cube.Cube.SetTrainerPairSlot(SelectedSlot.ToString(), viewport);
        }

        private void OnSlotToggled(object sender, EventArgs e)
        {
            if (sender is not CheckBox checkBox || checkBox.Tag is not string slotText)
                return;

            if (!Enum.TryParse(slotText, out PairSlot slot))
                return;

            if (checkBox.IsChecked != true && enabledSlots.Count == 1 && enabledSlots.Contains(slot))
            {
                checkBox.IsChecked = true;

                MessageBox.Show(
                    "At least one pair slot must remain enabled.",
                    "Invalid Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (checkBox.IsChecked == true)
                enabledSlots.Add(slot);
            else
                enabledSlots.Remove(slot);

            UpdateCheckboxes();
        }

        private void UpdateCheckboxes()
        {
            if (frCheckBox != null)
                frCheckBox.IsChecked = enabledSlots.Contains(PairSlot.FR);

            if (flCheckBox != null)
                flCheckBox.IsChecked = enabledSlots.Contains(PairSlot.FL);

            if (blCheckBox != null)
                blCheckBox.IsChecked = enabledSlots.Contains(PairSlot.BL);

            if (brCheckBox != null)
                brCheckBox.IsChecked = enabledSlots.Contains(PairSlot.BR);
        }
    }
}
