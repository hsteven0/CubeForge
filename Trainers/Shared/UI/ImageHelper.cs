using System.IO;
using System.Windows.Media.Imaging;

namespace CubeForge.Trainers.Shared
{
    public sealed class ImageHelper
    {
        public BitmapImage? LoadImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            try
            {
                using Stream stream = ResourceLoader.OpenRead(relativePath);

                BitmapImage image = new();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();

                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}