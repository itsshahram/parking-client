using Parking.Domain.Entities.Parkings;

namespace Parking.WebApi.Helpers.PriceCalculation;

/// <summary>
/// کلاس محاسبه قیمت پارکینگ.
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;

public class ParkingCostCalculator
{
    private const int MinutesPerHour = 60;
    private const int MinutesPerDay = 1440;

    private int EntryFee { get; }
    private int DailyRate { get; }
    private int FreeMinutes { get; }
    private int ThresholdNumberOfDays { get; }
    private decimal DailyPriceAfterCrossingThreshold { get; }
    private int ThresholdHoursPerDay { get; }
    private decimal DiscountPercentage { get; }
    private decimal TaxPercentage { get; }
    private IReadOnlyList<ParkingVehicleSegmentPrice> HourlyRates { get; }
    private IReadOnlyList<ParkingVehicleSegmentVariablePrice> VariablePriceList { get; }

    public ParkingCostCalculator(
        int entryFee,
        int dailyRate,
        int freeMinutes,
        int thresholdNumberOfDays,
        decimal dailyPriceAfterCrossingThreshold,
        int thresholdHoursPerDay,
        decimal discountPercentage,
        decimal taxPercentage,
        List<ParkingVehicleSegmentPrice> hourlyRates,
        List<ParkingVehicleSegmentVariablePrice> variablePriceList)
    {
        EntryFee = entryFee;
        DailyRate = dailyRate;
        FreeMinutes = freeMinutes;
        ThresholdNumberOfDays = thresholdNumberOfDays;
        DailyPriceAfterCrossingThreshold = dailyPriceAfterCrossingThreshold;
        ThresholdHoursPerDay = thresholdHoursPerDay > 0 ? thresholdHoursPerDay : 1; // حداقل 1 ساعت
        DiscountPercentage = discountPercentage;
        TaxPercentage = taxPercentage;
        HourlyRates = hourlyRates?.ToList() ?? [];
        VariablePriceList = variablePriceList?
                                .OrderBy(v => v.Number)
                                .ToList() 
                            ?? [];
    }

    public ParkingPriceOutputResult CalculateCost(DateTime entryTime, DateTime exitTime)
    {
        if (exitTime <= entryTime)
            throw new ArgumentException("زمان خروج باید بعد از زمان ورود باشد.");

        var totalDuration = exitTime - entryTime;
        var totalMinutes = (int)totalDuration.TotalMinutes;

        // اگر زمان پارکینگ کمتر یا مساوی زمان رایگان باشد
        if (totalMinutes <= FreeMinutes)
        {
            return CreateZeroCostOutput(totalDuration);
        }

        // کسر زمان ورود (اگر هزینه ورود وجود داشته باشد، معمولاً 60 دقیقه اول را کسر می‌کند)
        var billableMinutes = totalMinutes;
        if (EntryFee > 0)
        {
            billableMinutes -= MinutesPerHour;
            if (billableMinutes < 0) billableMinutes = 0;
        }

        decimal baseCost = EntryFee;

        // محاسبه هزینه روزهاي کامل
        var fullDays = totalDuration.Days;
        billableMinutes = (int)SubtractFullDaysCost(ref baseCost, fullDays, billableMinutes);

        // اگر آستانه ساعتی روزانه فعال باشد و ساعت‌های باقی‌مانده بیشتر از آستانه باشد → یک روز کامل حساب شود
        var remainingMinutesAfterDaily = billableMinutes;
        if (ThresholdHoursPerDay > 0 && (remainingMinutesAfterDaily / MinutesPerHour) >= ThresholdHoursPerDay)
        {
            baseCost += DailyRate;
            remainingMinutesAfterDaily = 0;
        }

        // محاسبه هزینه ساعات باقی‌مانده (کمتر از یک روز کامل)
        if (fullDays < 1 && remainingMinutesAfterDaily > 0)
        {
            baseCost += CalculateVariableOrHourlyCost(entryTime, remainingMinutesAfterDaily);
        }
        else if (remainingMinutesAfterDaily > 0)
        {
            // اگر روز کامل داشته باشیم، فقط تعرفه ساعتی اعمال می‌شود (نه متغیر)
            baseCost += CalculateHourlyCost(entryTime, totalMinutes - remainingMinutesAfterDaily, remainingMinutesAfterDaily);
        }

        // اعمال تخفیف و مالیات
        var discountAmount = baseCost * (DiscountPercentage / 100m);
        var amountAfterDiscount = baseCost - discountAmount;
        var taxAmount = amountAfterDiscount * (TaxPercentage / 100m);
        var payableAmount = amountAfterDiscount + taxAmount;

        return new ParkingPriceOutputResult
        {
            TotalWithoutDiscount = baseCost,
            DiscountAmount = discountAmount,
            PayableAmount = payableAmount,
            TotalDays = fullDays,
            TotalHours = totalDuration.Hours % ThresholdHoursPerDay,
            TotalMinutes = totalDuration.Minutes
        };
    }

    private decimal SubtractFullDaysCost(ref decimal cost, int fullDays, int billableMinutes)
    {
        if (fullDays <= 0) return billableMinutes;

        decimal dailyRateToUse;
        if (ThresholdNumberOfDays > 0 && fullDays > ThresholdNumberOfDays)
        {
            dailyRateToUse = DailyPriceAfterCrossingThreshold;
        }
        else
        {
            dailyRateToUse = DailyRate;
        }

        cost += fullDays * dailyRateToUse;
        return billableMinutes - fullDays * MinutesPerDay;
    }

    private decimal CalculateVariableOrHourlyCost(DateTime entryTime, int minutesToCharge)
    {
        // بررسی اینکه آیا در بازه زمانی متغیر هستیم؟
        var variableSegment = HourlyRates.FirstOrDefault(s => s.IsVariableEnable);
        if (variableSegment != null &&
            entryTime.TimeOfDay >= variableSegment.TimeFrom.ToTimeSpan() &&
            entryTime.TimeOfDay <= variableSegment.TimeTo.ToTimeSpan())
        {
            return CalculateVariablePrice(minutesToCharge);
        }

        // در غیر این صورت تعرفه ساعتی معمولی
        return CalculateHourlyCost(entryTime, 0, minutesToCharge);
    }

    private decimal CalculateVariablePrice(int minutes)
    {
        var cost = 0m;
        var remaining = minutes;

        foreach (var segment in VariablePriceList)
        {
            if (remaining <= 0) break;

            if (remaining > segment.Minutes)
            {
                cost += segment.Price ?? 0m;
                remaining -= segment.Minutes;
            }
            else
            {
                cost += segment.Price ?? 0m;
                remaining = 0;
            }
        }

        return cost;
    }

    private decimal CalculateHourlyCost(DateTime startTime, int alreadyChargedMinutes, int minutesToCharge)
    {
        var cost = 0m;
        var remaining = minutesToCharge;
        var current = startTime.AddMinutes(alreadyChargedMinutes);

        while (remaining > 0)
        {
            var rate = HourlyRates.FirstOrDefault(r =>
                current.TimeOfDay >= r.TimeFrom.ToTimeSpan() &&
                current.TimeOfDay < r.TimeTo.ToTimeSpan());

            if (rate == null) break;

            cost += rate.HourlyRate;
            current = current.AddHours(1);
            remaining -= MinutesPerHour;
        }

        return cost;
    }

    private static ParkingPriceOutputResult CreateZeroCostOutput(TimeSpan duration)
    {
        return new ParkingPriceOutputResult
        {
            TotalWithoutDiscount = 0,
            DiscountAmount = 0,
            PayableAmount = 0,
            TotalDays = 0,
            TotalHours = duration.Hours,
            TotalMinutes = duration.Minutes
        };
    }
}