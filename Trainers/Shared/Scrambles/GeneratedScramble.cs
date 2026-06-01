using CubeForge.Cube.FastState;
using System.Collections.Generic;

namespace CubeForge.Trainers.Shared
{
    public class GeneratedScramble
    {
        public bool Found { get; set; }
        public string FailureReason { get; set; } = "";
        public string CaseName { get; set; } = "";
        public int CaseNumber { get; set; }
        public FastCubeState TargetState { get; set; } = new();
        public List<SearchMove> ScrambleMoves { get; set; } = new();
        public List<SearchMove> SolutionMoves { get; set; } = new();
        public List<SearchMove> TargetSetupMoves { get; set; } = new();
        public int NodesSearched { get; set; }
        public int TotalNodesSearched { get; set; }
        public int AttemptNumber { get; set; }
        public string ScrambleText => MoveHelper.MovesToText(ScrambleMoves);
        public string SolutionText => MoveHelper.MovesToText(SolutionMoves);
        public string SetupText => MoveHelper.MovesToText(TargetSetupMoves);
    }
}
