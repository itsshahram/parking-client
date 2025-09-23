using Parking.Domain.Entities.ParkingTicket;

namespace Parking.App.Services;


public class TicketQueueService(
    ILogger<TicketQueueService> logger,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory) : ITicketQueueService
{
    private readonly ILogger<TicketQueueService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public int? AssignQueueNumberAsync(int ticketDescriptionItemId, Guid parkingTicketId)
    {
        try
        {
            var item = _unitOfWork.TicketDescriptionItems
                .GetById(ticketDescriptionItemId);

            if (item == null || !item.IsQueueEnabled)
            {
                _logger.LogWarning("نوبت‌دهی غیرفعال یا آیتم یافت نشد: {ItemId}", ticketDescriptionItemId);
                return null;
            }

            var resetPolicy = _unitOfWork.TicketQueueResetPolicies
                .Find(p => p.TicketDescriptionItemId == ticketDescriptionItemId).FirstOrDefault();

            var resetInterval = TimeSpan.FromDays(resetPolicy?.ResetIntervalDays ?? 30);

            var lastItem =  _unitOfWork.TicketQueueItems
                .Find(q => q.TicketDescriptionItemId == ticketDescriptionItemId)
                .OrderByDescending(q => q.AssignedDate)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastItem != null && DateTime.Now - lastItem.AssignedDate < resetInterval)
                nextNumber = lastItem.QueueNumber + 1;

            var newQueueItem = new TicketQueueItem
            {
                Id = Guid.NewGuid(),
                TicketDescriptionItemId = ticketDescriptionItemId,
                ParkingTicketId = parkingTicketId,
                QueueNumber = nextNumber,
                AssignedDate = DateTime.Now
            };

            _unitOfWork.TicketQueueItems.Add(newQueueItem);
            _unitOfWork.TicketQueueItems.Commit();

            _logger.LogInformation("نوبت {QueueNumber} برای آیتم {ItemId} ثبت شد", nextNumber, ticketDescriptionItemId);
            return nextNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در AssignQueueNumberAsync برای آیتم {ItemId}", ticketDescriptionItemId);
            return null;
        }
    }

    public async Task SetQueueEnabledAsync(int ticketDescriptionItemId, bool isEnabled)
    {
        try
        {
            var item = await _unitOfWork.TicketDescriptionItems
                .FirstOrDefaultAsync(i => i.Id == ticketDescriptionItemId);

            if (item == null)
            {

                _logger.LogWarning("آیتم برای فعال‌سازی نوبت‌دهی یافت نشد: {ItemId}", ticketDescriptionItemId);
                return;
            }

            item.IsQueueEnabled = isEnabled;
            _unitOfWork.TicketDescriptionItems.Update(item);
            await _unitOfWork.TicketDescriptionItems.CommitAsync();

            _logger.LogInformation("وضعیت نوبت‌دهی برای آیتم {ItemId} به {Status} تغییر یافت", ticketDescriptionItemId, isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در SetQueueEnabledAsync برای آیتم {ItemId}", ticketDescriptionItemId);
        }
    }

    public async Task SetResetIntervalAsync(int ticketDescriptionItemId, int resetIntervalDays)
    {
        try
        {
            var policy = await _unitOfWork.TicketQueueResetPolicies
                .FirstOrDefaultAsync(p => p.TicketDescriptionItemId == ticketDescriptionItemId);

            if (policy == null)
            {
                policy = new TicketQueueResetPolicy
                {
                    TicketDescriptionItemId = ticketDescriptionItemId,
                    ResetIntervalDays = resetIntervalDays
                };
                await _unitOfWork.TicketQueueResetPolicies.AddAsync(policy);
            }
            else
            {
                policy.ResetIntervalDays = resetIntervalDays;
                _unitOfWork.TicketQueueResetPolicies.Update(policy);
            }

            await _unitOfWork.TicketQueueResetPolicies.CommitAsync();
            _logger.LogInformation("بازه‌ی ریست نوبت برای آیتم {ItemId} به {Days} روز تنظیم شد", ticketDescriptionItemId, resetIntervalDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در SetResetIntervalAsync برای آیتم {ItemId}", ticketDescriptionItemId);
        }
    }

    public async Task<int?> GetLastQueueNumberAsync(int ticketDescriptionItemId)
    {
        try
        {
            var lastItem = await _unitOfWork.TicketQueueItems
                .Find(q => q.TicketDescriptionItemId == ticketDescriptionItemId)
                .OrderByDescending(q => q.AssignedDate)
                .FirstOrDefaultAsync();

            return lastItem?.QueueNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در GetLastQueueNumberAsync برای آیتم {ItemId}", ticketDescriptionItemId);
            return null;
        }
    }

    public async Task<List<TicketQueueItem>> GetQueueItemsAsync(int ticketDescriptionItemId)
    {
        try
        {
            return await _unitOfWork.TicketQueueItems
                .Find(q => q.TicketDescriptionItemId == ticketDescriptionItemId)
                .OrderBy(q => q.QueueNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در GetQueueItemsAsync برای آیتم {ItemId}", ticketDescriptionItemId);
            return new List<TicketQueueItem>();
        }
    }

    public async Task<bool> IsQueueEnabledAsync(int ticketDescriptionItemId)
    {
        try
        {
            var item = await _unitOfWork.TicketDescriptionItems
                .FirstOrDefaultAsync(i => i.Id == ticketDescriptionItemId);

            return item?.IsQueueEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در IsQueueEnabledAsync برای آیتم {ItemId}", ticketDescriptionItemId);
            return false;
        }
    }

    public async Task<int?> GetResetIntervalDaysAsync(int ticketDescriptionItemId)
    {
        try
        {
            var policy = await _unitOfWork.TicketQueueResetPolicies
                .FirstOrDefaultAsync(p => p.TicketDescriptionItemId == ticketDescriptionItemId);

            return policy?.ResetIntervalDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در GetResetIntervalDaysAsync برای آیتم {ItemId}", ticketDescriptionItemId);
            return null;
        }
    }

    public int? GetTicketQueueNumber(Guid ticketId)
    {
        try
        {
            var queueItem = _unitOfWork.TicketQueueItems
                .Find(q => q.ParkingTicketId == ticketId)
                .OrderByDescending(q => q.AssignedDate)
                .FirstOrDefault();
            return queueItem?.QueueNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در GetTicketQueueNumber برای قبض {TicketId}", ticketId);
            return null;
        }
    }
}