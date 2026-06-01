using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CubeForge.Trainers.Shared
{
    public static class ResourceLoader
    {
        public static Stream OpenRead(string relativePath)
        {
            if (Path.IsPathRooted(relativePath) && File.Exists(relativePath))
                return File.OpenRead(relativePath);

            string cleanPath = relativePath.Replace("\\", "/").TrimStart('/');
            string diskPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(diskPath))
                return File.OpenRead(diskPath);

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceSuffix = cleanPath.Replace("/", ".");

            string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
                throw new FileNotFoundException($"Could not find resource: {relativePath}");

            Stream? stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
                throw new FileNotFoundException($"Could not open resource: {relativePath}");

            return stream;
        }

        public static string ReadText(string relativePath)
        {
            using Stream stream = OpenRead(relativePath);
            using StreamReader reader = new(stream);

            return reader.ReadToEnd();
        }
    }
}