namespace Parking.App.Helpers;

public static class StringValidator
{
    public static bool IsNumeric(this string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
    }
    public static bool IsMobile(this string input)
    {
        return !string.IsNullOrEmpty(input) &&
               input.Length == 11 &&
               input.StartsWith("09") &&
               input.IsNumeric();
            }
}
