using CubeForge.Cube.FastState;

namespace CubeForge.Trainers.Shared
{
    public class ScrambleOptions
    {
        public string Algorithm { get; set; } = "";
        public FastCubeState? TargetState { get; set; }
        public string TargetSetupMoves { get; set; } = "";
        public string CaseName { get; set; } = "";
        public int CaseNumber { get; set; }
        public int MaxAttempts { get; set; } = 60;
        public int MinScrambleLength { get; set; } = 8;
        public int MaxScrambleLength { get; set; } = 30;
        public int MaxPhase1Depth { get; set; } = 10;
        public int MaxPhase2Depth { get; set; } = 14;
        public int MaxNodesPerAttempt { get; set; } = 500_000;
        public int AttemptTimeoutMs { get; set; } = 1500;
        public int MaxTotalNodes { get; set; } = 3_000_000;
        public bool RejectInverseSetup { get; set; } = true;
    }
}
