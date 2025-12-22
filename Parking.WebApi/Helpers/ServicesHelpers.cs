using System.Net;
using System.Text.RegularExpressions;

namespace Parking.WebApi.Helpers;

public static class ServicesHelpers
{
    
    private static (bool IsIranianPlate, string LeftTwoDigits, string Letter, string RightThreeDigits, string IranCode, string OriginalPlate)
        PlateParse(string plate)
    {
        const string pattern = @"^(?<left>\d{2})_(?<letter>[a-zA-Zآ-ی]{1,3})_(?<right>\d{3})_(?<iran>IR|ایران)(?<code>\d{2})$";

        var match = Regex.Match(plate, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return (
                true,
                match.Groups["left"].Value,
                match.Groups["letter"].Value,
                match.Groups["right"].Value,
                match.Groups["iran"].Value + match.Groups["code"].Value,
                plate
            );
        }

        return (false, string.Empty, string.Empty, string.Empty, string.Empty, plate);
    }
    
    public static (string FaLicensePlate, string EnLicensePlate) RefineLicensePlate(string plate)
    {
        string faLicensePlate;
        string enLicensePlate;
        
        if (plate.Contains('_'))
        { 
            var parsedPlate = PlateParse(plate); 
            faLicensePlate = 
                "ایران" +
                parsedPlate.IranCode.Replace("IR", "") + 
                "_" +
                parsedPlate.RightThreeDigits + 
                parsedPlate.Letter.ToLower()?.ConvertEnCharToFaCharIndex().Replace("ه", "هـ") + $"{parsedPlate.LeftTwoDigits}";
            
            enLicensePlate = plate;
        }
        else
        {
            faLicensePlate = plate;
            enLicensePlate = plate;
        }

        return (faLicensePlate, enLicensePlate);
    }
    
    public static long GenerateRandomBarcodeId()
    {
        var firstPart = Random.Shared.Next(1000, 10000);
        var lastDigit = Random.Shared.Next(1, 10);
        var middlePart = (long)Random.Shared.Next(0, 100_000_000) * 10;

        return firstPart * 1_000_000_000_000L + middlePart + lastDigit;
    }
    
    public static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ipAddr in host.AddressList)
        {
            if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ipAddr.ToString();
            }
        }
        return string.Empty;
    }
}