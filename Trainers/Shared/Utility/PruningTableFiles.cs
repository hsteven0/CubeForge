using System;
using System.IO;

namespace CubeForge.Trainers.Shared
{
    public static class PruningTableFiles
    {
        // define depths
        public const int CrossDepth = 6;
        public const int KociembaPhase1Depth = 7;
        public const int KociembaPhase2Depth = 8;

        public static string RootFolder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data", "PruningTables");

        public static string CrossFolder =>
            Path.Combine(RootFolder, "Cross");

        public static string KociembaFolder =>
            Path.Combine(RootFolder, "Kociemba");
    }
}