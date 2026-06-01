using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeForge.Trainers.Shared;

namespace CubeForge.Cube.FastState
{
    public static class CrossPruningTable
    {
        private static readonly Dictionary<string, Dictionary<ulong, byte>> cache = new();
        private static readonly Dictionary<string, int> depths = new();

        public static void Build(NCrossTarget target, int maxDepth, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, moveSet);

            if (cache.ContainsKey(tableKey) && depths.ContainsKey(tableKey) && depths[tableKey] >= maxDepth)
            {
                return;
            }

            depths[tableKey] = maxDepth;

            var table = new Dictionary<ulong, byte>();
            var queue = new Queue<(FastCubeState State, int Depth)>();
            var solved = new FastCubeState();

            ulong solvedKey = GetStateKey(solved, target);
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

                        ulong key = GetStateKey(next, target);
                        if (table.ContainsKey(key))
                            continue;

                        table[key] = (byte)(current.Depth + 1);
                        queue.Enqueue((next, current.Depth + 1));
                    }
                }
            }

            cache[tableKey] = table;
        }

        public static int Lookup(FastCubeState state, NCrossTarget target, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, moveSet);

            if (!cache.ContainsKey(tableKey)) Build(target, 6, moveSet);

            ulong key = GetStateKey(state, target);
            Dictionary<ulong, byte> table = cache[tableKey];

            if (table.TryGetValue(key, out byte distance))
                return distance;

            return depths[tableKey] + 1;
        }

        public static void Save(NCrossTarget target, string filePath, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, moveSet);

            if (!cache.ContainsKey(tableKey)) return;

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

        public static bool Load(NCrossTarget target, string filePath, MoveSet moveSet)
        {
            if (!PruningTableCompressor.Exists(filePath))
                return false;

            string tableKey = GetTableKey(target, moveSet);

            try
            {
                using Stream stream = PruningTableCompressor.OpenRead(filePath);
                using BinaryReader reader = new BinaryReader(stream);

                string filetableKey = reader.ReadString();

                if (filetableKey != tableKey)
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

        public static void LoadOrBuild(NCrossTarget target, string filePath, int maxDepth)
        {
            LoadOrBuild(target, filePath, maxDepth, MoveSet.FaceTurnsOnly);
        }

        public static void LoadOrBuild(NCrossTarget target,string filePath, int maxDepth, MoveSet moveSet)
        {
            string tableKey = GetTableKey(target, moveSet);

            if (Load(target, filePath, moveSet) && depths.ContainsKey(tableKey) && depths[tableKey] >= maxDepth)
            {
                return;
            }

            Build(target, maxDepth, moveSet);
            Save(target, filePath, moveSet);
        }

        private static ulong GetStateKey(FastCubeState state, NCrossTarget target)
        {
            ulong key = 0;
            int shift = 0;

            foreach (Edge edge in target.GetCrossEdges())
            {
                PackCrossEdge(ref key, ref shift, state, edge);
            }

            return key;
        }

        private static string GetTableKey(NCrossTarget target, MoveSet moveSet)
        {
            string cross = string.Join("_", target.GetCrossEdges().Select(e => ((int)e).ToString()));
            return $"cross_{SearchMoveSets.GetFileKey(moveSet)}_{cross}";
        }

        private static void PackCrossEdge(ref ulong key, ref int shift, FastCubeState state, Edge edgePiece)
        {
            Edge position = state.FindEdgePosition(edgePiece);

            int pos = (int)position;
            int ori = state.GetEdgeOrientation(position);

            key |= ((ulong)pos << shift);
            shift += 4;

            key |= ((ulong)ori << shift);
            shift += 1;
        }
    }
}
