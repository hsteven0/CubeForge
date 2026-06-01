namespace CubeForge.Views
{
    public class ScrambleEntry<TCase>
    {
        public TCase Case { get; set; } = default!;
        public string AlgorithmText { get; set; } = "";
        public string ScrambleText { get; set; } = "";
    }
}
