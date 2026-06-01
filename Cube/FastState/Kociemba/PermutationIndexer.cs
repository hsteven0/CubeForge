using System;

namespace CubeForge.Cube.FastState.Kociemba
{
    // This class converts permutations to compact integer coords (and/or reverse) 
    // - Used for Kociemba coordinate indexing and pruning cache
    public static class PermutationIndexer
    {
        private static readonly int[] Factorials =
        {
            1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800, 479001600
        };

        public static int Factorial(int n)
        {
            if (n < 0 || n >= Factorials.Length) // Make sure n is in range
                throw new ArgumentOutOfRangeException(nameof(n));

            return Factorials[n];
        }

        public static int Rank(int[] permutation)
        {
            int n = permutation.Length;
            bool[] used = new bool[n];
            int rank = 0;

            for (int i = 0; i < n; i++)
            {
                int value = permutation[i];

                int smallerUnused = 0;

                for (int j = 0; j < value; j++)
                {
                    if (!used[j])
                        smallerUnused++;
                }

                rank += smallerUnused * Factorial(n - i - 1);
                used[value] = true;
            }

            return rank;
        }

        public static int[] Unrank(int n, int rank)
        {
            int[] result = new int[n];
            bool[] used = new bool[n];

            for (int i = 0; i < n; i++)
            {
                int f = Factorial(n - i - 1);
                int index = rank / f;
                rank %= f;

                int value = SelectUnusedValue(used, index);
                result[i] = value;
                used[value] = true;
            }

            return result;
        }

        private static int SelectUnusedValue(bool[] used, int index)
        {
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i])
                    continue;

                if (index == 0)
                    return i;

                index--;
            }

            // stop here and throw exception if the selection is bad
            throw new InvalidOperationException("Could not select unused value.");
        }
    }
}
