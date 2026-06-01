namespace CubeForge.Cube.FastState.Kociemba
{
    public readonly record struct Phase1Coordinate(int CornerOrient, int EdgeOrient, int UDSliceComb);
    public readonly record struct Phase2Coordinate(int CornerPerm, int UDLayerPerm, int ESlicePerm);
}
