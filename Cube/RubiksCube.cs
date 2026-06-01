using HelixToolkit.Geometry;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Threading;

namespace CubeForge.Cube
{
    public class RubiksCube
    {
        public Cubie[,,] Cubies { get; }

        private readonly double cubieSize;
        private readonly double spacing;
        private readonly Color bodyColor;

        private readonly List<ModelVisual3D> renderedCubieVisuals = new();
        private bool isAnimating = false;

        private Transform3D displayOrientationTransform = Transform3D.Identity;

        public CubeOrientation CurrentOrientation { get; private set; } = new CubeOrientation(CubeColor.Yellow, CubeColor.Red);

        public CubeRenderMode RenderMode { get; private set; } = CubeRenderMode.FullColor;
        public CubeColor TrainerTopColor { get; private set; } = CubeColor.White;
        public string TrainerF2LSlot { get; private set; } = "FR";
        public CubeRenderFilter RenderFilter { get; private set; } = CubeRenderFilter.ForMode(CubeRenderMode.FullColor, CubeColor.White);
        private static readonly Color HiddenStickerColor = Color.FromRgb(70, 70, 70);

        public RubiksCube(double cubieSize = 0.9, double spacing = 0.88)
        {
            this.cubieSize = cubieSize;
            this.spacing = spacing;
            bodyColor = Color.FromRgb(0, 0, 0);

            Cubies = new Cubie[3, 3, 3];

            InitializeCubies();
        }

        private void InitializeCubies()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        var cubie = new Cubie(x, y, z);

                        if (z == 2) cubie.SetSticker(Face.Front, Colors.Red);
                        if (z == 0) cubie.SetSticker(Face.Back, Colors.Orange);

                        if (x == 2) cubie.SetSticker(Face.Right, Colors.Blue);
                        if (x == 0) cubie.SetSticker(Face.Left, Colors.Green);

                        if (y == 2) cubie.SetSticker(Face.Up, Colors.White);
                        if (y == 0) cubie.SetSticker(Face.Down, Colors.Yellow);

                        Cubies[x, y, z] = cubie;
                    }
                }
            }
        }

        public void Render(HelixViewport3D viewport)
        {
            ClearRenderedCubies(viewport);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        Cubie cubie = Cubies[x, y, z];
                        cubie.Visual = CreateCubieVisual(cubie);
                        cubie.Visual.Transform = displayOrientationTransform;

                        viewport.Children.Add(cubie.Visual);
                        renderedCubieVisuals.Add(cubie.Visual);
                    }
                }
            }
        }

        private void ClearRenderedCubies(HelixViewport3D viewport)
        {
            foreach (var visual in renderedCubieVisuals)
            {
                viewport.Children.Remove(visual);
            }

            renderedCubieVisuals.Clear();
        }

        private ModelVisual3D CreateCubieVisual(Cubie cubie)
        {
            double centerX = (cubie.X - 1) * spacing;
            double centerY = (cubie.Y - 1) * spacing;
            double centerZ = (cubie.Z - 1) * spacing;

            var group = new Model3DGroup();

            group.Children.Add(CreateBox(
                centerX, centerY, centerZ,
                cubieSize, cubieSize, cubieSize,
                bodyColor));

            double stickerThickness = 0.05;
            double stickerInset = 0.12;
            double stickerSize = cubieSize - (2 * stickerInset);
            double offset = (cubieSize / 2.0) + (stickerThickness / 2.0) + 0.001;

            if (cubie.Stickers[Face.Front] is Color frontColor)
            {
                group.Children.Add(CreateBox(
                    centerX, centerY, centerZ + offset,
                    stickerSize, stickerSize, stickerThickness,
                    GetStickerColor(cubie, Face.Front, frontColor)));
            }

            if (cubie.Stickers[Face.Back] is Color backColor)
            {
                group.Children.Add(CreateBox(
                    centerX, centerY, centerZ - offset,
                    stickerSize, stickerSize, stickerThickness,
                    GetStickerColor(cubie, Face.Back, backColor)));
            }

            if (cubie.Stickers[Face.Right] is Color rightColor)
            {
                group.Children.Add(CreateBox(
                    centerX + offset, centerY, centerZ,
                    stickerThickness, stickerSize, stickerSize,
                    GetStickerColor(cubie, Face.Right, rightColor)));
            }

            if (cubie.Stickers[Face.Left] is Color leftColor)
            {
                group.Children.Add(CreateBox(
                    centerX - offset, centerY, centerZ,
                    stickerThickness, stickerSize, stickerSize,
                    GetStickerColor(cubie, Face.Left, leftColor)));
            }

            if (cubie.Stickers[Face.Up] is Color upColor)
            {
                group.Children.Add(CreateBox(
                    centerX, centerY + offset, centerZ,
                    stickerSize, stickerThickness, stickerSize,
                    GetStickerColor(cubie, Face.Up, upColor)));
            }

            if (cubie.Stickers[Face.Down] is Color downColor)
            {
                group.Children.Add(CreateBox(
                    centerX, centerY - offset, centerZ,
                    stickerSize, stickerThickness, stickerSize,
                    GetStickerColor(cubie, Face.Down, downColor)));
            }

            return new ModelVisual3D { Content = group };
        }

        public void SetRenderMode(CubeRenderMode renderMode, CubeColor trainerTopColor, HelixViewport3D viewport)
        {
            RenderMode = renderMode;
            TrainerTopColor = trainerTopColor;

            IEnumerable<string> visibleSlots = renderMode switch
            {
                CubeRenderMode.F2LTrainer => new[] { TrainerF2LSlot },
                CubeRenderMode.CrossTrainer => RenderFilter.VisibleF2LSlots,
                _ => Enumerable.Empty<string>()
            };

            RenderFilter = CubeRenderFilter.ForMode(renderMode, trainerTopColor, visibleSlots);

            Render(viewport);
        }

        public void SetRenderFilter(CubeRenderFilter filter, HelixViewport3D viewport)
        {
            RenderMode = filter.Mode;
            TrainerTopColor = filter.FocusColor;
            RenderFilter = filter;

            Render(viewport);
        }

        public void SetTrainerPairSlot(string slot)
        {
            TrainerF2LSlot = CubeRenderFilter.NormalizeSlotKey(slot);

            if (string.IsNullOrWhiteSpace(TrainerF2LSlot))
                TrainerF2LSlot = "FR";

            if (RenderMode == CubeRenderMode.F2LTrainer)
                RenderFilter.SetVisibleSlots(new[] { TrainerF2LSlot });
        }

        public void SetTrainerPairSlot(string slot, HelixViewport3D viewport)
        {
            SetTrainerPairSlot(slot);

            if (RenderMode == CubeRenderMode.F2LTrainer)
                Render(viewport);
        }

        private Color GetStickerColor(Cubie cubie, Face stickerFace, Color realColor)
        {
            if (RenderMode == CubeRenderMode.FullColor)
                return realColor;

            if (RenderMode == CubeRenderMode.F2LTrainer || RenderMode == CubeRenderMode.CrossTrainer)
            {
                return GetFocusStickerColor(cubie, realColor);
            }

            Color trainerColor = ToWpfColor(TrainerTopColor);

            if (RenderMode == CubeRenderMode.PllTrainer)
            {
                return HasStickerColor(cubie, trainerColor) ? realColor : HiddenStickerColor;
            }

            if (AreSameColor(realColor, trainerColor))
                return trainerColor;

            return HiddenStickerColor;
        }

        private Color GetFocusStickerColor(Cubie cubie, Color realColor)
        {
            // Render the stickers to focus on, hide the hidden
            CubeColor bottomCubeColor = CubeColorHelper.GetOpposite(RenderFilter.FocusColor);
            Color focusColor = ToWpfColor(RenderFilter.FocusColor);
            Color bottomColor = ToWpfColor(bottomCubeColor);

            if (RenderFilter.HideFocusColorStickers && AreSameColor(realColor, focusColor))
                return HiddenStickerColor;

            if (RenderFilter.ShowCenters && IsCenterPiece(cubie))
                return realColor;

            if (RenderFilter.ShowCrossEdges && IsCrossPiece(cubie, bottomColor))
                return realColor;

            foreach (string slot in RenderFilter.VisibleF2LSlots)
            {
                GetPairSlotColors(slot, out Color firstSideColor, out Color secondSideColor);

                if (IsTargetPairEdge(cubie, firstSideColor, secondSideColor))
                    return realColor;

                if (IsTargetPairCorner(cubie, bottomColor, firstSideColor, secondSideColor))
                    return realColor;
            }

            return HiddenStickerColor;
        }

        private static bool IsCenterPiece(Cubie cubie)
        {
            return CountStickers(cubie) == 1;
        }

        private static bool IsCrossPiece(Cubie cubie, Color bottomColor)
        {
            return CountStickers(cubie) == 2 && HasStickerColor(cubie, bottomColor);
        }

        private static bool IsTargetPairEdge(Cubie cubie, Color firstSideColor, Color secondSideColor)
        {
            return CountStickers(cubie) == 2 &&
                   HasStickerColor(cubie, firstSideColor) &&
                   HasStickerColor(cubie, secondSideColor);
        }

        private static bool IsTargetPairCorner(Cubie cubie, Color bottomColor, Color firstSideColor, Color secondSideColor)
        {
            return CountStickers(cubie) == 3 &&
                   HasStickerColor(cubie, bottomColor) &&
                   HasStickerColor(cubie, firstSideColor) &&
                   HasStickerColor(cubie, secondSideColor);
        }

        private static int CountStickers(Cubie cubie)
        {
            int count = 0;

            foreach (Color? color in cubie.Stickers.Values)
            {
                if (color.HasValue)
                    count++;
            }

            return count;
        }

        private static bool HasStickerColor(Cubie cubie, Color targetColor)
        {
            foreach (Color? color in cubie.Stickers.Values)
            {
                if (color.HasValue && AreSameColor(color.Value, targetColor))
                    return true;
            }

            return false;
        }

        private void GetPairSlotColors(string slot, out Color firstSideColor, out Color secondSideColor)
        {
            CubeColor displayedFront = CurrentOrientation.FrontColor;
            CubeColor displayedBack = CubeColorHelper.GetOpposite(displayedFront);
            CubeColor displayedRight = GetDisplayedRightColor();
            CubeColor displayedLeft = CubeColorHelper.GetOpposite(displayedRight);

            (CubeColor first, CubeColor second) = CubeRenderFilter.NormalizeSlotKey(slot) switch
            {
                "FL" => (displayedFront, displayedLeft),
                "BL" => (displayedBack, displayedLeft),
                "BR" => (displayedBack, displayedRight),
                _ => (displayedFront, displayedRight)
            };

            firstSideColor = ToWpfColor(first);
            secondSideColor = ToWpfColor(second);
        }

        private CubeColor GetDisplayedRightColor()
        {
            Vector3D bottomDirection = CubeColorHelper.GetSolvedDirection(CurrentOrientation.BottomColor);
            Vector3D frontDirection = CubeColorHelper.GetSolvedDirection(CurrentOrientation.FrontColor);

            Vector3D upDirection = -bottomDirection;
            Vector3D rightDirection = Vector3D.CrossProduct(upDirection, frontDirection);
            rightDirection.Normalize();

            return GetColorForDirection(rightDirection);
        }

        private static CubeColor GetColorForDirection(Vector3D direction)
        {
            CubeColor[] colors =
            {
                CubeColor.White,
                CubeColor.Yellow,
                CubeColor.Red,
                CubeColor.Orange,
                CubeColor.Blue,
                CubeColor.Green
            };

            foreach (CubeColor color in colors)
            {
                Vector3D solvedDirection = CubeColorHelper.GetSolvedDirection(color);

                if (SameDirection(direction, solvedDirection))
                    return color;
            }

            return CubeColor.Blue;
        }

        private static bool SameDirection(Vector3D a, Vector3D b)
        {
            const double tolerance = 0.001;

            return Math.Abs(a.X - b.X) < tolerance &&
                   Math.Abs(a.Y - b.Y) < tolerance &&
                   Math.Abs(a.Z - b.Z) < tolerance;
        }

        private static Color ToWpfColor(CubeColor color)
        {
            return color switch
            {
                CubeColor.White => Colors.White,
                CubeColor.Yellow => Colors.Yellow,
                CubeColor.Red => Colors.Red,
                CubeColor.Orange => Colors.Orange,
                CubeColor.Blue => Colors.Blue,
                CubeColor.Green => Colors.Green,
                _ => Colors.White
            };
        }

        private static bool AreSameColor(Color a, Color b)
        {
            return a.A == b.A &&
                   a.R == b.R &&
                   a.G == b.G &&
                   a.B == b.B;
        }

        private GeometryModel3D CreateBox(double x, double y, double z, double sizeX, double sizeY, double sizeZ, Color color)
        {
            var builder = new MeshBuilder();

            builder.AddBox(new Point3D(x, y, z).ToVector3(), (float)sizeX, (float)sizeY, (float)sizeZ);

            HelixToolkit.Geometry.MeshGeometry3D mesh = builder.ToMesh();

            var material = new DiffuseMaterial(new SolidColorBrush(color));

            return new GeometryModel3D
            {
                Geometry = mesh.ToWndMeshGeometry3D(),
                Material = material,
                BackMaterial = material
            };
        }

        // Visual cube only
        public void SetDisplayOrientation(CubeColor bottomColor, CubeColor frontColor, HelixViewport3D viewport)
        {
            CurrentOrientation = new CubeOrientation(bottomColor, frontColor);
            displayOrientationTransform = CurrentOrientation.ToTransform();

            foreach (var visual in renderedCubieVisuals)
            {
                visual.Transform = displayOrientationTransform;
            }
        }

        public void ResetToSolved(HelixViewport3D viewport)
        {
            InitializeCubies();
            Render(viewport);
        }

        // Base moves
        public void ApplyMove(ParsedMove move, HelixViewport3D viewport)
        {
            ApplyMoveNoRender(move);
            Render(viewport);
        }

        public void ApplyMove(FaceMove move, int turns, HelixViewport3D viewport)
        {
            ApplyMoveNoRender(move, turns);
            Render(viewport);
        }

        public void ApplyMoveNoRender(ParsedMove move)
        {
            ApplyMoveNoRender(move.Move, move.Turns, move.DoublePrime);
        }

        public void ApplyMoveNoRender(FaceMove move, int turns)
        {
            ApplyMoveNoRender(move, turns, doublePrime: false);
        }

        public void ApplyMoveNoRender(FaceMove move, int turns, bool doublePrime)
        {
            turns = NormalizeTurns(turns);

            if (turns == 0)
                return;

            foreach (QuarterTurn quarterTurn in GetQuarterTurns(move, turns, doublePrime))
            {
                ApplySingleQuarterTurn(quarterTurn.Move, quarterTurn.Inverse);
            }
        }

        public async Task ApplyMoveAnimated(ParsedMove move, HelixViewport3D viewport, int durationMs, CancellationToken cancellationToken)
        {
            await ApplyMoveAnimated(move.Move, move.Turns, viewport, durationMs, move.DoublePrime, cancellationToken);
        }

        public async Task ApplyMoveAnimated(FaceMove move, int turns, HelixViewport3D viewport, int durationMs, bool doublePrime, CancellationToken cancellationToken)
        {
            if (isAnimating)
                return;

            turns = NormalizeTurns(turns);

            if (turns == 0)
                return;

            cancellationToken.ThrowIfCancellationRequested();
            isAnimating = true;

            try
            {
                foreach (QuarterTurn quarterTurn in GetQuarterTurns(move, turns, doublePrime))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await AnimateSingleQuarterTurn(quarterTurn.Move, quarterTurn.Inverse, durationMs, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    ApplySingleQuarterTurn(quarterTurn.Move, quarterTurn.Inverse);

                    Render(viewport);
                }
            }
            finally
            {
                StopVisualAnimations();
                isAnimating = false;
            }
        }

        public void StopVisualAnimations()
        {
            foreach (Cubie cubie in Cubies)
            {
                if (cubie.Visual == null)
                    continue;

                cubie.Visual.Transform = displayOrientationTransform;
            }
        }

        // Display/render stuff
        public void ApplyDisplayedMove(ParsedMove move, HelixViewport3D viewport)
        {
            ApplyMoveNoRender(CurrentOrientation.ToBaseMove(move));
            Render(viewport);
        }

        public async Task ApplyDisplayedMoveAnimated(ParsedMove move, HelixViewport3D viewport, int durationMs, CancellationToken cancellationToken)
        {
            await ApplyMoveAnimated(CurrentOrientation.ToBaseMove(move), viewport, durationMs, cancellationToken);
        }

        private static int NormalizeTurns(int turns)
        {
            return ((turns % 4) + 4) % 4;
        }

        private static List<QuarterTurn> GetQuarterTurns(FaceMove move, int turns, bool doublePrime = false)
        {
            turns = NormalizeTurns(turns);

            var result = new List<QuarterTurn>();

            if (turns == 1)
            {
                result.Add(new QuarterTurn(move, Inverse: false));
            }
            else if (turns == 2)
            {
                if (doublePrime)
                {
                    result.Add(new QuarterTurn(move, Inverse: true));
                    result.Add(new QuarterTurn(move, Inverse: true));
                }
                else
                {
                    result.Add(new QuarterTurn(move, Inverse: false));
                    result.Add(new QuarterTurn(move, Inverse: false));
                }
            }
            else if (turns == 3)
            {
                result.Add(new QuarterTurn(move, Inverse: true));
            }

            return result;
        }

        private void ApplySingleQuarterTurn(FaceMove move, bool inverse)
        {
            foreach (LayerTurn turn in GetLayerTurns(move, inverse))
            {
                ApplyLayerTurn(turn);
            }
        }

        private void ApplyLayerTurn(LayerTurn turn)
        {
            switch (turn.Axis)
            {
                case MoveAxis.X:
                    RotateLayerX(turn.Layer, turn.Clockwise);
                    break;

                case MoveAxis.Y:
                    RotateLayerY(turn.Layer, turn.Clockwise);
                    break;

                case MoveAxis.Z:
                    RotateLayerZ(turn.Layer, turn.Clockwise);
                    break;
            }
        }

        private void RotateLayerX(int layer, bool clockwise)
        {
            Cubie[,] oldLayer = new Cubie[3, 3];

            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    oldLayer[y, z] = Cubies[layer, y, z];

            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Cubie cubie = oldLayer[y, z];

                    int newY;
                    int newZ;

                    if (clockwise)
                    {
                        newY = z;
                        newZ = 2 - y;
                        cubie.RotateAroundXClockwise();
                    }
                    else
                    {
                        newY = 2 - z;
                        newZ = y;
                        cubie.RotateAroundXCounterClockwise();
                    }

                    cubie.X = layer;
                    cubie.Y = newY;
                    cubie.Z = newZ;

                    Cubies[layer, newY, newZ] = cubie;
                }
            }
        }

        private void RotateLayerY(int layer, bool clockwise)
        {
            Cubie[,] oldLayer = new Cubie[3, 3];

            for (int x = 0; x < 3; x++)
                for (int z = 0; z < 3; z++)
                    oldLayer[x, z] = Cubies[x, layer, z];

            for (int x = 0; x < 3; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Cubie cubie = oldLayer[x, z];

                    int newX;
                    int newZ;

                    if (clockwise)
                    {
                        newX = z;
                        newZ = 2 - x;
                        cubie.RotateAroundYClockwise();
                    }
                    else
                    {
                        newX = 2 - z;
                        newZ = x;
                        cubie.RotateAroundYCounterClockwise();
                    }

                    cubie.X = newX;
                    cubie.Y = layer;
                    cubie.Z = newZ;

                    Cubies[newX, layer, newZ] = cubie;
                }
            }
        }

        private void RotateLayerZ(int layer, bool clockwise)
        {
            Cubie[,] oldLayer = new Cubie[3, 3];

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    oldLayer[x, y] = Cubies[x, y, layer];

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Cubie cubie = oldLayer[x, y];

                    int newX;
                    int newY;

                    if (clockwise)
                    {
                        newX = y;
                        newY = 2 - x;
                        cubie.RotateAroundZClockwise();
                    }
                    else
                    {
                        newX = 2 - y;
                        newY = x;
                        cubie.RotateAroundZCounterClockwise();
                    }

                    cubie.X = newX;
                    cubie.Y = newY;
                    cubie.Z = layer;

                    Cubies[newX, newY, layer] = cubie;
                }
            }
        }

        private async Task AnimateSingleQuarterTurn(FaceMove move, bool inverse, int durationMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<LayerTurn> layerTurns = GetLayerTurns(move, inverse);
            List<Cubie> affectedCubies = GetAffectedCubies(layerTurns);

            GetAnimationParameters(layerTurns, out Vector3D axis, out Point3D center, out double angle);

            axis = displayOrientationTransform.Transform(axis);
            center = displayOrientationTransform.Transform(center);

            foreach (Cubie cubie in affectedCubies)
            {
                if (cubie.Visual == null)
                    continue;

                var axisAngle = new AxisAngleRotation3D(axis, 0);
                var rotateTransform = new RotateTransform3D(axisAngle, center);

                var transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(displayOrientationTransform);
                transformGroup.Children.Add(rotateTransform);

                cubie.Visual.Transform = transformGroup;

                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = angle,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    AccelerationRatio = 0.2,
                    DecelerationRatio = 0.2,
                    FillBehavior = FillBehavior.HoldEnd
                };

                axisAngle.BeginAnimation(
                    AxisAngleRotation3D.AngleProperty,
                    animation);
            }

            await Task.Delay(durationMs, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (Cubie cubie in affectedCubies)
            {
                if (cubie.Visual != null)
                    cubie.Visual.Transform = displayOrientationTransform;
            }
        }

        private List<Cubie> GetAffectedCubies(List<LayerTurn> layerTurns)
        {
            var result = new List<Cubie>();

            foreach (LayerTurn turn in layerTurns)
            {
                AddLayerCubies(result, turn.Axis, turn.Layer);
            }

            return result.Distinct().ToList();
        }

        private void AddLayerCubies(List<Cubie> result, MoveAxis axis, int layer)
        {
            switch (axis)
            {
                case MoveAxis.X:
                    for (int y = 0; y < 3; y++)
                        for (int z = 0; z < 3; z++)
                            result.Add(Cubies[layer, y, z]);
                    break;

                case MoveAxis.Y:
                    for (int x = 0; x < 3; x++)
                        for (int z = 0; z < 3; z++)
                            result.Add(Cubies[x, layer, z]);
                    break;

                case MoveAxis.Z:
                    for (int x = 0; x < 3; x++)
                        for (int y = 0; y < 3; y++)
                            result.Add(Cubies[x, y, layer]);
                    break;
            }
        }

        private void GetAnimationParameters(List<LayerTurn> layerTurns, out Vector3D axis, out Point3D center, out double angle)
        {
            if (layerTurns.Count == 0)
            {
                axis = new Vector3D(0, 1, 0);
                center = new Point3D(0, 0, 0);
                angle = 0;
                return;
            }

            LayerTurn first = layerTurns[0];

            axis = first.Axis switch
            {
                MoveAxis.X => new Vector3D(1, 0, 0),
                MoveAxis.Y => new Vector3D(0, 1, 0),
                MoveAxis.Z => new Vector3D(0, 0, 1),
                _ => new Vector3D(0, 1, 0)
            };

            center = first.Axis switch
            {
                MoveAxis.X => new Point3D((first.Layer - 1) * spacing, 0, 0),
                MoveAxis.Y => new Point3D(0, (first.Layer - 1) * spacing, 0),
                MoveAxis.Z => new Point3D(0, 0, (first.Layer - 1) * spacing),
                _ => new Point3D(0, 0, 0)
            };

            if (layerTurns.Count > 1)
                center = new Point3D(0, 0, 0);

            angle = GetAnimationAngle(first.Axis, first.Clockwise);
        }

        private static double GetAnimationAngle(MoveAxis axis, bool clockwise)
        {
            if (axis == MoveAxis.Y)
                return clockwise ? 90 : -90;

            return clockwise ? -90 : 90;
        }

        private List<LayerTurn> GetLayerTurns(FaceMove move, bool inverse)
        {
            List<LayerTurn> turns = move switch
            {
                FaceMove.R => new List<LayerTurn> { new(MoveAxis.X, 2, true) },
                FaceMove.L => new List<LayerTurn> { new(MoveAxis.X, 0, false) },
                FaceMove.U => new List<LayerTurn> { new(MoveAxis.Y, 2, false) },
                FaceMove.D => new List<LayerTurn> { new(MoveAxis.Y, 0, true) },
                FaceMove.F => new List<LayerTurn> { new(MoveAxis.Z, 2, true) },
                FaceMove.B => new List<LayerTurn> { new(MoveAxis.Z, 0, false) },

                FaceMove.Rw => new List<LayerTurn>
                {
                    new(MoveAxis.X, 2, true),
                    new(MoveAxis.X, 1, true)
                },

                FaceMove.Lw => new List<LayerTurn>
                {
                    new(MoveAxis.X, 0, false),
                    new(MoveAxis.X, 1, false)
                },

                FaceMove.Uw => new List<LayerTurn>
                {
                    new(MoveAxis.Y, 2, false),
                    new(MoveAxis.Y, 1, false)
                },

                FaceMove.Dw => new List<LayerTurn>
                {
                    new(MoveAxis.Y, 0, true),
                    new(MoveAxis.Y, 1, true)
                },

                FaceMove.Fw => new List<LayerTurn>
                {
                    new(MoveAxis.Z, 2, true),
                    new(MoveAxis.Z, 1, true)
                },

                FaceMove.Bw => new List<LayerTurn>
                {
                    new(MoveAxis.Z, 0, false),
                    new(MoveAxis.Z, 1, false)
                },

                FaceMove.M => new List<LayerTurn> { new(MoveAxis.X, 1, false) },
                FaceMove.E => new List<LayerTurn> { new(MoveAxis.Y, 1, true) },
                FaceMove.S => new List<LayerTurn> { new(MoveAxis.Z, 1, true) },

                FaceMove.X => new List<LayerTurn>
                {
                    new(MoveAxis.X, 0, true),
                    new(MoveAxis.X, 1, true),
                    new(MoveAxis.X, 2, true)
                },

                FaceMove.Y => new List<LayerTurn>
                {
                    new(MoveAxis.Y, 0, false),
                    new(MoveAxis.Y, 1, false),
                    new(MoveAxis.Y, 2, false)
                },

                FaceMove.Z => new List<LayerTurn>
                {
                    new(MoveAxis.Z, 0, true),
                    new(MoveAxis.Z, 1, true),
                    new(MoveAxis.Z, 2, true)
                },

                _ => new List<LayerTurn>()
            };

            if (!inverse)
                return turns;

            return turns.Select(t => new LayerTurn(t.Axis, t.Layer, !t.Clockwise)).ToList();
        }

        private enum MoveAxis
        {
            X,
            Y,
            Z
        }

        private readonly record struct LayerTurn(MoveAxis Axis, int Layer, bool Clockwise);

        private readonly record struct QuarterTurn(FaceMove Move, bool Inverse);
    }
}