using System;
using System.IO;
using System.IO.Compression;

namespace CubeForge.Trainers.Shared
{
    public static class PruningTableCompressor
    {
        public static void CompressFolder(string folder)
        {
#if DEBUG
            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Folder not found: {folder}");
                return;
            }

            string[] files = Directory.GetFiles(folder, "*.bin", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                CompressFile(file, deleteOriginal: false);
            }

            Console.WriteLine($"Compressed {files.Length} pruning table files.");
#endif
        }

        public static void CompressFile(string sourcePath, bool deleteOriginal)
        {
#if DEBUG
            if (!File.Exists(sourcePath))
                return;

            if (!sourcePath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                return;

            string outputPath = sourcePath + ".gz";

            if (File.Exists(outputPath))
            {
                DateTime sourceWriteTime = File.GetLastWriteTimeUtc(sourcePath);
                DateTime gzWriteTime = File.GetLastWriteTimeUtc(outputPath);

                if (gzWriteTime >= sourceWriteTime)
                    return;
            }

            using FileStream input = File.OpenRead(sourcePath);
            using FileStream output = File.Create(outputPath);
            using GZipStream gzip = new GZipStream(output, CompressionLevel.SmallestSize);

            input.CopyTo(gzip);

            if (deleteOriginal)
                File.Delete(sourcePath);

            Console.WriteLine($"Compressed: {Path.GetFileName(sourcePath)}");
#endif
        }

        public static Stream OpenRead(string path)
        {
            if (File.Exists(path))
                return File.OpenRead(path);

            string gzPath = path + ".gz";

            if (File.Exists(gzPath))
            {
                FileStream fileStream = File.OpenRead(gzPath);
                return new GZipStream(fileStream, CompressionMode.Decompress);
            }

            throw new FileNotFoundException("Pruning table file was not found.", path);
        }

        public static bool Exists(string path)
        {
            return File.Exists(path) || File.Exists(path + ".gz");
        }
    }
}