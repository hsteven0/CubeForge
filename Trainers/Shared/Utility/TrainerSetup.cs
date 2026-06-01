using System;
using System.IO;
using CubeForge.Cube.FastState.Kociemba;

namespace CubeForge.Trainers.Shared
{
    public static class TrainerSetup
    {
        public static void LoadKociembaTables()
        {
            string folder = PruningTableFiles.KociembaFolder;
            Directory.CreateDirectory(folder);

#if DEBUG
            PruningTables.LoadOrBuild(
                folder,
                phase1Depth: PruningTableFiles.KociembaPhase1Depth,
                phase2Depth: PruningTableFiles.KociembaPhase2Depth);
#else
            string phase1Path = Path.Combine(
                folder,
                $"kociemba_coord_phase1_depth{PruningTableFiles.KociembaPhase1Depth}_v2.bin");

            string phase2Path = Path.Combine(
                folder,
                $"kociemba_coord_phase2_depth{PruningTableFiles.KociembaPhase2Depth}_v2.bin");

            if (!PruningTables.LoadPhase1(phase1Path))
                throw new InvalidOperationException("Missing Kociemba phase 1 pruning table.");

            if (!PruningTables.LoadPhase2(phase2Path))
                throw new InvalidOperationException("Missing Kociemba phase 2 pruning table.");
#endif
        }
    }
}