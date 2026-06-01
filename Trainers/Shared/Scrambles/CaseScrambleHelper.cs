using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Views;
using System;
using System.Collections.Generic;

namespace CubeForge.Trainers.Shared
{
    public class CaseScrambleHelper<TCase>
    {
        private readonly TargetScrambleGenerator scrambleGenerator;
        private readonly Func<TCase, string> getName;
        private readonly Func<TCase, int> getNumber;
        private readonly Func<TCase, string> getSetupMoves;
        private readonly Func<TCase, string> getAlgorithm;
        private readonly Random random = new();
        private readonly List<ScrambleEntry<TCase>> history = new();
        private int historyIndex = -1;

        public CaseScrambleHelper(CaseTargetRules rules, Func<TCase, string> getName, Func<TCase, int> getNumber, Func<TCase, string> getSetupMoves, Func<TCase, string> getAlgorithmMoves)
        {
            scrambleGenerator = new TargetScrambleGenerator(rules);
            this.getName = getName;
            this.getNumber = getNumber;
            this.getSetupMoves = getSetupMoves;
            this.getAlgorithm = getAlgorithmMoves;
        }

        public TCase? CurrentCase { get; private set; }
        public List<ParsedMove> CurrentAlgorithmMoves { get; private set; } = new();
        public List<ParsedMove> CurrentScrambleMoves { get; private set; } = new();
        public int HintIndex { get; set; }
        public bool ScrambleApplied { get; set; }
        public FastCubeState? ScrambledFastState { get; set; }
        public List<ParsedMove> AppliedScrambleMoves { get; set; } = new();
        public bool HasHistory => history.Count > 0;
        public bool CanMoveNextHistory => history.Count > 0 && historyIndex < history.Count - 1;

        public CaseScrambleResult<TCase> GenerateNext(List<TCase> pool)
        {
            if (pool.Count == 0)
                return CaseScrambleResult<TCase>.Fail("No selected cases found.");

            TCase selectedCase = pool[random.Next(pool.Count)];
            string algorithm = getAlgorithm(selectedCase);

            if (string.IsNullOrWhiteSpace(algorithm))
                return CaseScrambleResult<TCase>.Fail($"No algorithm found for {getName(selectedCase)}.");

            string setupMoves = getSetupMoves(selectedCase);
            FastCubeState targetState = FastCubeState.FromAlgorithm(setupMoves);

            GeneratedScramble result = scrambleGenerator.Generate(new ScrambleOptions
            {
                Algorithm = algorithm,
                TargetState = targetState,
                TargetSetupMoves = setupMoves,
                CaseName = getName(selectedCase),
                CaseNumber = getNumber(selectedCase),
                MaxAttempts = 60,
                MinScrambleLength = 8,
                MaxScrambleLength = 30,
                MaxPhase1Depth = 10,
                MaxPhase2Depth = 14,
                MaxNodesPerAttempt = 500_000,
                AttemptTimeoutMs = 1500,
                MaxTotalNodes = 3_000_000,
                RejectInverseSetup = true
            });

            if (!result.Found)
                return CaseScrambleResult<TCase>.Fail(result.FailureReason);

            var entry = new ScrambleEntry<TCase>
            {
                Case = selectedCase,
                AlgorithmText = algorithm,
                ScrambleText = result.ScrambleText
            };

            AddHistory(entry);
            return CaseScrambleResult<TCase>.Success(entry);
        }

        public ScrambleEntry<TCase> MoveNextHistory()
        {
            if (historyIndex < history.Count - 1)
                historyIndex++;

            return history[historyIndex];
        }

        public ScrambleEntry<TCase> MovePreviousHistory()
        {
            if (history.Count == 0)
                throw new InvalidOperationException("No trainer scramble history exists.");

            if (historyIndex > 0)
                historyIndex--;

            return history[historyIndex];
        }

        public void LoadEntry(ScrambleEntry<TCase> entry)
        {
            CurrentCase = entry.Case;
            CurrentAlgorithmMoves = ScrambleParser.Parse(entry.AlgorithmText);
            CurrentScrambleMoves = ScrambleParser.Parse(entry.ScrambleText);
            ScrambleApplied = false;
            ScrambledFastState = null;
            AppliedScrambleMoves.Clear();
            HintIndex = 0;
        }

        private void AddHistory(ScrambleEntry<TCase> entry)
        {
            if (historyIndex < history.Count - 1)
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);

            history.Add(entry);
            historyIndex = history.Count - 1;
        }
    }

    public class CaseScrambleResult<TCase>
    {
        public bool Found { get; set; }
        public string Message { get; set; } = "";
        public ScrambleEntry<TCase> Entry { get; set; } = new ScrambleEntry<TCase>();

        public static CaseScrambleResult<TCase> Success(ScrambleEntry<TCase> entry)
        {
            return new CaseScrambleResult<TCase>
            {
                Found = true,
                Entry = entry
            };
        }

        public static CaseScrambleResult<TCase> Fail(string message)
        {
            return new CaseScrambleResult<TCase>
            {
                Found = false,
                Message = message
            };
        }
    }
}
