using Parking.Domain.Entities.Parkings;


namespace Parking.App.Utilities.PriceCalculation;

/// <summary>
/// کلاس محاسبه قیمت پارکینگ.
/// </summary>
public class ParkingCostCalculator
{
    private readonly int entryFee;
    private readonly int dailyRate;
    private readonly int freeMinutes;
    private readonly int thresholdNumberOfDays;
    private readonly decimal dailyPriceAfterCrossingThreshold;
    private readonly decimal discountPercentage;
    private readonly decimal taxPercentage;
    private int thresholdHoursPerDay;
    private const int MinutesPerDay = 1440;

    private List<ParkingVehicleSegmentPrice> hourlyRates;
    private List<ParkingVehicleSegmentVariablePrice> variablePriceList;

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
        this.entryFee = entryFee;
        this.dailyRate = dailyRate;
        this.freeMinutes = freeMinutes;
        this.thresholdNumberOfDays = thresholdNumberOfDays;
        this.dailyPriceAfterCrossingThreshold = dailyPriceAfterCrossingThreshold;
        this.thresholdHoursPerDay = thresholdHoursPerDay;
        this.discountPercentage = discountPercentage;
        this.taxPercentage = taxPercentage;
        this.hourlyRates = hourlyRates;
        this.variablePriceList = variablePriceList;
    }

    public ParkingPriceOutputModel CalculateCost(DateTime entryTime, DateTime exitTime)
    {
        TimeSpan totalTime = exitTime - entryTime;
        int totalMinutes = (int)totalTime.TotalMinutes;
        decimal totalCost = entryFee; // هزینه اولیه
        int totalDays = (int)totalTime.Days;

        if (totalMinutes <= freeMinutes)
        {
            return new ParkingPriceOutputModel
            {
                TotalWithoutDiscount = 0,
                DiscountAmount = 0,
                PayableAmount = 0,
                TotalDays = 0,
                TotalHours = totalTime.Hours,
                TotalMinutes = totalTime.Minutes
            };
        }
        if (totalMinutes <= 60 && entryFee > 0 && !hourlyRates.Any())
        {
            totalMinutes = totalMinutes - 60;
            if (totalMinutes < 0)
                totalMinutes = 0;
        }

        // محاسبه هزینه برای زمان بیشتر از یک روز
        if (totalDays >= 1)
        {
            if (thresholdNumberOfDays > 0)
            {
                if ((int)totalTime.TotalDays > thresholdNumberOfDays)
                {
                    totalDays = (int)totalTime.TotalDays;
                    totalCost += totalDays * dailyPriceAfterCrossingThreshold;
                    totalMinutes -= totalDays * MinutesPerDay;
                    totalTime.Add(TimeSpan.FromDays(-totalDays));
                }
                else
                {
                    totalCost += totalDays * dailyRate;
                    totalMinutes -= totalDays * MinutesPerDay; // کم کردن دقیقه‌های روزها
                }
            }
            else
            {
                totalCost += totalDays * dailyRate;
                totalMinutes -= totalDays * MinutesPerDay; // کم کردن دقیقه‌های روزها
            }

        }
        // محاسبه هزینه برای زمان بیشتر از آستانه
        int remainingMinutes = totalMinutes;
        if (thresholdHoursPerDay > 0)
        {
            if (remainingMinutes > 1)
            {
                if ((totalMinutes / 60) >= thresholdHoursPerDay)
                {
                    totalCost += dailyRate;
                    remainingMinutes = 0;
                }
            }
        }

        // محاسبه هزینه برای زمان کمتر از آستانه روزانه یا روزها

        if (totalDays < 1)
        {
            if (remainingMinutes > 1)
            {
                var variableTimeRange = hourlyRates.FirstOrDefault(v => v.IsVariableEnable);
                if (entryTime.TimeOfDay >= variableTimeRange?.TimeFrom.ToTimeSpan() && entryTime.TimeOfDay <= variableTimeRange?.TimeTo.ToTimeSpan())
                {
                    foreach (var segment in variablePriceList.OrderBy(v => v.Number))
                    {
                        if (remainingMinutes > segment.Minutes)
                        {
                            totalCost += segment.Price ?? 0;
                            remainingMinutes -= segment.Minutes;
                        }
                        else
                        {
                            totalCost += segment.Price ?? 0;
                            remainingMinutes = 0;
                            break;
                        }
                    }
                }

            }

        }

        // بررسی باقی‌مانده و اعمال تعرفه ساعتی
        if (remainingMinutes > 0)
        {
            DateTime currentTime = entryTime.AddMinutes(totalMinutes - remainingMinutes);
            //remainingMinutes -= 60;
            while (remainingMinutes > 0)
            {
                var currentRate = hourlyRates.FirstOrDefault(rate =>
                    currentTime.TimeOfDay >= rate.TimeFrom.ToTimeSpan() &&
                    currentTime.TimeOfDay < rate.TimeTo.ToTimeSpan());

                if (currentRate is null)
                    break;

                totalCost += currentRate.HourlyRate;
                currentTime = currentTime.AddMinutes(60); // حرکت به ساعت بعد
                remainingMinutes -= 60;
            }
        }

        // محاسبه تخفیف و مالیات
        decimal discountAmount = totalCost * (discountPercentage / 100);
        decimal totalWithDiscount = totalCost - discountAmount;
        decimal taxAmount = totalWithDiscount * (taxPercentage / 100);
        decimal payableAmount = totalWithDiscount + taxAmount;

            if (thresholdHoursPerDay == 0)
            thresholdHoursPerDay = 1;

        return new ParkingPriceOutputModel
        {
            TotalWithoutDiscount = totalCost,
            DiscountAmount = discountAmount,
            PayableAmount = payableAmount,
            TotalDays = totalDays,
            TotalHours = totalTime.Hours % thresholdHoursPerDay,
            TotalMinutes = totalTime.Minutes
        };
    }
}
