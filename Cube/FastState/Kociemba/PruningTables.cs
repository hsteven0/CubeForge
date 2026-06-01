using CubeForge.Trainers.Shared;
using System;
using System.IO;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class PruningTables
    {
        private const byte Unvisited = 255;

        private static byte[] phase1CornerEdgeTable = Array.Empty<byte>();
        private static byte[] phase1CornerSliceTable = Array.Empty<byte>();
        private static byte[] phase1EdgeSliceTable = Array.Empty<byte>();

        private static byte[] phase2CornerSliceTable = Array.Empty<byte>();
        private static byte[] phase2UDLayerSliceTable = Array.Empty<byte>();

        public static int Phase1MaxDepth { get; private set; }
        public static int Phase2MaxDepth { get; private set; }

        public static bool IsPhase1Built => phase1CornerEdgeTable.Length > 0;
        public static bool IsPhase2Built => phase2CornerSliceTable.Length > 0;

        public static void BuildPhase1(int maxDepth)
        {
            if (IsPhase1Built && Phase1MaxDepth >= maxDepth)
                return;

            CoordinateMoveTables.Build();

            phase1CornerEdgeTable = BuildPairTable(
                CubeCoordinates.CornerOrientCount,
                CubeCoordinates.EdgeOrientCount,
                CubeCoordinates.SolvedCornerOrient,
                CubeCoordinates.SolvedEdgeOrient,
                CoordinateMoveTables.CornerOriMove,
                CoordinateMoveTables.EdgeOriMove,
                KociembaMoveSets.Phase1Moves.Count,
                maxDepth);

            phase1CornerSliceTable = BuildPairTable(
                CubeCoordinates.CornerOrientCount,
                CubeCoordinates.UDSliceCount,
                CubeCoordinates.SolvedCornerOrient,
                CubeCoordinates.SolvedUDSliceComb,
                CoordinateMoveTables.CornerOriMove,
                CoordinateMoveTables.UDSliceMove,
                KociembaMoveSets.Phase1Moves.Count,
                maxDepth);

            phase1EdgeSliceTable = BuildPairTable(
                CubeCoordinates.EdgeOrientCount,
                CubeCoordinates.UDSliceCount,
                CubeCoordinates.SolvedEdgeOrient,
                CubeCoordinates.SolvedUDSliceComb,
                CoordinateMoveTables.EdgeOriMove,
                CoordinateMoveTables.UDSliceMove,
                KociembaMoveSets.Phase1Moves.Count,
                maxDepth);

            Phase1MaxDepth = maxDepth;
        }

        public static void BuildPhase2(int maxDepth)
        {
            if (IsPhase2Built && Phase2MaxDepth >= maxDepth)
                return;

            CoordinateMoveTables.Build();

            phase2CornerSliceTable = BuildPairTable(
                CubeCoordinates.CornerPermCount,
                CubeCoordinates.ESlicePermCount,
                CubeCoordinates.SolvedCornerPerm,
                CubeCoordinates.SolvedESlicePerm,
                CoordinateMoveTables.CornerPermMove,
                CoordinateMoveTables.ESlicePermMove,
                KociembaMoveSets.Phase2Moves.Count,
                maxDepth);

            phase2UDLayerSliceTable = BuildPairTable(
                CubeCoordinates.UDLayerPermCount,
                CubeCoordinates.ESlicePermCount,
                CubeCoordinates.SolvedUDLayerPerm,
                CubeCoordinates.SolvedESlicePerm,
                CoordinateMoveTables.UDLayerPermMove,
                CoordinateMoveTables.ESlicePermMove,
                KociembaMoveSets.Phase2Moves.Count,
                maxDepth);

            Phase2MaxDepth = maxDepth;
        }

        private static byte[] BuildPairTable(
            int countA,
            int countB,
            int solvedA,
            int solvedB,
            int[,] moveTableA,
            int[,] moveTableB,
            int moveCount,
            int maxDepth)
        {
            int total = countA * countB;

            byte[] table = new byte[total];
            Array.Fill(table, Unvisited);

            int solvedIndex = GetPairIndex(solvedA, solvedB, countB);
            table[solvedIndex] = 0;

            var queue = new int[total];
            int head = 0;
            int tail = 0;

            queue[tail++] = solvedIndex;

            while (head < tail)
            {
                int currentIndex = queue[head++];

                byte currentDepth = table[currentIndex];

                if (currentDepth >= maxDepth)
                    continue;

                int coordinateA = currentIndex / countB;
                int coordinateB = currentIndex % countB;

                for (int moveIndex = 0; moveIndex < moveCount; moveIndex++)
                {
                    int nextA = moveTableA[coordinateA, moveIndex];
                    int nextB = moveTableB[coordinateB, moveIndex];

                    int nextIndex = GetPairIndex(nextA, nextB, countB);

                    if (table[nextIndex] != Unvisited)
                        continue;

                    table[nextIndex] = (byte)(currentDepth + 1);
                    queue[tail++] = nextIndex;
                }
            }

            return table;
        }

        public static int EstimatePhase1(Phase1Coordinate key)
        {
            if (!IsPhase1Built)
                return BasicEstimatePhase1(key);

            int h = 0;

            h = Math.Max(h, LookupPair(phase1CornerEdgeTable, key.CornerOrient, key.EdgeOrient, CubeCoordinates.EdgeOrientCount, Phase1MaxDepth));
            h = Math.Max(h, LookupPair(phase1CornerSliceTable, key.CornerOrient, key.UDSliceComb, CubeCoordinates.UDSliceCount, Phase1MaxDepth));
            h = Math.Max(h, LookupPair(phase1EdgeSliceTable, key.EdgeOrient, key.UDSliceComb, CubeCoordinates.UDSliceCount, Phase1MaxDepth));

            return Math.Max(h, BasicEstimatePhase1(key));
        }

        public static int EstimatePhase2(Phase2Coordinate key)
        {
            if (!IsPhase2Built)
                return BasicEstimatePhase2(key);

            int h = 0;

            h = Math.Max(h, LookupPair(phase2CornerSliceTable, key.CornerPerm, key.ESlicePerm, CubeCoordinates.ESlicePermCount, Phase2MaxDepth));
            h = Math.Max(h, LookupPair(phase2UDLayerSliceTable, key.UDLayerPerm, key.ESlicePerm, CubeCoordinates.ESlicePermCount, Phase2MaxDepth));

            return Math.Max(h, BasicEstimatePhase2(key));
        }

        private static int LookupPair(byte[] table, int coordinateA, int coordinateB, int countB, int maxDepthBuilt)
        {
            if (coordinateA < 0 || coordinateB < 0)
                return maxDepthBuilt + 1;

            int index = GetPairIndex(coordinateA, coordinateB, countB);
            byte value = table[index];

            if (value == Unvisited)
                return maxDepthBuilt + 1;

            return value;
        }

        private static int BasicEstimatePhase1(Phase1Coordinate key)
        {
            int estimate = 0;

            if (key.CornerOrient != CubeCoordinates.SolvedCornerOrient)
                estimate = 1;

            if (key.EdgeOrient != CubeCoordinates.SolvedEdgeOrient)
                estimate = 1;

            if (key.UDSliceComb != CubeCoordinates.SolvedUDSliceComb)
                estimate = 1;

            return estimate;
        }

        private static int BasicEstimatePhase2(Phase2Coordinate key)
        {
            if (key.CornerPerm == CubeCoordinates.SolvedCornerPerm &&
                key.UDLayerPerm == CubeCoordinates.SolvedUDLayerPerm &&
                key.ESlicePerm == CubeCoordinates.SolvedESlicePerm)
            {
                return 0;
            }

            return 1;
        }

        private static int GetPairIndex(int a, int b, int countB)
        {
            return a * countB + b;
        }

        public static void SavePhase1(string filePath)
        {
            using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

            writer.Write(Phase1MaxDepth);

            WriteTable(writer, phase1CornerEdgeTable);
            WriteTable(writer, phase1CornerSliceTable);
            WriteTable(writer, phase1EdgeSliceTable);
        }

        public static bool LoadPhase1(string filePath)
        {
            if (!PruningTableCompressor.Exists(filePath))
                return false;

            try
            {
                using Stream stream = PruningTableCompressor.OpenRead(filePath);
                using BinaryReader reader = new BinaryReader(stream);

                Phase1MaxDepth = reader.ReadInt32();

                phase1CornerEdgeTable = ReadTable(reader);
                phase1CornerSliceTable = ReadTable(reader);
                phase1EdgeSliceTable = ReadTable(reader);

                return true;
            }
            catch
            {
                phase1CornerEdgeTable = Array.Empty<byte>();
                phase1CornerSliceTable = Array.Empty<byte>();
                phase1EdgeSliceTable = Array.Empty<byte>();
                Phase1MaxDepth = 0;
                return false;
            }
        }

        public static void SavePhase2(string filePath)
        {
            using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

            writer.Write(Phase2MaxDepth);

            WriteTable(writer, phase2CornerSliceTable);
            WriteTable(writer, phase2UDLayerSliceTable);
        }

        public static bool LoadPhase2(string filePath)
        {
            if (!PruningTableCompressor.Exists(filePath))
                return false;

            try
            {
                using Stream stream = PruningTableCompressor.OpenRead(filePath);
                using BinaryReader reader = new BinaryReader(stream);

                Phase2MaxDepth = reader.ReadInt32();

                phase2CornerSliceTable = ReadTable(reader);
                phase2UDLayerSliceTable = ReadTable(reader);

                return true;
            }
            catch
            {
                phase2CornerSliceTable = Array.Empty<byte>();
                phase2UDLayerSliceTable = Array.Empty<byte>();
                Phase2MaxDepth = 0;
                return false;
            }
        }

        public static void LoadOrBuild(string folder, int phase1Depth, int phase2Depth)
        {
            Directory.CreateDirectory(folder);

            string phase1Path = Path.Combine(folder, $"kociemba_coord_phase1_depth{phase1Depth}_v2.bin");
            string phase2Path = Path.Combine(folder, $"kociemba_coord_phase2_depth{phase2Depth}_v2.bin");

            if (!LoadPhase1(phase1Path) || Phase1MaxDepth < phase1Depth)
            {
                BuildPhase1(phase1Depth);
                SavePhase1(phase1Path);
            }

            if (!LoadPhase2(phase2Path) || Phase2MaxDepth < phase2Depth)
            {
                BuildPhase2(phase2Depth);
                SavePhase2(phase2Path);
            }
        }

        private static void WriteTable(BinaryWriter writer, byte[] table)
        {
            writer.Write(table.Length);
            writer.Write(table);
        }

        private static byte[] ReadTable(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }
    }
}
