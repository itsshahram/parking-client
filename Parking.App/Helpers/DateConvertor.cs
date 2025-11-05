using System.Diagnostics.Contracts;
using System.Globalization;

namespace Parking.App.Helpers;
public static class DateConvertor
{
    public static DateTime ShamsiToDateTime(int persianYear, int persianMonth, int persianDay)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        return persianCalendar.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);
    }
    public static DateTime ShamsiToDateTime(int persianYear, int persianMonth, int persianDay, int hours, int minutes)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        return persianCalendar.ToDateTime(persianYear, persianMonth, persianDay, hours, minutes, 0, 0);
    }
    public static DateTime? SafeShamsiToDateTime(string shamsi)
    {
        if (string.IsNullOrWhiteSpace(shamsi)) return null;

        var parts = shamsi.Split('/');
        if (parts.Length != 3) return null;

        if (!int.TryParse(parts[0], out int year)) return null;
        if (!int.TryParse(parts[1], out int month)) return null;
        if (!int.TryParse(parts[2], out int day)) return null;

        try
        {
            return DateConvertor.ShamsiToDateTime(year, month, day);
        }
        catch
        {
            return null;
        }
    }


    public static string ToShamsi(this DateTime value, bool includeTime = false)
    {
        if (value == default || value == DateTime.MinValue)
            return string.Empty;

        var pc = new PersianCalendar();
        string datePart = $"{pc.GetYear(value)}/{pc.GetMonth(value):00}/{pc.GetDayOfMonth(value):00}";

        if (includeTime)
        {
            string timePart = $"{pc.GetHour(value):00}:{pc.GetMinute(value):00}";
            return $"{datePart} {timePart}";
        }

        return datePart;
    }

    public static string ToLongShamsiString(this DateTime? Value)
    {
        PersianCalendar pc = new PersianCalendar();
        if (Value == null)
        {
            return "";
        }
        try
        {
            return WeekDay((int)pc.GetDayOfWeek((DateTime)Value)) + " " + pc.GetDayOfMonth((DateTime)Value) + " " + PersianMounth(pc.GetMonth((DateTime)Value)) + " " + pc.GetYear((DateTime)Value);

        }
        catch
        {
            return "";
        }


    }
    public static string ToLongShamsiString(this DateTime Value)
    {
        PersianCalendar pc = new PersianCalendar();
        try
        {
            var a = WeekDay((int)pc.GetDayOfWeek((DateTime)Value)) + " " + pc.GetDayOfMonth((DateTime)Value) + " " + PersianMounth(pc.GetMonth((DateTime)Value)) + " " + pc.GetYear((DateTime)Value);
            return a;
        }
        catch
        {
            return "-";
        }


    }
    public static string ToRelativeDate(this DateTime theDate)
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(DateTime.Now.Ticks - theDate.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 1 * MINUTE)
            return ts.Seconds == 1 ? "1 ثانیه قبل" : ts.Seconds + " ثانیه قبل";

        if (delta < 2 * MINUTE)
            return "1 دقیقه قبل";

        if (delta < 45 * MINUTE)
            return ts.Minutes + " دقیقه قبل";

        if (delta < 90 * MINUTE)
            return "1 ساعت قبل";

        if (delta < 24 * HOUR)
            return ts.Hours + " ساعت قبل";

        if (delta < 48 * HOUR)
            return "دیروز";

        if (delta < 30 * DAY)
            return ts.Days + " روز قبل";

        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return months <= 1 ? "1 ماه قبل" : months + " ماه قبل";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "1 سال قبل" : years + " سال قبل";
        }
    }


    private static string WeekDay(int i)
    {
        switch (i)
        {

            case 0:
                return "یکشنبه";
            case 1:
                return "دوشنبه";
            case 2:
                return "سه شنبه";
            case 3:
                return "چهارشنبه";
            case 4:
                return "پنج شنبه";
            case 5:
                return "جمعه";
            case 6:
                return "شنبه";
            default:
                return "";
        }
    }
    private static string PersianMounth(int i)
    {
        switch (i)
        {
            case 1:
                return "فروردین";
            case 2:
                return "اردیبهشت";
            case 3:
                return "خرداد";
            case 4:
                return "تیر";
            case 5:
                return "مرداد";
            case 6:
                return "شهریور";
            case 7:
                return "مهر";
            case 8:
                return "آبان";
            case 9:
                return "آذر";
            case 10:
                return "دی";
            case 11:
                return "بهمن";
            case 12:
                return "اسفند";
            default:
                return "";
        }
    }


}

