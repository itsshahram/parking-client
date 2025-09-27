using Parking.Domain.Entities.ParkingTicket;

namespace Parking.App.Services.Interfaces;

public interface ITicketQueueService
{
    /// <summary>
    /// تخصیص نوبت جدید به یک قبض پارکینگ در صف مشخص
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی (صف)</param>
    /// <param name="parkingTicketId">شناسه قبض پارکینگ</param>
    /// <returns>شماره نوبت تخصیص داده‌شده یا null اگر صف غیرفعال باشد</returns>
    int? AssignQueueNumberAsync(int ticketDescriptionItemId, Guid parkingTicketId);

    /// <summary>
    /// فعال یا غیرفعال کردن نوبت‌دهی برای یک صف خاص
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <param name="isEnabled">وضعیت فعال بودن</param>
    Task SetQueueEnabledAsync(int ticketDescriptionItemId, bool isEnabled);

    /// <summary>
    /// تنظیم یا به‌روزرسانی بازه‌ی ریست نوبت برای یک صف
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <param name="resetIntervalDays">تعداد روزهای بازه</param>
    Task SetResetIntervalAsync(int ticketDescriptionItemId, int resetIntervalDays);

    /// <summary>
    /// گرفتن آخرین نوبت تخصیص داده‌شده در یک صف
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <returns>شماره آخرین نوبت یا null اگر نوبتی وجود نداشته باشد</returns>
    Task<int?> GetLastQueueNumberAsync(int ticketDescriptionItemId);

    /// <summary>
    /// گرفتن لیست نوبت‌های ثبت‌شده برای یک صف خاص
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <returns>لیست نوبت‌ها</returns>
    Task<List<TicketQueueItem>> GetQueueItemsAsync(int ticketDescriptionItemId);

    /// <summary>
    /// بررسی فعال بودن نوبت‌دهی برای یک صف
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <returns>true اگر فعال باشد، false اگر غیرفعال</returns>
    Task<bool> IsQueueEnabledAsync(int ticketDescriptionItemId);

    /// <summary>
    /// گرفتن بازه‌ی ریست نوبت برای یک صف
    /// </summary>
    /// <param name="ticketDescriptionItemId">شناسه آیتم توضیحی</param>
    /// <returns>تعداد روزهای بازه یا null اگر تنظیم نشده باشد</returns>
    Task<int?> GetResetIntervalDaysAsync(int ticketDescriptionItemId);
    int? GetTicketQueueNumber(Guid ticketId);
    Task ResetQueueAsync(int ticketDescriptionItemId);
}