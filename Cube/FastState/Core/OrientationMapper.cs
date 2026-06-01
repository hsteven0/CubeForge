using System;
using CubeForge.Cube;

namespace CubeForge.Cube.FastState
{
    public static class OrientationMapper
    {
        public static Edge[] GetCrossEdges(CubeOrientation orientation)
        {
            return new[]
            {
                MapEdgeToBase(orientation, Edge.DF),
                MapEdgeToBase(orientation, Edge.DR),
                MapEdgeToBase(orientation, Edge.DB),
                MapEdgeToBase(orientation, Edge.DL)
            };
        }

        public static Edge MapEdgeToBase(CubeOrientation orientation, Edge displayedEdge)
        {
            GetEdgeFaces(displayedEdge, out Face a, out Face b);

            Face baseA = orientation.MapFaceToBase(a);
            Face baseB = orientation.MapFaceToBase(b);

            return EdgeFromFaces(baseA, baseB);
        }

        public static Corner MapCornerToBase(CubeOrientation orientation, Corner displayedCorner)
        {
            GetCornerFaces(displayedCorner, out Face a, out Face b, out Face c);

            Face baseA = orientation.MapFaceToBase(a);
            Face baseB = orientation.MapFaceToBase(b);
            Face baseC = orientation.MapFaceToBase(c);

            return CornerFromFaces(baseA, baseB, baseC);
        }

        public static F2LSlot MapSlotToBase(CubeOrientation orientation, F2LSlot displayedSlot)
        {
            GetSlotPieces(displayedSlot, out Corner displayedCorner, out Edge displayedEdge);

            Corner baseCorner = MapCornerToBase(orientation, displayedCorner);
            Edge baseEdge = MapEdgeToBase(orientation, displayedEdge);

            if (baseCorner == Corner.DFR && baseEdge == Edge.FR)
                return F2LSlot.FrontRight;

            if (baseCorner == Corner.DLF && baseEdge == Edge.FL)
                return F2LSlot.FrontLeft;

            if (baseCorner == Corner.DBL && baseEdge == Edge.BL)
                return F2LSlot.BackLeft;

            if (baseCorner == Corner.DRB && baseEdge == Edge.BR)
                return F2LSlot.BackRight;

            throw new InvalidOperationException($"Displayed slot {displayedSlot} does not map to a canonical D-layer F2L slot.");
        }

        public static void GetMappedSlotPieces(CubeOrientation orientation, F2LSlot displayedSlot, out Corner baseCorner, out Edge baseEdge)
        {
            GetSlotPieces(displayedSlot, out Corner displayedCorner, out Edge displayedEdge);

            baseCorner = MapCornerToBase(orientation, displayedCorner);
            baseEdge = MapEdgeToBase(orientation, displayedEdge);
        }

        public static void GetSlotPieces(F2LSlot slot, out Corner corner, out Edge edge)
        {
            switch (slot)
            {
                case F2LSlot.FrontRight:
                    corner = Corner.DFR;
                    edge = Edge.FR;
                    break;

                case F2LSlot.FrontLeft:
                    corner = Corner.DLF;
                    edge = Edge.FL;
                    break;

                case F2LSlot.BackLeft:
                    corner = Corner.DBL;
                    edge = Edge.BL;
                    break;

                case F2LSlot.BackRight:
                    corner = Corner.DRB;
                    edge = Edge.BR;
                    break;

                default:
                    corner = Corner.DFR;
                    edge = Edge.FR;
                    break;
            }
        }

        private static void GetEdgeFaces(Edge edge, out Face a, out Face b)
        {
            switch (edge)
            {
                case Edge.UR: a = Face.Up; b = Face.Right; break;
                case Edge.UF: a = Face.Up; b = Face.Front; break;
                case Edge.UL: a = Face.Up; b = Face.Left; break;
                case Edge.UB: a = Face.Up; b = Face.Back; break;

                // Important: side face first for D-layer edges
                case Edge.DR: a = Face.Right; b = Face.Down; break;
                case Edge.DF: a = Face.Front; b = Face.Down; break;
                case Edge.DL: a = Face.Left; b = Face.Down; break;
                case Edge.DB: a = Face.Back; b = Face.Down; break;

                case Edge.FR: a = Face.Front; b = Face.Right; break;
                case Edge.FL: a = Face.Front; b = Face.Left; break;
                case Edge.BL: a = Face.Back; b = Face.Left; break;
                case Edge.BR: a = Face.Back; b = Face.Right; break;
                default: a = Face.Up; b = Face.Right; break;
            }
        }

        private static void GetCornerFaces(Corner corner, out Face a, out Face b, out Face c)
        {
            switch (corner)
            {
                case Corner.URF: a = Face.Up; b = Face.Right; c = Face.Front; break;
                case Corner.UFL: a = Face.Up; b = Face.Front; c = Face.Left; break;
                case Corner.ULB: a = Face.Up; b = Face.Left; c = Face.Back; break;
                case Corner.UBR: a = Face.Up; b = Face.Back; c = Face.Right; break;
                case Corner.DFR: a = Face.Down; b = Face.Front; c = Face.Right; break;
                case Corner.DLF: a = Face.Down; b = Face.Left; c = Face.Front; break;
                case Corner.DBL: a = Face.Down; b = Face.Back; c = Face.Left; break;
                case Corner.DRB: a = Face.Down; b = Face.Right; c = Face.Back; break;
                default: a = Face.Up; b = Face.Right; c = Face.Front; break;
            }
        }

        private static Edge EdgeFromFaces(Face a, Face b)
        {
            bool Has(Face face) => a == face || b == face;

            if (Has(Face.Up) && Has(Face.Right)) return Edge.UR;
            if (Has(Face.Up) && Has(Face.Front)) return Edge.UF;
            if (Has(Face.Up) && Has(Face.Left)) return Edge.UL;
            if (Has(Face.Up) && Has(Face.Back)) return Edge.UB;

            if (Has(Face.Down) && Has(Face.Right)) return Edge.DR;
            if (Has(Face.Down) && Has(Face.Front)) return Edge.DF;
            if (Has(Face.Down) && Has(Face.Left)) return Edge.DL;
            if (Has(Face.Down) && Has(Face.Back)) return Edge.DB;

            if (Has(Face.Front) && Has(Face.Right)) return Edge.FR;
            if (Has(Face.Front) && Has(Face.Left)) return Edge.FL;
            if (Has(Face.Back) && Has(Face.Left)) return Edge.BL;
            if (Has(Face.Back) && Has(Face.Right)) return Edge.BR;

            throw new InvalidOperationException($"Invalid edge faces: {a}, {b}");
        }

        private static Corner CornerFromFaces(Face a, Face b, Face c)
        {
            bool Has(Face face) => a == face || b == face || c == face;

            if (Has(Face.Up) && Has(Face.Right) && Has(Face.Front)) return Corner.URF;
            if (Has(Face.Up) && Has(Face.Front) && Has(Face.Left)) return Corner.UFL;
            if (Has(Face.Up) && Has(Face.Left) && Has(Face.Back)) return Corner.ULB;
            if (Has(Face.Up) && Has(Face.Back) && Has(Face.Right)) return Corner.UBR;

            if (Has(Face.Down) && Has(Face.Front) && Has(Face.Right)) return Corner.DFR;
            if (Has(Face.Down) && Has(Face.Left) && Has(Face.Front)) return Corner.DLF;
            if (Has(Face.Down) && Has(Face.Back) && Has(Face.Left)) return Corner.DBL;
            if (Has(Face.Down) && Has(Face.Right) && Has(Face.Back)) return Corner.DRB;

            throw new InvalidOperationException($"Invalid corner faces: {a}, {b}, {c}");
        }
    }
}
