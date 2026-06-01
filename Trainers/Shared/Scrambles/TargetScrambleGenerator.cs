using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Cube.FastState.Kociemba;
using System;
using System.Collections.Generic;

namespace CubeForge.Trainers.Shared
{
    public class TargetScrambleGenerator
    {
        private readonly CaseTargetRules rules;
        private readonly Random random;

        public TargetScrambleGenerator(CaseTargetRules rules, Random? random = null)
        {
            this.rules = rules;
            this.random = random ?? new Random();
        }

        public GeneratedScramble Generate(ScrambleOptions options)
        {
            if (options.TargetState == null)
            {
                return new GeneratedScramble
                {
                    Found = false,
                    CaseName = options.CaseName,
                    CaseNumber = options.CaseNumber,
                    FailureReason = "No trusted FastCubeState target was provided."
                };
            }

            FastCubeState trustedTarget = rules.FixOrientation(options.TargetState.Clone());

            if (!rules.IsValid(trustedTarget))
            {
                return new GeneratedScramble
                {
                    Found = false,
                    CaseName = options.CaseName,
                    CaseNumber = options.CaseNumber,
                    TargetState = trustedTarget,
                    FailureReason = rules.InvalidMessage(options.CaseName)
                };
            }

            List<SearchMove> setupMoves = MoveHelper.ParseSearchMoves(options.TargetSetupMoves);
            int totalNodes = 0;

            for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
            {
                FastCubeState target = trustedTarget.Clone();
                TwoPhaseSolver solver = new();

                TwoPhaseSolution solution = solver.Solve(
                    target,
                    options.MaxPhase1Depth,
                    options.MaxPhase2Depth,
                    maxNodes: options.MaxNodesPerAttempt,
                    timeoutMs: options.AttemptTimeoutMs,
                    randomizeMoveOrder: true,
                    randomSeed: random.Next());

                totalNodes += solution.NodesSearched;

                if (totalNodes > options.MaxTotalNodes)
                {
                    return new GeneratedScramble
                    {
                        Found = false,
                        CaseName = options.CaseName,
                        CaseNumber = options.CaseNumber,
                        TargetState = target,
                        TotalNodesSearched = totalNodes,
                        FailureReason = $"Generation stopped after searching {totalNodes:N0} nodes. Try again or raise MaxTotalNodes."
                    };
                }

                if (!solution.Found)
                    continue;

                List<SearchMove> scrambleMoves = TwoPhaseSolution.InvertMoves(solution.Moves);

                if (scrambleMoves.Count < options.MinScrambleLength)
                    continue;

                if (scrambleMoves.Count > options.MaxScrambleLength)
                    continue;

                if (options.RejectInverseSetup && setupMoves.Count > 0 && MoveHelper.SameMoveText(scrambleMoves, setupMoves))
                    continue;

                FastCubeState check = new FastCubeState();
                MoveHelper.ApplyMoves(check, scrambleMoves);

                if (!SameState(check, target))
                    continue;

                return new GeneratedScramble
                {
                    Found = true,
                    CaseName = options.CaseName,
                    CaseNumber = options.CaseNumber,
                    TargetState = target,
                    ScrambleMoves = scrambleMoves,
                    SolutionMoves = solution.Moves,
                    TargetSetupMoves = setupMoves,
                    NodesSearched = solution.NodesSearched,
                    TotalNodesSearched = totalNodes,
                    AttemptNumber = attempt
                };
            }

            return new GeneratedScramble
            {
                Found = false,
                CaseName = options.CaseName,
                CaseNumber = options.CaseNumber,
                TargetState = trustedTarget,
                TotalNodesSearched = totalNodes,
                FailureReason = $"Could not generate a scramble matching the trusted FastCubeState target after {options.MaxAttempts} attempts."
            };
        }

        private static bool SameState(FastCubeState a, FastCubeState b)
        {
            for (int i = 0; i < 8; i++)
            {
                Corner position = (Corner)i;

                if (a.GetCornerPiece(position) != b.GetCornerPiece(position))
                    return false;

                if (a.GetCornerOrientation(position) != b.GetCornerOrientation(position))
                    return false;
            }

            for (int i = 0; i < 12; i++)
            {
                Edge position = (Edge)i;

                if (a.GetEdgePiece(position) != b.GetEdgePiece(position))
                    return false;

                if (a.GetEdgeOrientation(position) != b.GetEdgeOrientation(position))
                    return false;
            }

            return true;
        }
    }
}
