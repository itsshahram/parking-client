using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Utilities;

public static class FetchImage
{
    private static readonly HttpClientHandler handler = new HttpClientHandler { PreAuthenticate = true, UseDefaultCredentials = false };
    private static readonly HttpClient client = new HttpClient(handler);

    public static async Task<BitmapImage> FetchImageWithDigestAuthAsync(string url, string username, string password)
    {
        try
        {
            var initialResponse = await client.GetAsync(url);
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Invalid URL format.");
            }

            if (initialResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var wwwAuthenticateHeader = initialResponse.Headers.WwwAuthenticate.ToString();
                if (string.IsNullOrEmpty(wwwAuthenticateHeader))
                {
                    throw new Exception("Authentication header is missing.");
                }

                var digestHeader = new DigestHeader(wwwAuthenticateHeader, username, password, url, "GET");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Digest", digestHeader.GetDigestHeaderValue());

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await ConvertStreamToBitmapImage(response);
            }
            else
            {
                return await ConvertStreamToBitmapImage(initialResponse);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching image: {ex.Message}");
            return new BitmapImage();
        }
    }

    private static async Task<BitmapImage> ConvertStreamToBitmapImage(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Console.WriteLine($"Image Content-Type: {contentType}");

        if (contentType != "image/jpeg" && contentType != "image/png")
        {
            throw new NotSupportedException($"Unsupported image format: {contentType}");
        }
        using var memoryStream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = memoryStream;
        bitmap.EndInit();
        bitmap.Freeze(); // جلوگیری از مشکلات UI

        // بارگذاری مجدد تصویر بدون متادیتا
        var finalBitmap = new BitmapImage();
        finalBitmap.BeginInit();
        finalBitmap.CacheOption = BitmapCacheOption.OnLoad;
        finalBitmap.StreamSource = new MemoryStream(memoryStream.ToArray()); // حذف متادیتا
        finalBitmap.EndInit();
        finalBitmap.Freeze();

        return finalBitmap;
    }
}
