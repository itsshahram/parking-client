using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public static class PriceRoundingHelper
{
    /// <summary>
    /// رند کردن قیمت به نزدیک‌ترین عدد صحیح و حذف اعشار
    /// </summary>
    /// <param name="price">قیمت اولیه</param>
    /// <returns>قیمت رند شده</returns>
    public static decimal RoundAndRemoveDecimals(this decimal price)
    {
        return Math.Round(price, 0);
    }

    /// <summary>
    /// رند کردن قیمت به پایین‌ترین عدد صحیح و حذف اعشار
    /// </summary>
    /// <param name="price">قیمت اولیه</param>
    /// <returns>قیمت رند شده</returns>
    public static decimal FloorAndRemoveDecimals(this decimal price)
    {
        return Math.Floor(price);
    }

    /// <summary>
    /// رند کردن قیمت به بالاترین عدد صحیح و حذف اعشار
    /// </summary>
    /// <param name="price">قیمت اولیه</param>
    /// <returns>قیمت رند شده</returns>
    public static decimal CeilingAndRemoveDecimals(decimal price)
    {
        return Math.Ceiling(price);
    }
}

