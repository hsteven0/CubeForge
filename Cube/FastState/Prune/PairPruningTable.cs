using System;
using System.Collections.Generic;
using System.IO;
using CubeForge.Trainers.Shared;

namespace CubeForge.Cube.FastState
{
    public static class PairPruningTable
    {
        private static readonly Dictionary<string, Dictionary<ulong, byte>> cache = new();
        private static readonly Dictionary<string, int> depths = new();

        public static void Build(NCrossTarget target, F2LSlot slot, int maxDepth, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, slot, moveSet);

            if (cache.ContainsKey(tableKey) && depths.ContainsKey(tableKey) && depths[tableKey] >= maxDepth)
                return;

            depths[tableKey] = maxDepth;

            var table = new Dictionary<ulong, byte>();
            var queue = new Queue<(FastCubeState State, int Depth)>();
            var solved = new FastCubeState();

            ulong solvedKey = GetStateKey(solved, target, slot);

            table[solvedKey] = 0;
            queue.Enqueue((solved, 0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.Depth >= maxDepth)
                    continue;

                foreach (FaceMove move in SearchMoveSets.GetMoves(moveSet))
                {
                    foreach (int turns in SearchMoveSets.Turns)
                    {
                        FastCubeState next = current.State.Clone();
                        next.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move, turns)));

                        ulong key = GetStateKey(next, target, slot);

                        if (table.ContainsKey(key))
                            continue;

                        table[key] = (byte)(current.Depth + 1);
                        queue.Enqueue((next, current.Depth + 1));
                    }
                }
            }

            cache[tableKey] = table;
        }

        public static int Lookup(FastCubeState state, NCrossTarget target, F2LSlot slot, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, slot, moveSet);

            if (!cache.ContainsKey(tableKey)) Build(target, slot, 6, moveSet);

            ulong key = GetStateKey(state, target, slot);
            Dictionary<ulong, byte> table = cache[tableKey];

            if (table.TryGetValue(key, out byte distance))
                return distance;

            return depths[tableKey] + 1;
        }

        public static void Save(NCrossTarget target, F2LSlot slot, string filePath, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, slot, moveSet);

            if (!cache.ContainsKey(tableKey))
                return;

            Dictionary<ulong, byte> table = cache[tableKey];

            using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

            writer.Write(tableKey);
            writer.Write(depths[tableKey]);
            writer.Write(table.Count);

            foreach (var entry in table)
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
        }

        public static bool Load(NCrossTarget target, F2LSlot slot, string filePath, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, slot, moveSet);

            if (cache.ContainsKey(tableKey) && depths.ContainsKey(tableKey))
                return true;

            if (!PruningTableCompressor.Exists(filePath))
                return false;

            try
            {
                using Stream stream = PruningTableCompressor.OpenRead(filePath);
                using BinaryReader reader = new(stream);

                string fileTableKey = reader.ReadString();

                if (fileTableKey != tableKey)
                    return false;

                int depth = reader.ReadInt32();
                int count = reader.ReadInt32();

                var table = new Dictionary<ulong, byte>(count);

                for (int i = 0; i < count; i++)
                {
                    ulong key = reader.ReadUInt64();
                    byte value = reader.ReadByte();

                    table[key] = value;
                }

                cache[tableKey] = table;
                depths[tableKey] = depth;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LoadOrBuild(NCrossTarget target, F2LSlot slot, string filePath, int maxDepth)
        {
            LoadOrBuild(target, slot, filePath, maxDepth, MoveSet.FaceTurnsOnly);
        }

        public static void LoadOrBuild(NCrossTarget target, F2LSlot slot, string filePath, int maxDepth, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, slot, moveSet);

            if (Load(target, slot, filePath, moveSet) && depths.ContainsKey(tableKey) && depths[tableKey] >= maxDepth)
                return;

            Build(target, slot, maxDepth, moveSet);
            Save(target, slot, filePath, moveSet);
        }

        private static ulong GetStateKey(FastCubeState state, NCrossTarget target, F2LSlot slot)
        {
            target.GetSlotPieces(slot, out Corner corner, out Edge edge);

            ulong key = 0;
            int shift = 0;

            PackEdge(ref key, ref shift, state, edge);
            PackCorner(ref key, ref shift, state, corner);

            return key;
        }

        private static string GetTableKey(NCrossTarget target, F2LSlot slot, MoveSet moveSet)
        {
            target.GetSlotPieces(slot, out Corner corner, out Edge edge);

            return $"pair_{SearchMoveSets.GetFileKey(moveSet)}_{slot}_{(int)corner}_{(int)edge}";
        }

        private static void PackEdge(ref ulong key, ref int shift, FastCubeState state, Edge edge)
        {
            Edge position = state.FindEdgePosition(edge);

            int pos = (int)position;
            int ori = state.GetEdgeOrientation(position);

            key |= ((ulong)pos << shift);
            shift += 4;

            key |= ((ulong)ori << shift);
            shift += 1;
        }

        private static void PackCorner(ref ulong key, ref int shift, FastCubeState state, Corner corner)
        {
            Corner position = state.FindCornerPosition(corner);

            int pos = (int)position;
            int ori = state.GetCornerOrientation(position);

            key |= ((ulong)pos << shift);
            shift += 3;

            key |= ((ulong)ori << shift);
            shift += 2;
        }
    }
}