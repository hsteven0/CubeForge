using HelixToolkit.Wpf;
using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Trainers.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CubeForge.Trainers.Cross
{
    public sealed class CrossScrambleResult
    {
        public CrossScrambleResult(string scramble, List<SearchMove> guaranteedSolution)
        {
            Scramble = scramble;
            GuaranteedSolution = guaranteedSolution;
        }

        public string Scramble { get; }
        public List<SearchMove> GuaranteedSolution { get; }
    }

    public sealed class CrossScrambleManager
    {
        private readonly CubeManager cube;
        private readonly CrossTargetManager target;
        private readonly ComboBox generationModeComboBox;
        private readonly ComboBox difficultyComboBox;
        private readonly HelixViewport3D viewPort;
        private readonly Func<bool> instantMoves;
        private readonly Func<int> animationSpeed;
        private readonly Action setOrientation;
        private readonly Action setDisplayMode;

        public CrossScrambleManager(CubeManager cube, CrossTargetManager target, ComboBox generationModeComboBox, ComboBox difficultyComboBox, HelixViewport3D viewPort, Func<bool> useInstantMoves, Func<int> getAnimationSpeed, Action applyOrientation, Action applyDisplayMode)
        {
            this.cube = cube;
            this.target = target;
            this.generationModeComboBox = generationModeComboBox;
            this.difficultyComboBox = difficultyComboBox;
            this.viewPort = viewPort;
            this.instantMoves = useInstantMoves;
            this.animationSpeed = getAnimationSpeed;
            this.setOrientation = applyOrientation;
            this.setDisplayMode = applyDisplayMode;
        }

        public bool ScrambleApplied { get; private set; }
        public FastCubeState? ScrambledFastState { get; private set; }
        public string AppliedScrambleText { get; private set; } = "";

        public CrossScrambleResult Generate(NCrossTarget resolvedTarget)
        {
            string generationMode = CrossTargetManager.GetComboBoxText(generationModeComboBox, "Guaranteed solution");

            if (generationMode != "Guaranteed solution")
                return new CrossScrambleResult(Scrambler.GenerateScramble(20), new List<SearchMove>());

            return GenerateGuaranteed(resolvedTarget);
        }

        public void Clear()
        {
            ScrambleApplied = false;
            ScrambledFastState = null;
            AppliedScrambleText = "";
        }

        public void ResetCube()
        {
            cube.StopMoves();
            cube.Reset();
            setOrientation();
            setDisplayMode();
            Clear();
        }

        public async Task Apply(string scramble)
        {
            cube.StopMoves();
            cube.Reset();
            setOrientation();
            setDisplayMode();

            AppliedScrambleText = scramble;
            List<ParsedMove> moves = ScrambleParser.Parse(scramble);

            try
            {
                foreach (ParsedMove move in moves)
                    await cube.ApplyMoveAsync(move, instantMoves(), animationSpeed());
            }
            catch (OperationCanceledException)
            {
                return;
            }

            ScrambledFastState = cube.FastCube.Clone();
            ScrambleApplied = true;
        }

        public async Task ApplyCurrentInstantly(string scramble)
        {
            if (string.IsNullOrWhiteSpace(scramble))
                return;

            cube.StopMoves();
            cube.Reset();
            setOrientation();
            setDisplayMode();

            List<ParsedMove> moves = ScrambleParser.Parse(scramble);

            foreach (ParsedMove move in moves)
            {
                cube.FastCube.ApplyMove(cube.Cube.CurrentOrientation.ToBaseMove(move));
                cube.Cube.ApplyDisplayedMove(move, viewPort);
            }

            ScrambledFastState = cube.FastCube.Clone();
            AppliedScrambleText = scramble;
            ScrambleApplied = true;

            await Task.CompletedTask;
        }

        public void EnsurePruningTablesLoaded(NCrossTarget target)
        {
            string tableFolder = PruningTableFiles.CrossFolder;
            Directory.CreateDirectory(tableFolder);

            string orientationKey = GetOrientationKey(target);

            string crossPath = Path.Combine(
                tableFolder,
                $"cross_{orientationKey}_depth{PruningTableFiles.CrossDepth}.bin");

#if DEBUG
            CrossPruningTable.LoadOrBuild(target, crossPath, PruningTableFiles.CrossDepth);
#else
            if (!CrossPruningTable.Load(target, crossPath, MoveSet.FaceTurnsOnly))
                throw new InvalidOperationException("Missing cross pruning table files.");
#endif

            foreach (F2LSlot slot in target.Slots)
            {
                string slotName = slot.ToString().ToLower();

                string pairPath = Path.Combine(
                    tableFolder,
                    $"pair_{orientationKey}_{slotName}_depth{PruningTableFiles.CrossDepth}.bin");

                string nCrossPath = Path.Combine(
                    tableFolder,
                    $"ncross_{orientationKey}_{slotName}_depth{PruningTableFiles.CrossDepth}.bin");

#if DEBUG
                PairPruningTable.LoadOrBuild(target, slot, pairPath, PruningTableFiles.CrossDepth);
                NCrossPruningTable.LoadOrBuild(target, slot, nCrossPath, PruningTableFiles.CrossDepth);
#else
            if (!PairPruningTable.Load(target, slot, pairPath, MoveSet.FaceTurnsOnly))
                throw new InvalidOperationException("Missing pair pruning table files.");

            if (!NCrossPruningTable.Load(target, slot, nCrossPath, MoveSet.FaceTurnsOnly))
                throw new InvalidOperationException("Missing N-Cross pruning table files.");
#endif
            }
        }

        private CrossScrambleResult GenerateGuaranteed(NCrossTarget resolvedTarget)
        {
            EnsurePruningTablesLoaded(resolvedTarget);

            int maxDepth = GetMaxDepth();
            int minScrambleLength = GetMinScrambleLength();

            for (int attempt = 1; attempt <= 500; attempt++)
            {
                string candidate = Scrambler.GenerateScramble(20);

                FastCubeState test = new FastCubeState();
                test.SetSolved();

                foreach (ParsedMove move in ScrambleParser.Parse(candidate))
                    test.ApplyMove(cube.Cube.CurrentOrientation.ToBaseMove(move));

                NCrossSearchResult result = NCrossSearcher.Solve(test, resolvedTarget, maxDepth);

                if (!result.Found)
                    continue;

                if (result.Solution.Count == 0)
                    continue;

                if (result.Solution.Count < GetMinSolutionLength())
                    continue;

                if (ScrambleParser.Parse(candidate).Count < minScrambleLength)
                    continue;

                if (HasEasySolution(candidate, result.Solution))
                    continue;

                return new CrossScrambleResult(candidate, new List<SearchMove>(result.Solution));
            }

            throw new InvalidOperationException("Could not generate a guaranteed scrambleManager with the current settings. Try increasing max solution length or lowering difficulty.");
        }

        private int GetMinSolutionLength()
        {
            string difficulty = CrossTargetManager.GetComboBoxText(difficultyComboBox, "Medium");

            return difficulty switch
            {
                "Easy" => 3,
                "Medium" => 5,
                "Hard" => 7,
                "Custom" => Math.Max(1, target.GetSelectedSolutionLength() - 2),
                _ => 5
            };
        }

        private int GetMinScrambleLength()
        {
            string difficulty = CrossTargetManager.GetComboBoxText(difficultyComboBox, "Medium");

            return difficulty switch
            {
                "Easy" => 16,
                "Medium" => 18,
                "Hard" => 20,
                "Custom" => 18,
                _ => 18
            };
        }

        private int GetMaxDepth()
        {
            string difficulty = CrossTargetManager.GetComboBoxText(difficultyComboBox, "Medium");

            if (difficulty == "Easy")
                return Math.Min(target.GetSelectedSolutionLength(), 5);

            if (difficulty == "Medium")
                return Math.Min(target.GetSelectedSolutionLength(), 8);

            if (difficulty == "Hard")
                return Math.Max(target.GetSelectedSolutionLength(), 9);

            return target.GetSelectedSolutionLength();
        }

        private bool HasEasySolution(string scrambleText, List<SearchMove> solution)
        {
            List<ParsedMove> scrambleMoves = ScrambleParser.Parse(scrambleText);

            if (solution.Count <= 2)
                return true;

            if (HasRepeatedAxes(solution))
                return true;

            if (IsInverseScrambleTail(scrambleMoves, solution))
                return true;

            return false;
        }

        private bool HasRepeatedAxes(List<SearchMove> solution)
        {
            if (solution.Count < 4)
                return false;

            int repeatedAxisCount = 0;

            for (int i = 1; i < solution.Count; i++)
            {
                if (GetAxis(solution[i].Move) == GetAxis(solution[i - 1].Move))
                    repeatedAxisCount++;
            }

            return repeatedAxisCount >= solution.Count - 2;
        }

        private bool IsInverseScrambleTail(List<ParsedMove> scrambleMoves, List<SearchMove> solution)
        {
            if (solution.Count > scrambleMoves.Count)
                return false;

            int scrambleIndex = scrambleMoves.Count - 1;

            for (int i = 0; i < solution.Count; i++, scrambleIndex--)
            {
                ParsedMove scrambleMove = scrambleMoves[scrambleIndex];
                SearchMove solutionMove = solution[i];

                if (scrambleMove.Move != solutionMove.Move)
                    return false;

                int inverseTurns = scrambleMove.Turns switch
                {
                    1 => 3,
                    2 => 2,
                    3 => 1,
                    _ => scrambleMove.Turns
                };

                if (solutionMove.Turns != inverseTurns)
                    return false;
            }

            return true;
        }

        private static int GetAxis(FaceMove move)
        {
            return move switch
            {
                FaceMove.U or FaceMove.D => 0,
                FaceMove.R or FaceMove.L => 1,
                FaceMove.F or FaceMove.B => 2,
                _ => 3
            };
        }

        private static string GetOrientationKey(NCrossTarget target)
        {
            string bottom = target.Orientation.BottomColor.ToString().ToLower();
            string front = target.Orientation.FrontColor.ToString().ToLower();

            return $"bottom_{bottom}_front_{front}";
        }
    }
}
