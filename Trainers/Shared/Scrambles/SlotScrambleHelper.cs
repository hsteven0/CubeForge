using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeForge.Trainers.Shared
{
    public sealed class SlotScrambleHelper<TCase, TSlot> where TCase : ICaseInfo
    {
        private readonly Func<IEnumerable<TSlot>> getEnabledSlots;
        private readonly Func<TCase, string> getCaseName;
        private readonly Func<TCase, TSlot, string> getAlgorithmMoves;
        private readonly Func<TCase, TSlot, string> getSetupMoves;
        private readonly Func<TSlot, CaseTargetRules> getTargetRules;
        private readonly Func<TCase, TSlot, string, FastCubeState>? buildTargetState;
        private readonly Random random = new();
        private readonly List<SlotScrambleEntry<TCase, TSlot>> history = new();
        private int historyIndex = -1;

        public SlotScrambleHelper(Func<IEnumerable<TSlot>> getEnabledContexts, Func<TCase, string> getCaseName, Func<TCase, TSlot, string> getAlgorithmMoves, Func<TCase, TSlot, string> getSetupMoves, Func<TSlot, CaseTargetRules> getTargetRules, Func<TCase, TSlot, string, FastCubeState>? buildTargetState = null)
        {
            this.getEnabledSlots = getEnabledContexts;
            this.getCaseName = getCaseName;
            this.getAlgorithmMoves = getAlgorithmMoves;
            this.getSetupMoves = getSetupMoves;
            this.getTargetRules = getTargetRules;
            this.buildTargetState = buildTargetState;
        }

        public TCase? CurrentCase { get; private set; }
        public TSlot? CurrentContext { get; private set; }
        public List<ParsedMove> CurrentAlgorithmMoves { get; private set; } = new();
        public List<ParsedMove> CurrentScrambleMoves { get; private set; } = new();
        public int HintIndex { get; set; }
        public bool ScrambleApplied { get; set; }
        public FastCubeState? ScrambledFastState { get; set; }
        public List<ParsedMove> AppliedScrambleMoves { get; set; } = new();
        public bool HasHistory => history.Count > 0;
        public bool CanMoveNextHistory => history.Count > 0 && historyIndex < history.Count - 1;

        public SlotScrambleResult<TCase, TSlot> GenerateNext(List<TCase> pool)
        {
            if (pool.Count == 0)
                return SlotScrambleResult<TCase, TSlot>.Fail("No selected cases found.");

            List<TSlot> contexts = getEnabledSlots().Distinct().ToList();

            if (contexts.Count == 0)
                return SlotScrambleResult<TCase, TSlot>.Fail("Select at least one slot first.");

            TCase selectedCase = pool[random.Next(pool.Count)];
            TSlot context = contexts[random.Next(contexts.Count)];
            string caseName = getCaseName(selectedCase);
            string algorithm = getAlgorithmMoves(selectedCase, context);

            if (string.IsNullOrWhiteSpace(algorithm))
                return SlotScrambleResult<TCase, TSlot>.Fail($"No algorithm found for {caseName}.");

            string setupMoves = getSetupMoves(selectedCase, context);
            FastCubeState targetState = buildTargetState?.Invoke(selectedCase, context, setupMoves) ?? FastCubeState.FromAlgorithm(setupMoves);
            TargetScrambleGenerator generator = new TargetScrambleGenerator(getTargetRules(context), random);

            GeneratedScramble result = generator.Generate(new ScrambleOptions
            {
                Algorithm = algorithm,
                TargetState = targetState,
                TargetSetupMoves = setupMoves,
                CaseName = caseName,
                CaseNumber = selectedCase.Number,
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
                return SlotScrambleResult<TCase, TSlot>.Fail(result.FailureReason);

            var entry = new SlotScrambleEntry<TCase, TSlot>
            {
                Case = selectedCase,
                Context = context,
                AlgorithmText = algorithm,
                ScrambleText = result.ScrambleText
            };

            AddHistory(entry);
            return SlotScrambleResult<TCase, TSlot>.Success(entry);
        }

        public SlotScrambleEntry<TCase, TSlot> MoveNextHistory()
        {
            if (historyIndex < history.Count - 1)
                historyIndex++;

            return history[historyIndex];
        }

        public SlotScrambleEntry<TCase, TSlot> MovePreviousHistory()
        {
            if (history.Count == 0)
                throw new InvalidOperationException("No trainer scramble history exists.");

            if (historyIndex > 0)
                historyIndex--;

            return history[historyIndex];
        }

        public void LoadEntry(SlotScrambleEntry<TCase, TSlot> entry)
        {
            CurrentCase = entry.Case;
            CurrentContext = entry.Context;
            CurrentAlgorithmMoves = ScrambleParser.Parse(entry.AlgorithmText);
            CurrentScrambleMoves = ScrambleParser.Parse(entry.ScrambleText);
            ScrambleApplied = false;
            ScrambledFastState = null;
            AppliedScrambleMoves.Clear();
            HintIndex = 0;
        }

        private void AddHistory(SlotScrambleEntry<TCase, TSlot> entry)
        {
            if (historyIndex < history.Count - 1)
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);

            history.Add(entry);
            historyIndex = history.Count - 1;
        }
    }

    public sealed class SlotScrambleEntry<TCase, TContext>
    {
        public TCase Case { get; set; } = default!;
        public TContext Context { get; set; } = default!;
        public string AlgorithmText { get; set; } = "";
        public string ScrambleText { get; set; } = "";
    }

    public sealed class SlotScrambleResult<TCase, TContext>
    {
        public bool Found { get; set; }
        public string Message { get; set; } = "";
        public SlotScrambleEntry<TCase, TContext> Entry { get; set; } = new();

        public static SlotScrambleResult<TCase, TContext> Success(SlotScrambleEntry<TCase, TContext> entry)
        {
            return new SlotScrambleResult<TCase, TContext>
            {
                Found = true,
                Entry = entry
            };
        }

        public static SlotScrambleResult<TCase, TContext> Fail(string message)
        {
            return new SlotScrambleResult<TCase, TContext>
            {
                Found = false,
                Message = message
            };
        }
    }
}
