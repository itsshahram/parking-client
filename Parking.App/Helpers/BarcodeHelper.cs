using ZXing;
using ZXing.Common;

namespace Parking.App.Helpers;
public static class BarcodeHelper
{
    public static BitmapImage GenerateBarcode(string text)
    {
        var writer = new ZXing.Windows.Compatibility.BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = 200,
                Height = 70,
                Margin = 10
            }
        };

        var bitmap = writer.Write(text);
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }

}

