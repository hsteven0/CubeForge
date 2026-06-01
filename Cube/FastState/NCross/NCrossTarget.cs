using System;
using CubeForge.Cube;

namespace CubeForge.Cube.FastState
{
    public class NCrossTarget
    {
        private static readonly CubeOrientation DefaultOrientation = new CubeOrientation(CubeColor.Yellow, CubeColor.Red);

        public List<F2LSlot> Slots { get; }
        public CubeOrientation Orientation { get; }

        public NCrossTarget(params F2LSlot[] slots)
        {
            Orientation = DefaultOrientation;
            Slots = CleanSlots(slots);
        }

        public NCrossTarget(CubeOrientation orientation, params F2LSlot[] slots)
        {
            Orientation = orientation;
            Slots = CleanSlots(slots);
        }

        public Edge[] GetCrossEdges()
        {
            return OrientationMapper.GetCrossEdges(Orientation);
        }

        public void GetSlotPieces(F2LSlot slot, out Corner corner, out Edge edge)
        {
            OrientationMapper.GetMappedSlotPieces(Orientation, slot, out corner, out edge);
        }

        private static List<F2LSlot> CleanSlots(F2LSlot[] slots)
        {
            List<F2LSlot> cleanSlots = slots.Distinct().ToList();

            return cleanSlots;
        }
    }
}