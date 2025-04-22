using System;
using System.Text.RegularExpressions;

public static class PlateParser
{

    public static (bool IsIranianPlate, string LeftTwoDigits, string Letter, string RightThreeDigits, string IranCode, string OriginalPlate) ParsePlate(this string plate)
    {
        // الگوی فرمت پلاک ایرانی
        string pattern = @"^(\d{2})_([a-zA-Zآ-ی]{1,3})_(\d{3})_(IR|\u0627\u06CC\u0631\u0627\u0646)(\d{2})$";

        var match = Regex.Match(plate, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            string leftTwoDigits = match.Groups[1].Value;   // دو رقم سمت چپ
            string letter = match.Groups[2].Value;         // حرف
            string rightThreeDigits = match.Groups[3].Value; // سه رقم سمت راست
            string iranCode = match.Groups[4].Value + match.Groups[5].Value; // کد ایران (IR یا ایران)

            return (true, leftTwoDigits, letter, rightThreeDigits, iranCode, plate);
        }
        else
        {
            // در صورت عدم تطابق با فرمت ایرانی، بخش‌های پلاک خالی خواهند بود
            return (false, string.Empty, string.Empty, string.Empty, string.Empty, plate);
        }
    }
}