using HelixToolkit.Wpf;
using CubeForge.Cube;
using CubeForge.Cube.FastState;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CubeForge.Trainers.Shared
{
    public sealed class CubeManager
    {
        private readonly HelixViewport3D viewPort;
        private CancellationTokenSource moveCancellation = new();

        public RubiksCube Cube { get; }
        public FastCubeState FastCube { get; private set; } = new FastCubeState();

        public CubeManager(HelixViewport3D viewPort)
        {
            this.viewPort = viewPort;

            Cube = new RubiksCube();
            Cube.Render(viewPort);
        }

        public void StopMoves()
        {
            moveCancellation.Cancel();
            moveCancellation.Dispose();
            moveCancellation = new CancellationTokenSource();
            Cube.StopVisualAnimations();
        }

        public void Reset()
        {
            StopMoves();
            Cube.ResetToSolved(viewPort);
            FastCube.SetSolved();
        }

        public void SetOrientation(CubeColor bottomColor, CubeColor frontColor)
        {
            Cube.SetDisplayOrientation(bottomColor, frontColor, viewPort);
        }

        public void SetRenderMode(CubeRenderMode mode, CubeColor topColor)
        {
            Cube.SetRenderMode(mode, topColor, viewPort);
        }

        public void ApplyMoves(IEnumerable<ParsedMove> moves, bool renderImmediately)
        {
            StopMoves();

            foreach (ParsedMove move in moves)
            {
                FastCube.ApplyMove(Cube.CurrentOrientation.ToBaseMove(move));

                if (renderImmediately)
                    Cube.ApplyDisplayedMove(move, viewPort);
            }
        }

        public void ApplyMove(ParsedMove move)
        {
            StopMoves();
            FastCube.ApplyMove(Cube.CurrentOrientation.ToBaseMove(move));
            Cube.ApplyDisplayedMove(move, viewPort);
        }

        public async Task ApplyMoveAsync(ParsedMove move, bool instantMoves, int speed)
        {
            CancellationToken token = moveCancellation.Token;
            token.ThrowIfCancellationRequested();

            if (instantMoves)
            {
                FastCube.ApplyMove(Cube.CurrentOrientation.ToBaseMove(move));
                Cube.ApplyDisplayedMove(move, viewPort);
                return;
            }

            await Cube.ApplyDisplayedMoveAnimated(move, viewPort, speed, token);
            token.ThrowIfCancellationRequested();
            FastCube.ApplyMove(Cube.CurrentOrientation.ToBaseMove(move));
        }


        public async Task ApplyVisualMoveAsync(ParsedMove move, int speed)
        {
            CancellationToken token = moveCancellation.Token;
            token.ThrowIfCancellationRequested();
            await Cube.ApplyDisplayedMoveAnimated(move, viewPort, speed, token);
        }

        public async Task ApplyVisualMove(FaceMove move, int turns, int speed)
        {
            await ApplyVisualMoveAsync(new ParsedMove(move, turns), speed);
        }

        public FastCubeState CloneFastState()
        {
            return FastCube.Clone();
        }
    }
}
