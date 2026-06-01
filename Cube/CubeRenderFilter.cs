using System.Collections.Generic;
using System.Linq;

namespace CubeForge.Cube
{
    public class CubeRenderFilter
    {
        public CubeRenderMode Mode { get; set; } = CubeRenderMode.FullColor;
        public CubeColor FocusColor { get; set; } = CubeColor.White;
        public bool ShowCenters { get; set; }
        public bool ShowCrossEdges { get; set; }
        public bool HideFocusColorStickers { get; set; }

        public HashSet<string> VisibleF2LSlots { get; } = new();

        public static CubeRenderFilter ForMode(CubeRenderMode mode, CubeColor focusColor, IEnumerable<string>? visibleSlots = null)
        {
            CubeRenderFilter filter = new()
            {
                Mode = mode,
                FocusColor = focusColor
            };

            switch (mode)
            {
                case CubeRenderMode.F2LTrainer:
                    filter.ShowCenters = true;
                    filter.ShowCrossEdges = true;
                    filter.HideFocusColorStickers = true;
                    break;

                case CubeRenderMode.CrossTrainer:
                    filter.ShowCenters = true;
                    filter.ShowCrossEdges = true;
                    filter.HideFocusColorStickers = true;
                    break;
            }

            filter.SetVisibleSlots(visibleSlots ?? Enumerable.Empty<string>());

            return filter;
        }

        public void SetVisibleSlots(IEnumerable<string> slots)
        {
            VisibleF2LSlots.Clear();

            foreach (string slot in slots)
            {
                string normalized = NormalizeSlotKey(slot);

                if (!string.IsNullOrWhiteSpace(normalized))
                    VisibleF2LSlots.Add(normalized);
            }
        }

        public static string NormalizeSlotKey(string slot)
        {
            return slot switch
            {
                "FR" or "FrontRight" => "FR",
                "FL" or "FrontLeft" => "FL",
                "BL" or "BackLeft" => "BL",
                "BR" or "BackRight" => "BR",
                _ => ""
            };
        }
    }
}
