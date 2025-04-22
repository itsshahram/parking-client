
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;

namespace Parking.App.Helpers;

public static class ImageHelper
{
    public static  BitmapImage ConvertBitmapToBitmapImage(this System.Drawing.Bitmap bitmap)
    {
        using (var memoryStream = new System.IO.MemoryStream())
        {
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }


    public static string ResizeAndCompressBitmap(this Bitmap source, int width, int height, float dpiX, float dpiY, long quality)
    {
        Bitmap resizedBitmap = new Bitmap(width, height);

        resizedBitmap.SetResolution(dpiX, dpiY);

        using (Graphics graphics = Graphics.FromImage(resizedBitmap))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(source, 0, 0, width, height);
        }

        // تنظیم پارامترهای فشرده‌سازی
        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

        System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
        EncoderParameters encoderParameters = new EncoderParameters(1);

        // پایین آوردن کیفیت برای فشرده‌سازی
        EncoderParameter encoderParameter = new EncoderParameter(qualityEncoder, quality);
        encoderParameters.Param[0] = encoderParameter;

        //Save
        using (MemoryStream memoryStream = new MemoryStream())
        {
            resizedBitmap.Save(memoryStream, jpgEncoder, encoderParameters);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }
    public static string ResizeAndCompressBitmap(this BitmapSource source, int width, int height, float dpiX, float dpiY, int quality)
    {
        Bitmap bitmap = BitmapFromSource(source);

        Bitmap resizedBitmap = new Bitmap(width, height);
        resizedBitmap.SetResolution(dpiX, dpiY);

        using (Graphics graphics = Graphics.FromImage(resizedBitmap))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(bitmap, 0, 0, width, height);
        }

        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
        System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
        EncoderParameters encoderParameters = new EncoderParameters(1);
        EncoderParameter encoderParameter = new EncoderParameter(qualityEncoder, quality);
        encoderParameters.Param[0] = encoderParameter;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            resizedBitmap.Save(memoryStream, jpgEncoder, encoderParameters);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }

    private static Bitmap BitmapFromSource(BitmapSource source)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);

            return new Bitmap(memoryStream);
        }
    }
    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }
    public static ImageSource BitmapToImageSource(this Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
    public static BitmapSource ConvertBitmapToBitmapSource(this System.Drawing.Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            bitmap.PixelFormat);

        var bitmapSource = BitmapSource.Create(
            bitmap.Width,
            bitmap.Height,
            bitmap.HorizontalResolution,
            bitmap.VerticalResolution,
            System.Windows.Media.PixelFormats.Bgr24,
            null,
            bitmapData.Scan0,
            bitmapData.Stride * bitmap.Height,
            bitmapData.Stride);

        bitmap.UnlockBits(bitmapData);
        return bitmapSource;
    }

    public static BitmapImage? ToImageSource(this byte[] bitmap)
    {
        using (var stream = new MemoryStream(bitmap))
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return (BitmapImage?)bitmapImage;
        }

    }
    public static ImageSource Base64ToImageSource(string base64)
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64.Contains(",") ? base64.Split(',')[1] : base64);
            using var ms = new System.IO.MemoryStream(imageBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
    public static string ImageSourceToBase64(this ImageSource imageSource)
    {
        if (imageSource == null)
            throw new ArgumentNullException(nameof(imageSource));

        BitmapSource bitmap = imageSource as BitmapSource;
        if (bitmap == null)
            throw new ArgumentException("Invalid ImageSource format.");

        using MemoryStream memoryStream = new MemoryStream();
        BitmapEncoder encoder = new PngBitmapEncoder(); // Change to JpegBitmapEncoder if needed
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(memoryStream);

        byte[] imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }
    public static string BitmapToBase64(this Bitmap bitmap)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); 
            byte[] imageBytes = ms.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }


}
