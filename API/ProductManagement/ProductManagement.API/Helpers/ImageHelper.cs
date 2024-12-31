using System.Drawing;
using System.Drawing.Imaging;

namespace ProductManagement.API.Helpers;

public class ImageHelper
{
    public static bool ValidateImage(string path)
    {
        try
        {
            var candidateImage = new Bitmap(path);
            candidateImage!.Dispose();

            return candidateImage != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void ResizeImage(string path)
    {
        if (File.Exists(path))
        {
            var image = Image.FromFile(path);

            if (image.Width > newWidth)
            {
                var newHeight = image.Height * 1000 / image.Width;
                var newImage = new Bitmap(image, new Size(newWidth, newHeight));
                image.Dispose();
                newImage.Save(path, encoder, encParams);
            }
        }
    }

    private static readonly int newWidth = 1000;
    private static readonly long ImageQuality = 75;
    private static readonly ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
    private static readonly EncoderParameters encParams = new() { Param = new[] { new EncoderParameter(Encoder.Quality, ImageQuality) } };
}
