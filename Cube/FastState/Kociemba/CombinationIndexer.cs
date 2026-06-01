using System;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class CombinationIndexer
    {
        private static readonly int[,] binomial = BuildBinomialTable(13, 13);

        private static int[,] BuildBinomialTable(int nMax, int kMax)
        {
            int[,] table = new int[nMax + 1, kMax + 1];
            for (int n = 0; n <= nMax; n++)
            {
                table[n, 0] = 1;
                table[n, n] = 1;

                for (int k = 1; k < n; k++)
                    table[n, k] = table[n - 1, k - 1] + table[n - 1, k];
            }
            return table;
        }

        public static int Choose(int n, int k)
        {
            if (k < 0 || k > n)
                return 0;

            return binomial[n, k];
        }

        public static int Rank(bool[] selected, int k)
        {
            int n = selected.Length;
            int rank = 0;
            int remaining = k;

            for (int i = 0; i < n; i++)
            {
                if (remaining == 0)
                    break;

                if (selected[i])
                {
                    remaining--;
                }
                else
                {
                    rank += Choose(n - i - 1, remaining - 1);
                }
            }

            return rank;
        }

        public static bool[] Unrank(int n, int k, int rank)
        {
            bool[] selected = new bool[n];
            int remaining = k;

            for (int i = 0; i < n; i++)
            {
                if (remaining == 0)
                    break;

                int countIfSelected = Choose(n - i - 1, remaining - 1);

                if (rank < countIfSelected)
                {
                    selected[i] = true;
                    remaining--;
                }
                else
                {
                    rank -= countIfSelected;
                }
            }

            return selected;
        }
    }
}
