namespace Parking.Core.Models;

public enum PricingStage
{
    PreProcess,     // قوانین رایگان، قوانین ویژه قبل از محاسبه
    BaseCalculate,  // ورود، روزانه، ساعتی، متغیر
    Adjust,         // آستانه ساعتی، ضریب نرخ، تنظیمات دیگر
    Finalize        // تخفیف، مالیات
}