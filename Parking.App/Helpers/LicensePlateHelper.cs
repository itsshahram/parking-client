using Parking.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public static class LicensePlateHelper
{
    private static string[] charclassnames_fa = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "ب", "پ", "ت", "ث", "ق", "ف", "گ", "ک", "ل", "د", "م", "ن", "و", "معلولین", "س", "ش", "ص", "هـ", "ط", "ظ", "ع", "ی", "ج", "الف", "ز"];

    private static string[] charclassnames_en = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "p", "t", "se", "gh", "f", "g", "k", "l", "de", "m", "n", "v", "zh", "sin", "sh", "sad", "h", "ta", "za", "ain", "y", "jim", "alef", "z"];

    public static string ConvertToLicenseEnCharModel(this int index) {

        return charclassnames_en[index];
    }
    public static string ConvertToLicenseFaCharModel(this int index)
    {

        return charclassnames_fa[index];
    }
    public static int ConvertFaCharToLicenseIndex(this string character)
    {

        return charclassnames_fa.ToList().IndexOf(character);
    }
    public static string ConvertFaCharToEnCharIndex(this string character)
    {

        int index = charclassnames_fa.ToList().IndexOf(character);


        return charclassnames_en[index];
    }
    public static string ConvertEnCharToFaCharIndex(this string character)
    {
        int index = charclassnames_en.ToList().IndexOf(character);

        return charclassnames_fa[index];
    }
    public static List<LicensePlateStringModel> GetChars()
    {

       //string[] charclassnames_fa = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "ب", "پ", "ت", "ث", "ق", "ف", "گ", "ک", "ل", "د", "م", "ن", "و", "معلولین", "س", "ش", "ص", "هـ", "ط", "ظ", "ع", "ی", "ج", "الف", "ز"];

    List<LicensePlateStringModel> result = new List<LicensePlateStringModel>();

        for (int i=11; i<charclassnames_fa.Length; i++)
        {
            result.Add(new LicensePlateStringModel() { Index = i, PlateFa = charclassnames_fa[i], PlateEn = charclassnames_en[i] });
        }
        return result;
    }
    public static bool IsValidCarLicensePlateFormat(string input)
    {
        // الگوی عبارت منظم برای تطبیق رشته‌های موردنظر
        string pattern = @"^ایران\d{2}_.{3}\d{2}$";

        // استفاده از Regex برای تطبیق الگو
        Regex regex = new Regex(pattern);
        return regex.IsMatch(input);
    }
    // "12_h_143_IR50"
    public static bool IsValidCarEnLicensePlateFormat(string input)
    {

        // الگوی عبارت منظم برای تطبیق رشته‌های موردنظر
        string pattern = @"^\d{2}_[a-z]+_\d{3}_IR\d{2}$";

        // استفاده از Regex برای تطبیق الگو
        Regex regex = new Regex(pattern);
        return regex.IsMatch(input);
    }
    public static bool AreAllCharactersDigits(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (char c in input)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
