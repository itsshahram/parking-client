using Parking.App.Models.Dto.Card;
using Parking.App.Models.Dto.Parking.ParkingLot;
using Parking.App.Models.Dto.Parking.ParkingSection;
using Parking.App.Models.Dto.Parking.ParkingSpace;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.GeneralServiceResponse;
using Parking.App.Utilities.PriceCalculation;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.Vehicles;
using Parking.Domain.General;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Card = Parking.Domain.Entities.Parkings.Card;
using RandomNumberGenerator = Parking.App.Helpers.RandomNumberGenerator;

namespace Parking.App.Services;

public class ParkingService : IParkingService
{
    private readonly ILogger<ParkingService> _logger;
    private readonly IUnitOfWork unitOfWork;
    private IHttpClientFactory _httpClientFactory;
    private ITicketQueueService _ticketQueueService;

    private HttpClient client = new HttpClient();

    private ParkingCostCalculator? _parkingCostCalculator;

    public List<VehicleSegment> _vehicleSegmentsList;
    public ParkingService(ILogger<ParkingService> logger, IUnitOfWork _unitOfWork, IHttpClientFactory httpClientFactory, ITicketQueueService ticketQueueService)
    {
        _logger = logger;
        _ticketQueueService = ticketQueueService;
        _httpClientFactory = httpClientFactory;
        client = _httpClientFactory.CreateClient();
        unitOfWork = _unitOfWork;
        _vehicleSegmentsList = unitOfWork.VehicleSegments.ToList();

    }
    public TServiceResponse<ParkingLotModel> GetParkingLotDetails()
    {
        try
        {
            var _localParkingInfo = unitOfWork.ParkingLots.FirstOrDefault();
            if (_localParkingInfo != null)
            {
                ParkingLotModel result = new ParkingLotModel()
                {
                    Address = _localParkingInfo.Address,
                    Capacity = _localParkingInfo.Capacity,
                    City = _localParkingInfo.City,
                    CreateDate = DateTime.Now,
                    CreatorUserId = _localParkingInfo.CreatorUserId,
                    Description = _localParkingInfo.Description,
                    EndWorkingHours = _localParkingInfo.EndWorkingHours,
                    FloorsCount = _localParkingInfo.FloorsCount,
                    Image = _localParkingInfo.Image,
                    Id = _localParkingInfo.Id,
                    IsActive = _localParkingInfo.IsActive,
                    IsOnline = _localParkingInfo.IsOnline,
                    LastSyncDateTime = _localParkingInfo.LastSyncDateTime,
                    latitude = _localParkingInfo.latitude,
                    longitude = _localParkingInfo.longitude,
                    Name = _localParkingInfo.Name,
                    OwnerUserId = _localParkingInfo.OwnerUserId,
                    StartWorkingHours = _localParkingInfo.StartWorkingHours,
                    Province = _localParkingInfo.Province
                };
                return new TServiceResponse<ParkingLotModel>(true, "عملیات موفق", result);
            }
            else
            {
                return new TServiceResponse<ParkingLotModel>(false, "اطلاعات پارکینگ یافت نشد");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای سیستمی");
            return new TServiceResponse<ParkingLotModel>(false, "خطای سیستمی");
        }
    }
    public async Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceList()
    {
        try
        {
            List<VehicleSegmentPriceListItemModel> result = new List<VehicleSegmentPriceListItemModel>();
            result = await unitOfWork.VehicleSegments.GetAll().Select(v => new VehicleSegmentPriceListItemModel
            {
                DailyRate = v.DailyRate,
                Description = v.Description,
                FreeEntranceMinutes = v.FreeEntranceMinutes,
                Image = v.Image,
                NameFa = v.NameFa,
                ParkingEntranceFixedFee = v.ParkingEntranceFixedFee,
                ParkingLotId = v.ParkingLotId,
                Id = v.Id
            }).ToListAsync();
            var vehicleSegmentPrices = unitOfWork.ParkingVehicleSegmentPrices.ToList();
            var vehicleSegmentVariablePrices = unitOfWork.ParkingVehicleSegmentVariablePrices.ToList();

            foreach (var item in result)
            {
                item.VehicleSegmentPrices = vehicleSegmentPrices.Where(p => p.VehicleSegmentId == item.Id).Select(p => new ParkingVehicleSegmentPriceListItemModel
                {
                    Id = p.Id,
                    VehicleSegmentId = p.VehicleSegmentId,
                    HourlyRate = p.HourlyRate,
                    IsVariableEnable = p.IsVariableEnable,
                    ParkingLotId = p.ParkingLotId,
                    TimeFrom = p.TimeFrom,
                    TimeTo = p.TimeTo
                }).ToList();
                foreach (var sub in item.VehicleSegmentPrices)
                {
                    if (sub.IsVariableEnable)
                    {
                        sub.VehicleSegmentVariablePrices = vehicleSegmentVariablePrices.Where(v => v.VehicleSegmentId == sub.VehicleSegmentId)
                            .Select(v => new ParkingVehicleSegmentVariablePriceModel
                            {
                                Id = v.Id,
                                VehicleSegmentId = v.VehicleSegmentId,
                                Minutes = v.Minutes,
                                Price = v.Price,
                                Number = v.Number,
                                ParkingLotId = v.ParkingLotId
                            })
                            .ToList();
                    }
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<VehicleSegmentPriceListItemModel>();
        }
    }
    public async Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceListAsync()
    {
        try
        {

            List<VehicleSegmentPriceListItemModel> result = new List<VehicleSegmentPriceListItemModel>();

            var vehicleSegmentsTask = unitOfWork.VehicleSegments.GetAll().Select(v => new VehicleSegmentPriceListItemModel
            {
                DailyRate = v.DailyRate,
                Description = v.Description,
                FreeEntranceMinutes = v.FreeEntranceMinutes,
                Image = v.Image,
                NameFa = v.NameFa,
                ParkingEntranceFixedFee = v.ParkingEntranceFixedFee,
                ParkingLotId = v.ParkingLotId,
                Id = v.Id
            }).ToListAsync();
            var vehicleSegmentPricesTask = unitOfWork.ParkingVehicleSegmentPrices.ToListAsync();
            var vehicleSegmentVariablePricesTask = unitOfWork.ParkingVehicleSegmentVariablePrices.ToListAsync();


            await Task.WhenAll(vehicleSegmentsTask, vehicleSegmentPricesTask, vehicleSegmentVariablePricesTask);
            var vehicleSegments = await vehicleSegmentsTask;
            var vehicleSegmentPrices = await vehicleSegmentPricesTask;
            var vehicleSegmentVariablePrices = await vehicleSegmentVariablePricesTask;
            result = vehicleSegments;
            foreach (var item in result)
            {
                item.VehicleSegmentPrices = vehicleSegmentPrices.Where(p => p.VehicleSegmentId == item.Id).Select(p => new ParkingVehicleSegmentPriceListItemModel
                {
                    Id = p.Id,
                    VehicleSegmentId = p.VehicleSegmentId,
                    HourlyRate = p.HourlyRate,
                    IsVariableEnable = p.IsVariableEnable,
                    ParkingLotId = p.ParkingLotId,
                    TimeFrom = p.TimeFrom,
                    TimeTo = p.TimeTo
                }).ToList();
                foreach (var sub in item.VehicleSegmentPrices)
                {
                    if (sub.IsVariableEnable)
                    {
                        sub.VehicleSegmentVariablePrices = vehicleSegmentVariablePrices.Where(v => v.VehicleSegmentId == sub.VehicleSegmentId)
                            .Select(v => new ParkingVehicleSegmentVariablePriceModel
                            {
                                Id = v.Id,
                                VehicleSegmentId = v.VehicleSegmentId,
                                Minutes = v.Minutes,
                                Price = v.Price,
                                Number = v.Number,
                                ParkingLotId = v.ParkingLotId
                            })
                            .ToList();
                    }

                }
            }

            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<VehicleSegmentPriceListItemModel>();
        }
    }
    public List<ParkingSectionModel> GetSections()
    {
        try
        {

            var sections = unitOfWork.ParkingSections.GetAll().Select(s => new ParkingSectionModel
            {
                Id = s.Id,
                Capacity = s.Capacity,
                CreateDate = s.CreateDate,
                Description = s.Description,
                SectionNumber = s.SectionNumber,
                Floor = s.Floor,
                IndexName = s.IndexName,
                Name = s.Name,
                ParkingId = s.ParkingId,
            }).ToList();
            return sections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<ParkingSectionModel>();
        }
    }
    public List<ParkingSpaceModel> GetSpaces()
    {
        try
        {

            var spaces = unitOfWork.ParkingSpaces.GetAll().Select(s => new ParkingSpaceModel
            {
                Id = s.Id,
                Name = s.Name,
                ParkingId = s.ParkingId,
                IsActive = s.IsActive,
                IsOccupied = s.IsOccupied,
                ParkingSectionId = s.ParkingSectionId,
                SpaceNumber = s.SpaceNumber
            }).ToList();
            return spaces;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<ParkingSpaceModel>();
        }
    }
    public int? GetFreeSpacesCount()
    {
        try
        {
            var parkingLotCapacity = GetParkingLotCapacity();
            var count = parkingLotCapacity - unitOfWork.ParkingTickets.Find(p => p.IsExited == false).Count();
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }
    public int? GetSpacesCount()
    {
        try
        {
            var capacity = GetParkingLotCapacity();
            return capacity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }
    public (Guid? SpaceId, Guid? SectionId) GetOneFreeSpaceId()
    {
        try
        {
            var section = unitOfWork.ParkingSpaces
                .FirstOrDefault(p => p.IsOccupied == false && p.IsActive == true,
                selector: p => new { p.Id, SectionId = p.ParkingSectionId });
            return (section?.Id, section?.SectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (null, null);
        }
    }

    public int? GetParkingLotCapacity()
        => unitOfWork.ParkingLots.FirstOrDefault()?.Capacity;
    public async Task<(ImageSource? StartImage, ImageSource? ExitImage)> GetTicketImages(Guid ticketId)
    {
        try
        {
            var ticketImage = await unitOfWork.ParkingTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            (ImageSource? StartImage, ImageSource? ExitImage) result = (null, null);
            if (ticketImage != null)
            {
                if (ticketImage.StartImage != null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(ticketImage.StartImage))
                        {
                            result.StartImage = null;
                        }
                        else if (IsValidUrl(ticketImage.StartImage))
                        {
                            client = _httpClientFactory.CreateClient();
                            client.Timeout = TimeSpan.FromSeconds(5);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
                            var response = await client.GetAsync(new Uri(ticketImage.StartImage));
                            response.EnsureSuccessStatusCode();
                            var stream = await response.Content.ReadAsStreamAsync();
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            result.StartImage = bitmap;
                        }

                        else if (IsBase64(ticketImage.StartImage))
                        {
                            result.StartImage = ImageHelper.Base64ToImageSource(ticketImage.StartImage);
                        }
                        else
                        {
                            result.StartImage = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                        result.StartImage = null;
                    }
                }
                else
                {
                    result.StartImage = null;
                }

                //ExitImage ===================

                if (ticketImage.ExitImage != null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(ticketImage.ExitImage))
                        {
                            result.ExitImage = null;
                        }
                        else if (IsValidUrl(ticketImage.ExitImage))
                        {
                            client = _httpClientFactory.CreateClient();
                            client.Timeout = TimeSpan.FromSeconds(5);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
                            var response = await client.GetAsync(new Uri(ticketImage.ExitImage));
                            response.EnsureSuccessStatusCode();
                            var stream = await response.Content.ReadAsStreamAsync();
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            result.ExitImage = bitmap;
                        }

                        else if (IsBase64(ticketImage.ExitImage))
                        {
                            result.ExitImage = ImageHelper.Base64ToImageSource(ticketImage.ExitImage);
                        }
                        else
                        {
                            result.ExitImage = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                        result.ExitImage = null;
                    }
                }
                else
                {
                    result.ExitImage = null;
                }
                return result;
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (null, null);
        }
    }
    public List<TicketsListViewModel> GetLatestTickets(TicketType type, int take)
    {
        try
        {

            List<TicketsListViewModel> result = new List<TicketsListViewModel>();
            if (type == TicketType.All)
            {
                result = unitOfWork.ParkingTickets.GetAll()
                 .OrderByDescending(s => s.StartTime)
                 .Take(take).Select(s => new TicketsListViewModel
                 {
                     BarcodeId = s.BarcodeId,
                     Id = s.Id,
                     EndTime = s.EndTime,
                     LicensePlate = s.LicensePlate,
                     ParkingSpaceID = s.ParkingSpaceID,
                     StartTime = s.StartTime,
                     VehicleManufacturerName = s.VehicleManufacturerName,
                     VehicleSegmentId = s.VehicleSegmentId,
                     StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                     StartTimeString = s.StartTime.ToLongShamsiString(),
                     StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                     EndTimeString = s.EndTime.ToLongShamsiString(),
                     Discount = s.Discount,
                     DiscountPercent = s.DiscountPercent,
                     DurationMinutes = s.DurationMinutes,
                     EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                     IsExited = s.IsExited,
                     IsPaid = s.IsPaid,
                     PaidAmount = s.PaidAmount,
                     PaidCreditCard = s.PaidCreditCard,
                     PaidType = s.PaidType,
                     RefId = s.RefId,
                     TotalAmount = s.TotalAmount,
                     EnLicensePlate = s.EnLicensePlate,
                     LicensePlateGroupId = s.LicensePlateGroupId,
                     ParkingLotId = s.ParkingLotId,
                     MerchantNumber = s.MerchantNumber,
                     PaidDate = s.PaidDate,
                     RRN = s.RRN,
                     TraceNo = s.TraceNo,
                 })
                 .ToList();
            }
            else if (type == TicketType.Entrance)
            {
                result = unitOfWork.ParkingTickets.GetAll()
                  .OrderByDescending(s => s.StartTime)
                  .Where(s => s.IsExited == false)
                  .Take(take).Select(s => new TicketsListViewModel
                  {
                      BarcodeId = s.BarcodeId,
                      Id = s.Id,
                      EndTime = s.EndTime,
                      LicensePlate = s.LicensePlate,
                      ParkingSpaceID = s.ParkingSpaceID,
                      StartTime = s.StartTime,
                      VehicleManufacturerName = s.VehicleManufacturerName,
                      VehicleSegmentId = s.VehicleSegmentId,
                      StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                      StartTimeString = s.StartTime.ToLongShamsiString(),
                      StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                      EndTimeString = s.EndTime.ToLongShamsiString(),
                      Discount = s.Discount,
                      DiscountPercent = s.DiscountPercent,
                      DurationMinutes = s.DurationMinutes,
                      EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                      IsExited = s.IsExited,
                      IsPaid = s.IsPaid,
                      PaidAmount = s.PaidAmount,
                      PaidCreditCard = s.PaidCreditCard,
                      PaidType = s.PaidType,
                      RefId = s.RefId,
                      TotalAmount = s.TotalAmount,
                      EnLicensePlate = s.EnLicensePlate,
                      LicensePlateGroupId = s.LicensePlateGroupId,
                      ParkingLotId = s.ParkingLotId,
                      MerchantNumber = s.MerchantNumber,
                      PaidDate = s.PaidDate,
                      RRN = s.RRN,
                      TraceNo = s.TraceNo,
                  })
                  .ToList();
            }
            else
            {
                result = unitOfWork.ParkingTickets.GetAll()
                    .OrderByDescending(s => s.EndTime)
                    .Where(s => s.IsExited == true)
                    .Take(take).Select(s => new TicketsListViewModel
                    {
                        BarcodeId = s.BarcodeId,
                        Id = s.Id,
                        EndTime = s.EndTime,
                        LicensePlate = s.LicensePlate,
                        ParkingSpaceID = s.ParkingSpaceID,
                        StartTime = s.StartTime,
                        VehicleManufacturerName = s.VehicleManufacturerName,
                        VehicleSegmentId = s.VehicleSegmentId,
                        StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                        StartTimeString = s.StartTime.ToLongShamsiString(),
                        StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                        EndTimeString = s.EndTime.ToLongShamsiString(),
                        Discount = s.Discount,
                        DiscountPercent = s.DiscountPercent,
                        DurationMinutes = s.DurationMinutes,
                        EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                        IsExited = s.IsExited,
                        IsPaid = s.IsPaid,
                        PaidAmount = s.PaidAmount,
                        PaidCreditCard = s.PaidCreditCard,
                        PaidType = s.PaidType,
                        RefId = s.RefId,
                        TotalAmount = s.TotalAmount,
                        EnLicensePlate = s.EnLicensePlate,
                        LicensePlateGroupId = s.LicensePlateGroupId,
                        ParkingLotId = s.ParkingLotId,
                        MerchantNumber = s.MerchantNumber,
                        PaidDate = s.PaidDate,
                        RRN = s.RRN,
                        TraceNo = s.TraceNo,
                    })
                    .ToList();

            }

            foreach (var irem in result)
            {
                irem.IsSeized = IsSeizedLicensePlate(irem.EnLicensePlate);
            }
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketsListViewModel>();
        }
    }
    public async Task<List<TicketsListViewModel>> GetLatestTicketsAsync(TicketType type, int take)
    {
        try
        {
            List<TicketsListViewModel> result = new List<TicketsListViewModel>();
            if (type == TicketType.All)
            {

                result = await unitOfWork.ParkingTickets.GetAll()
                 .OrderByDescending(s => s.StartTime)
                 .Take(take).Select(s => new TicketsListViewModel
                 {
                     BarcodeId = s.BarcodeId,
                     Id = s.Id,
                     EndTime = s.EndTime,
                     LicensePlate = s.LicensePlate,
                     ParkingSpaceID = s.ParkingSpaceID,
                     StartTime = s.StartTime,
                     VehicleManufacturerName = s.VehicleManufacturerName,
                     VehicleSegmentId = s.VehicleSegmentId,
                     StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                     StartTimeString = s.StartTime.ToLongShamsiString(),
                     StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                     EndTimeString = s.EndTime.ToLongShamsiString(),
                     Discount = s.Discount,
                     DiscountPercent = s.DiscountPercent,
                     DurationMinutes = s.DurationMinutes,
                     EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                     IsExited = s.IsExited,
                     IsPaid = s.IsPaid,
                     PaidAmount = s.PaidAmount,
                     PaidCreditCard = s.PaidCreditCard,
                     PaidType = s.PaidType,
                     RefId = s.RefId,
                     TotalAmount = s.TotalAmount,
                     EnLicensePlate = s.EnLicensePlate,
                     LicensePlateGroupId = s.LicensePlateGroupId,
                     ParkingLotId = s.ParkingLotId,
                     MerchantNumber = s.MerchantNumber,
                     PaidDate = s.PaidDate,
                     RRN = s.RRN,
                     TraceNo = s.TraceNo,
                 })
                 .ToListAsync();
            }
            else if (type == TicketType.Entrance)
            {
                result = await unitOfWork.ParkingTickets.GetAll()
                  .OrderByDescending(s => s.StartTime)
                  .Where(s => s.IsExited == false)
                  .Take(take).Select(s => new TicketsListViewModel
                  {
                      BarcodeId = s.BarcodeId,
                      Id = s.Id,
                      EndTime = s.EndTime,
                      LicensePlate = s.LicensePlate,
                      ParkingSpaceID = s.ParkingSpaceID,
                      StartTime = s.StartTime,
                      VehicleManufacturerName = s.VehicleManufacturerName,
                      VehicleSegmentId = s.VehicleSegmentId,
                      StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                      StartTimeString = s.StartTime.ToLongShamsiString(),
                      StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                      EndTimeString = s.EndTime.ToLongShamsiString(),
                      Discount = s.Discount,
                      DiscountPercent = s.DiscountPercent,
                      DurationMinutes = s.DurationMinutes,
                      EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                      IsExited = s.IsExited,
                      IsPaid = s.IsPaid,
                      PaidAmount = s.PaidAmount,
                      PaidCreditCard = s.PaidCreditCard,
                      PaidType = s.PaidType,
                      RefId = s.RefId,
                      TotalAmount = s.TotalAmount,
                      EnLicensePlate = s.EnLicensePlate,
                      LicensePlateGroupId = s.LicensePlateGroupId,
                      ParkingLotId = s.ParkingLotId,
                      MerchantNumber = s.MerchantNumber,
                      PaidDate = s.PaidDate,
                      RRN = s.RRN,
                      TraceNo = s.TraceNo,
                  })
                  .ToListAsync();
            }
            else
            {
                result = await unitOfWork.ParkingTickets.GetAll()
                    .OrderByDescending(s => s.EndTime)
                    .Where(s => s.IsExited == true)
                    .Take(take).Select(s => new TicketsListViewModel
                    {
                        BarcodeId = s.BarcodeId,
                        Id = s.Id,
                        EndTime = s.EndTime,
                        LicensePlate = s.LicensePlate,
                        ParkingSpaceID = s.ParkingSpaceID,
                        StartTime = s.StartTime,
                        VehicleManufacturerName = s.VehicleManufacturerName,
                        VehicleSegmentId = s.VehicleSegmentId,
                        StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                        StartTimeString = s.StartTime.ToLongShamsiString(),
                        StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                        EndTimeString = s.EndTime.ToLongShamsiString(),
                        Discount = s.Discount,
                        DiscountPercent = s.DiscountPercent,
                        DurationMinutes = s.DurationMinutes,
                        EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                        IsExited = s.IsExited,
                        IsPaid = s.IsPaid,
                        PaidAmount = s.PaidAmount,
                        PaidCreditCard = s.PaidCreditCard,
                        PaidType = s.PaidType,
                        RefId = s.RefId,
                        TotalAmount = s.TotalAmount,
                        EnLicensePlate = s.EnLicensePlate,
                        LicensePlateGroupId = s.LicensePlateGroupId,
                        ParkingLotId = s.ParkingLotId,
                        MerchantNumber = s.MerchantNumber,
                        PaidDate = s.PaidDate,
                        RRN = s.RRN,
                        TraceNo = s.TraceNo,
                    })
                    .ToListAsync();

            }

            foreach (var irem in result)
            {
                irem.IsSeized = IsSeizedLicensePlate(irem.EnLicensePlate);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketsListViewModel>();
        }
    }
    public TicketsListViewModel? GetTicketDetails(Guid ticketId)
    {
        try
        {
            // 1) Load TICKET ONLY from DB
            var ticket = unitOfWork.ParkingTickets
                .FirstOrDefault(s => s.Id == ticketId);

            if (ticket == null)
                return null;

            // 2) Map into ViewModel (NO calculations here)
            var vm = new TicketsListViewModel
            {
                Id = ticket.Id,
                LicensePlate = ticket.LicensePlate,
                ParkingSpaceID = ticket.ParkingSpaceID,
                StartTime = ticket.StartTime,
                EndTime = ticket.EndTime,
                VehicleManufacturerName = ticket.VehicleManufacturerName,
                VehicleSegmentId = ticket.VehicleSegmentId,
                Discount = ticket.Discount,
                DiscountPercent = ticket.DiscountPercent,
                DurationMinutes = ticket.DurationMinutes,
                IsExited = ticket.IsExited,
                IsPaid = ticket.IsPaid,
                PaidAmount = ticket.PaidAmount,
                PaidCreditCard = ticket.PaidCreditCard,
                PaidType = ticket.PaidType,
                RefId = ticket.RefId,
                TotalAmount = ticket.TotalAmount,
                EnLicensePlate = ticket.EnLicensePlate,
                ParkingLotId = ticket.ParkingLotId,
                MerchantNumber = ticket.MerchantNumber,
                TraceNo = ticket.TraceNo,
                PaidDate = ticket.PaidDate,
                RRN = ticket.RRN,
                LicensePlateGroupId = ticket.LicensePlateGroupId,
                Description = ticket.Description,
                CardUid = ticket.CardUid,
                BarcodeId = ticket.BarcodeId,
                QueueNumber = ticket.QueueNumber,
                DriverDescription = ticket.DriverDescription
            };

            // If exited → no calculation needed
            if (ticket.IsExited)
                return vm;

            // 3) Load segment + card + prices (separate lightweight queries)
            var segment = unitOfWork.VehicleSegments
                .FirstOrDefault(s => s.Id == ticket.VehicleSegmentId);

            if (segment == null)
                return vm;

            var segmentPrices = unitOfWork.ParkingVehicleSegmentPrices
                .Find(p => p.VehicleSegmentId == segment.Id)
                .ToList();

            var segmentVariables = unitOfWork.ParkingVehicleSegmentVariablePrices
                .Find(p => p.VehicleSegmentId == segment.Id)
                .ToList();

            var card = (ticket.CardUid.HasValue)
                ? unitOfWork.Cards.FirstOrDefault(c => c.CardSerialNo == ticket.CardUid)
                : null;

            // 4) Discount logic
            short discount = GetLicensePlateDiscountPercent(ticket.EnLicensePlate ?? "_");

            if (card != null && card.PercentDiscount > 0)
                discount = (short)card.PercentDiscount;

            // 5) Calculate cost
            var calc = new ParkingCostCalculator(
                (int)segment.ParkingEntranceFixedFee,
                (int)segment.DailyRate,
                segment.FreeEntranceMinutes,
                segment.ThresholdNumberOfDays,
                segment.DailyPriceAfterCrossingThreshold,
                segment.ThresholdHoursPerDay,
                discount,
                segment.TaxPercentage,
                segmentPrices,
                segmentVariables
            );

            var result = calc.CalculateCost(ticket.StartTime, DateTime.Now);
            var duration = DateTime.Now - ticket.StartTime;

            string desc = $"{duration.Days} روز و {duration.Hours} ساعت و {duration.Minutes} دقیقه در {segment.NameFa}";

            if (card != null)
            {
                if (card.PercentDiscount > 0)
                    desc += $" | کارت دارای تخفیف {card.PercentDiscount}% است";

                if (card.FixDiscount > 0)
                {
                    result.PayableAmount =
                        Math.Max(result.PayableAmount - card.FixDiscount, 0);

                    desc += $" | کارت دارای تخفیف {card.FixDiscount} ریال است";
                }
            }

            var lot = unitOfWork.ParkingLots
                .FirstOrDefault(x => x.Id == ticket.ParkingLotId);

            vm.ParkingName = lot?.Name;
            vm.DiscountPercent = (byte)discount;
            vm.TotalAmount = result.PayableAmount;
            vm.Description = desc;
            vm.DurationMinutes = (int)duration.TotalMinutes;

            unitOfWork.ParkingTickets.ExecuteUpdate(p => p.Id == ticketId, update => update
                .SetProperty(p => p.DurationMinutes, (int)duration.TotalMinutes)
                .SetProperty(p => p.DiscountPercent, (byte)discount)
                .SetProperty(p => p.TotalAmount, result.PayableAmount)
                .SetProperty(p => p.Description, desc)
                .SetProperty(p => p.TicketStatus, TicketStatus.Unsynced));

            return vm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTicketDetails");
            return null;
        }
    }

    public async Task<TicketsListViewModel?> GetTicketDetailsAsync(Guid ticketId)
    {
        try
        {
            var ticket = await unitOfWork.ParkingTickets
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return null;

            var vm = new TicketsListViewModel
            {
                Id = ticket.Id,
                LicensePlate = ticket.LicensePlate,
                ParkingSpaceID = ticket.ParkingSpaceID,
                StartTime = ticket.StartTime,
                EndTime = ticket.EndTime,
                VehicleManufacturerName = ticket.VehicleManufacturerName,
                VehicleSegmentId = ticket.VehicleSegmentId,
                Discount = ticket.Discount,
                DiscountPercent = ticket.DiscountPercent,
                DurationMinutes = ticket.DurationMinutes,
                IsExited = ticket.IsExited,
                IsPaid = ticket.IsPaid,
                PaidAmount = ticket.PaidAmount,
                PaidCreditCard = ticket.PaidCreditCard,
                PaidType = ticket.PaidType,
                RefId = ticket.RefId,
                TotalAmount = ticket.TotalAmount,
                EnLicensePlate = ticket.EnLicensePlate,
                ParkingLotId = ticket.ParkingLotId,
                MerchantNumber = ticket.MerchantNumber,
                TraceNo = ticket.TraceNo,
                PaidDate = ticket.PaidDate,
                RRN = ticket.RRN,
                LicensePlateGroupId = ticket.LicensePlateGroupId,
                Description = ticket.Description,
                CardUid = ticket.CardUid,
                BarcodeId = ticket.BarcodeId,
                QueueNumber = ticket.QueueNumber,
                DriverDescription = ticket.DriverDescription,
            };

            if (ticket.IsExited)
                return vm;

            var segment = await unitOfWork.VehicleSegments
                .FirstOrDefaultAsync(p => p.Id == ticket.VehicleSegmentId);

            if (segment == null)
                return vm;

            var segmentPrices = await unitOfWork.ParkingVehicleSegmentPrices
                .Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId)
                .ToListAsync();

            var segmentVariables = await unitOfWork.ParkingVehicleSegmentVariablePrices
                .Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId)
                .ToListAsync();

            var card = await unitOfWork.Cards
                .FirstOrDefaultAsync(c => c.CardSerialNo == ticket.CardUid);

            short discount = await GetLicensePlateDiscountPercentAsync(ticket.EnLicensePlate ?? "_");

            if (card != null && card.PercentDiscount > 0)
                discount = (short)card.PercentDiscount;

            var calc = new ParkingCostCalculator(
                (int)segment.ParkingEntranceFixedFee,
                (int)segment.DailyRate,
                segment.FreeEntranceMinutes,
                segment.ThresholdNumberOfDays,
                segment.DailyPriceAfterCrossingThreshold,
                segment.ThresholdHoursPerDay,
                discount,
                segment.TaxPercentage,
                segmentPrices,
                segmentVariables
            );

            var result = calc.CalculateCost(ticket.StartTime, DateTime.Now);
            var duration = DateTime.Now - ticket.StartTime;

            string desc = $"{duration.Days} روز و {duration.Hours} ساعت و {duration.Minutes} دقیقه در {segment.NameFa}";

            if (card != null)
            {
                if (card.PercentDiscount > 0)
                    desc += $" | کارت دارای تخفیف {card.PercentDiscount}% است";

                if (card.FixDiscount > 0)
                {
                    result.PayableAmount = Math.Max(result.PayableAmount - card.FixDiscount, 0);
                    desc += $" | کارت دارای تخفیف {card.FixDiscount} ریال است";
                }
            }

            var lot = await unitOfWork.ParkingLots
                .FirstOrDefaultAsync(x => x.Id == ticket.ParkingLotId);

            vm.ParkingName = lot?.Name;
            vm.DiscountPercent = (byte)discount;
            vm.TotalAmount = result.PayableAmount;
            vm.Description = desc;
            vm.DurationMinutes = (int)duration.TotalMinutes;

            await unitOfWork.ParkingTickets.ExecuteUpdateAsync(
                p => p.Id == ticketId,
                update => update
                    .SetProperty(p => p.DurationMinutes, (int)duration.TotalMinutes)
                    .SetProperty(p => p.DiscountPercent, (byte)discount)
                    .SetProperty(p => p.TotalAmount, result.PayableAmount)
                    .SetProperty(p => p.Description, desc)
                    .SetProperty(p => p.TicketStatus, TicketStatus.Unsynced));

            return vm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTicketDetailsAsync");
            return null;
        }
    }

    public TicketsListViewModel? GetTicketDetails(long barcode)
    {
        try
        {
            var ticket = unitOfWork.ParkingTickets.Find(s => s.BarcodeId == barcode)
                .OrderByDescending(s => s.StartTime).Select(s => new TicketsListViewModel
                {
                    Id = s.Id,
                    EndTime = s.EndTime,
                    LicensePlate = s.LicensePlate,
                    ParkingSpaceID = s.ParkingSpaceID,
                    StartTime = s.StartTime,
                    VehicleManufacturerName = s.VehicleManufacturerName,
                    VehicleSegmentId = s.VehicleSegmentId,
                    StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                    StartTimeString = s.StartTime.ToLongShamsiString(),
                    StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                    EndTimeString = s.EndTime.ToLongShamsiString(),
                    Discount = s.Discount,
                    DiscountPercent = s.DiscountPercent,
                    DurationMinutes = s.DurationMinutes,
                    EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                    ExitImage = s.ExitImage,
                    StartImage = s.StartImage,
                    IsExited = s.IsExited,
                    IsPaid = s.IsPaid,
                    PaidAmount = s.PaidAmount,
                    PaidCreditCard = s.PaidCreditCard,
                    PaidType = s.PaidType,
                    RefId = s.RefId,
                    TotalAmount = s.TotalAmount,
                    EnLicensePlate = s.EnLicensePlate,
                    ParkingLotId = s.ParkingLotId,
                    MerchantNumber = s.MerchantNumber,
                    TraceNo = s.TraceNo,
                    PaidDate = s.PaidDate,
                    RRN = s.RRN,
                    LicensePlateGroupId = s.LicensePlateGroupId,
                    CardUid = s.CardUid
                }).FirstOrDefault();
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public TicketsListViewModel? GetTicketDetailsByCardId(long cardUid)
    {
        try
        {

            var ticket = unitOfWork.ParkingTickets
                .Find(s => s.CardUid == cardUid)
                .OrderByDescending(s => s.StartTime).Select(s => new TicketsListViewModel
                {
                    Id = s.Id,
                    EndTime = s.EndTime,
                    LicensePlate = s.LicensePlate,
                    ParkingSpaceID = s.ParkingSpaceID,
                    StartTime = s.StartTime,
                    VehicleManufacturerName = s.VehicleManufacturerName,
                    VehicleSegmentId = s.VehicleSegmentId,
                    StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                    StartTimeString = s.StartTime.ToLongShamsiString(),
                    StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                    EndTimeString = s.EndTime.ToLongShamsiString(),
                    Discount = s.Discount,
                    DiscountPercent = s.DiscountPercent,
                    DurationMinutes = s.DurationMinutes,
                    EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                    ExitImage = s.ExitImage,
                    StartImage = s.StartImage,
                    IsExited = s.IsExited,
                    IsPaid = s.IsPaid,
                    PaidAmount = s.PaidAmount,
                    PaidCreditCard = s.PaidCreditCard,
                    PaidType = s.PaidType,
                    RefId = s.RefId,
                    TotalAmount = s.TotalAmount,
                    EnLicensePlate = s.EnLicensePlate,
                    ParkingLotId = s.ParkingLotId,
                    MerchantNumber = s.MerchantNumber,
                    TraceNo = s.TraceNo,
                    PaidDate = s.PaidDate,
                    RRN = s.RRN,
                    LicensePlateGroupId = s.LicensePlateGroupId,
                    CardUid = s.CardUid
                })
                .FirstOrDefault();
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public TicketsListViewModel? GetActiveTicketByCard(long cardUid)
    {
        try
        {

            var s = unitOfWork.ParkingTickets.Find(s => s.CardUid == cardUid && s.IsExited == false).OrderByDescending(s => s.StartTime)
                    .Select(s => new TicketsListViewModel
                    {
                        BarcodeId = s.BarcodeId,
                        Id = s.Id,
                        EndTime = s.EndTime,
                        LicensePlate = s.LicensePlate,
                        ParkingSpaceID = s.ParkingSpaceID,
                        StartTime = s.StartTime,
                        VehicleManufacturerName = s.VehicleManufacturerName,
                        VehicleSegmentId = s.VehicleSegmentId,
                        StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                        StartTimeString = s.StartTime.ToLongShamsiString(),
                        StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                        EndTimeString = s.EndTime.ToLongShamsiString(),
                        Discount = s.Discount,
                        DiscountPercent = s.DiscountPercent,
                        DurationMinutes = s.DurationMinutes,
                        EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                        StartImage = s.StartImage,
                        ExitImage = s.ExitImage,
                        IsExited = s.IsExited,
                        IsPaid = s.IsPaid,
                        PaidAmount = s.PaidAmount,
                        PaidCreditCard = s.PaidCreditCard,
                        PaidType = s.PaidType,
                        RefId = s.RefId,
                        TotalAmount = s.TotalAmount,
                        EnLicensePlate = s.EnLicensePlate,
                        ParkingLotId = s.ParkingLotId,
                        Description = s.Description,
                        LicensePlateGroupId = s.LicensePlateGroupId,
                        RRN = s.RRN,
                        PaidDate = s.PaidDate,
                        TraceNo = s.TraceNo,
                        MerchantNumber = s.MerchantNumber,
                        CardUid = s.CardUid
                    })
                    .FirstOrDefault();
            return s;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public Guid? GetActiveLicensePlateTicketId(string licenseEnPlate)
    {
        try
        {
            //using (var uow = _unitOfWorkFactory.Create())
            return unitOfWork.ParkingTickets.Find(s => s.EnLicensePlate == licenseEnPlate && s.IsExited == false).OrderByDescending(s => s.StartTime).Select(s => (Guid?)s.Id).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public bool LicensePlateTicketIsExist(string licenseEnPlate)
       => unitOfWork.ParkingTickets.Find(t => t.EnLicensePlate == licenseEnPlate && t.IsExited == false).Any();
    public TServiceResponse<Guid> CreateTicket(CreateParkingTicketModel request, string? StartImage)
    {
        try
        {
            var groupId = GetGroupIdByEnLicensePlate(request.EnLicensePlate ?? "__-_-___");
            long barcode = RandomNumberGenerator.GenerateLongRandomNumber();

            string? DriverDescription = (request.TicketDescriptionItemId != null && request.TicketDescriptionItemId > 0) ? GetTicketDescriptionItemById((int)request.TicketDescriptionItemId)?.Text : request.DriverDescription;

            while (unitOfWork.ParkingTickets.Find(x => x.BarcodeId == barcode).Any())
            {
                barcode = RandomNumberGenerator.GenerateLongRandomNumber();
            }
            if (StartImage != null)
            {
                request.StartImage = StartImage;
            }
            ParkingTicket ticket = new ParkingTicket()
            {
                VehicleManufacturerName = request.VehicleManufacturerName,
                VehicleModel = request.VehicleModel,
                VehicleColor = request.VehicleColor,
                LicensePlate = request.LicensePlate,
                VehicleSegmentId = request.VehicleSegmentId,
                ParkingSpaceID = request.ParkingSpaceID,
                StartImage = request.StartImage,
                StartTime = request.StartTime,
                EndTime = DateTime.Now,
                Description = request.Description,
                ParkingLotId = TokenStore.ParkingLotId,
                ParkingSectionId = request.ParkingSectionId,
                EnLicensePlate = request.EnLicensePlate,
                TicketStatus = TicketStatus.Unsynced,
                LicensePlateGroupId = groupId,
                IsPaid = false,
                IsExited = false,
                CardUid = request.CardUid,
                EntranceGate = request.EntranceGate,
                BarcodeId = barcode,
                UserId = request.CreatorUserId,
                TicketDescriptionItemId = (request.TicketDescriptionItemId > 0) ? request.TicketDescriptionItemId : null,
                DriverDescription = DriverDescription,
                DriverFullName = request.DriverFullName,
                DriverPhoneNumber = request.DriverPhoneNumber,
                DeviceId = Settings.Default.Application_DeviceId,
                IP = LogHelper.GetLocalIPAddress()
            };
            unitOfWork.ParkingTickets.Add(ticket);

            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == request.CardUid, update => update.SetProperty(s => s.IsInUse, true));
            if (Settings.Default.Application_QueueActive)
            {
                if (ticket.TicketDescriptionItemId != null && request.TicketDescriptionItemId > 0)
                {
                    try
                    {
                        var result = _ticketQueueService.AssignQueueNumberAsync((int)ticket.TicketDescriptionItemId, ticket.Id);
                        unitOfWork.ParkingTickets.ExecuteUpdate(s => s.Id == ticket.Id, update => update.SetProperty(s => s.QueueNumber, result));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"خطا در ثبت نوبت قبض  {ex.Message}", ex);
                    }
                }
            }
            return new TServiceResponse<Guid>() { Succeeded = true, Result = ticket.Id, Message = "بلیط بارکینگ با موفقیت ثبت شد" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new TServiceResponse<Guid>() { Succeeded = false, Message = "خطا در ثبت قبض" };
        }
    }

    #region TicketList
    public (List<TicketsListViewModel> Data, int TotalCount) GetTicketList(GetTicketListRequestModel request)
    {
        try
        {
            IQueryable<TicketsListViewModel> tickets = TicketListBaseQuery();

            tickets = ApplyTicketListFilter(request, tickets);

            int TotalCount = tickets.Count();
            var t = tickets
              .Select(ToTicketListViewModelResult())
              .Skip((request.CurrentPage - 1) * request.ItemsPerPage)
              .Take(request.ItemsPerPage).ToList();

            return (t, TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (new List<TicketsListViewModel>(), 0);
        }
    }
    public async Task<(List<TicketsListViewModel> Data, int TotalCount)> GetTicketListAsync(GetTicketListRequestModel request)
    {
        try
        {
            var tickets = TicketListBaseQuery();

            tickets = ApplyTicketListFilter(request, tickets);

            int TotalCount = tickets.Count();

            var t = await tickets
                .Select(ToTicketListViewModelResult())
                .Skip((request.CurrentPage - 1) * request.ItemsPerPage)
                .Take(request.ItemsPerPage)
                .ToListAsync();

            return (t, TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (new List<TicketsListViewModel>(), 0);
        }
    }
    private static IQueryable<TicketsListViewModel> ApplyTicketListFilter(GetTicketListRequestModel request, IQueryable<TicketsListViewModel> tickets)
    {
        if (request.ParkingId != null)
            tickets = tickets.Where(t => t.ParkingLotId == request.ParkingId);
        if (request.VehicleSegmentId != null)
            tickets = tickets.Where(t => t.VehicleSegmentId == request.VehicleSegmentId);
        if (request.ParkingSpaceID != null)
            tickets = tickets.Where(t => t.ParkingSpaceID == request.ParkingSpaceID);
        if (request.PaidType != null)
            tickets = tickets.Where(t => t.PaidType == request.PaidType);
        if (request.MinDurationMinutes != null)
            tickets = tickets.Where(t => t.DurationMinutes >= request.MinDurationMinutes);
        if (request.MaxDurationMinutes != null)
            tickets = tickets.Where(t => t.DurationMinutes <= request.MaxDurationMinutes);
        if (request.MinTotalAmount != null)
            tickets = tickets.Where(t => t.TotalAmount >= request.MinTotalAmount);
        if (request.MaxTotalAmount != null)
            tickets = tickets.Where(t => t.TotalAmount <= request.MaxTotalAmount);
        if (request.IsPaid != null)
            tickets = tickets.Where(t => t.IsPaid == request.IsPaid);

        if (request.EntryFrom != null)
            tickets = tickets.Where(t => t.StartTime >= request.EntryFrom);

        if (request.EntryTo != null)
            tickets = tickets.Where(t => t.StartTime <= request.EntryTo);

        if (request.ExitFrom != null)
            tickets = tickets.Where(t => t.EndTime >= request.ExitFrom);

        if (request.ExitTo != null)
            tickets = tickets.Where(t => t.EndTime <= request.ExitTo);

        if (!string.IsNullOrEmpty(request.GateType))
        {
            if (request.GateType == "EntranceGate")
                tickets = tickets.Where(t => t.EntranceGate != null && t.EntranceGate != "");
            else if (request.GateType == "ExitGate")
                tickets = tickets.Where(t => t.ExitGate != null && t.ExitGate != "");
        }

        if (request.BarcodeId != null)
            tickets = tickets.Where(t => t.BarcodeId == request.BarcodeId);
        if (request.LicensePlate != null && request.LicensePlate.Length > 1)
            tickets = tickets.Where(t => t.EnLicensePlate.Contains(request.LicensePlate));

        if (request.PriceFrom != null)
            tickets = tickets.Where(x => x.TotalAmount >= request.PriceFrom);

        if (request.PriceTo != null)
            tickets = tickets.Where(x => x.TotalAmount <= request.PriceTo);

        if (!string.IsNullOrEmpty(request.EntryRegistrar))
            tickets.Where(x => x.EntranceGate == request.EntryRegistrar);

        if (!string.IsNullOrEmpty(request.ExitRegistrar))
            tickets.Where(x => x.ExitGate == request.ExitRegistrar);

        if (request.HasDiscrepancy == true)
        {
            tickets = tickets.Where(x =>
            x.IsExited == true &&
            x.IsPaid.Value == true &&
            !(
            x.TotalAmount == 0m &&
            x.DiscountPercent == 100 &&
            x.PaidAmount == 0m
            ) &&
            x.PaidAmount != (x.TotalAmount * (1 - (x.DiscountPercent / 100m))));
        }

        if (!string.IsNullOrEmpty(request.RRN))
            tickets = tickets.Where(x => x.RRN == request.RRN);


        if (!string.IsNullOrEmpty(request.TrackNo))
            tickets = tickets.Where(x => x.TraceNo == request.TrackNo);


        if (request.VehicleStatus != null)
        {
            Expression<Func<TicketsListViewModel, bool>> expression = x =>
            request.VehicleStatus == VehicleStatus.Entered ? x.IsExited.Value == false : x.IsExited.Value;

            tickets = tickets.Where(expression);
        }

        if (request.Discount != null)
            tickets = tickets.Where(x => x.DiscountPercent == request.Discount);

        return tickets;
    }
    private static Expression<Func<TicketsListViewModel, TicketsListViewModel>> ToTicketListViewModelResult()
    {
        return s => new TicketsListViewModel
        {
            Id = s.Id,
            BarcodeId = s.BarcodeId,
            ParkingLotId = s.ParkingLotId,
            EndTime = s.EndTime,
            LicensePlate = s.LicensePlate,
            ParkingSpaceID = s.ParkingSpaceID,
            StartTime = s.StartTime,
            VehicleManufacturerName = s.VehicleManufacturerName,
            VehicleSegmentId = s.VehicleSegmentId,
            StartRelativeTimeString = s.StartTime.ToRelativeDate(),
            StartTimeString = s.StartTime.ToLongShamsiString(),
            StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
            EndTimeString = s.EndTime.ToLongShamsiString(),
            Discount = s.Discount,
            DiscountPercent = s.DiscountPercent,
            DurationMinutes = s.DurationMinutes,
            EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
            StartImage = s.StartImage,
            ExitImage = s.ExitImage,
            IsExited = s.IsExited,
            IsPaid = s.IsPaid,
            PaidAmount = s.PaidAmount,
            PaidCreditCard = s.PaidCreditCard,
            PaidType = s.PaidType,
            RefId = s.RefId,
            TotalAmount = s.TotalAmount,
            EnLicensePlate = s.EnLicensePlate,
            ParkingName = ParkingLotInfoStore.ParkingInfo.Name,
            LicensePlateGroupId = s.LicensePlateGroupId,
            MerchantNumber = s.MerchantNumber,
            RRN = s.RRN,
            PaidDate = s.PaidDate,
            TraceNo = s.TraceNo,
            ExitGate = s.ExitGate,
            EntranceGate = s.ExitGate,
            IsCustomPaid = s.IsCustomPaid,
            IsSeized = s.IsSeized,
            VehicleSegmentName = s.VehicleSegmentName,
        };
    }

    private IQueryable<TicketsListViewModel> TicketListBaseQuery()
    {
        return unitOfWork.ParkingTickets.GetAll()
                                         .Select(s => new TicketsListViewModel
                                         {
                                             BarcodeId = s.BarcodeId,
                                             Id = s.Id,
                                             LicensePlate = s.LicensePlate,
                                             ParkingSpaceID = s.ParkingSpaceID,
                                             StartTime = s.StartTime,
                                             EndTime = s.EndTime,
                                             VehicleManufacturerName = s.VehicleManufacturerName,
                                             VehicleSegmentId = s.VehicleSegmentId,
                                             Discount = s.Discount,
                                             DiscountPercent = s.DiscountPercent,
                                             DurationMinutes = s.DurationMinutes,
                                             IsExited = s.IsExited,
                                             IsPaid = s.IsPaid,
                                             PaidAmount = s.PaidAmount,
                                             PaidCreditCard = s.PaidCreditCard,
                                             PaidType = s.PaidType,
                                             RefId = s.RefId,
                                             TotalAmount = s.TotalAmount,
                                             EnLicensePlate = s.EnLicensePlate,
                                             LicensePlateGroupId = s.LicensePlateGroupId,
                                             MerchantNumber = s.MerchantNumber,
                                             PaidDate = s.PaidDate,
                                             ParkingName = ParkingLotInfoStore.ParkingInfo.Name,
                                             ParkingLotId = s.ParkingLotId,
                                             TraceNo = s.TraceNo,
                                             RRN = s.RRN,
                                             EntranceGate = s.EntranceGate,
                                             ExitGate = s.ExitGate,
                                         });
    }
    #endregion

    public bool ExitRequest(Guid ticketId)
    {
        try
        {
            try
            {


                PaymentAmountCalculation(ticketId);
                var ticket = unitOfWork.ParkingTickets.GetById(ticketId);
                if (ticket == null)
                    return false;


                ticket.IsExited = true;
                ticket.EndTime = DateTime.Now;
                unitOfWork.ParkingTickets.Update(ticket);
                //unitOfWork.Commit();
                unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == ticket.CardUid, update => update.SetProperty(s => s.IsInUse, false));
                return true;

            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }
    public void PaymentAmountCalculation(Guid ticketId)
    {
        try
        {

            var ticket = unitOfWork.ParkingTickets.Find(t => t.Id == ticketId).Select(t => new ParkingTicket
            {
                BarcodeId = t.BarcodeId,
                CardUid = t.CardUid,
                DiscountPercent = t.DiscountPercent,
                EnLicensePlate = t.EnLicensePlate,
                StartTime = t.StartTime,
                VehicleSegmentId = t.VehicleSegmentId,
                Discount = t.Discount,
                TotalAmount = t.TotalAmount,
                IsExited = t.IsExited,
                EndTime = t.EndTime,
                Id = t.Id,
                IsPaid = t.IsPaid,
                LicensePlate = t.LicensePlate,
                LicensePlateGroupId = t.LicensePlateGroupId,
                PaidAmount = t.PaidAmount,
                PaidDate = t.PaidDate,
                PaidType = t.PaidType,
                ParkingLotId = t.ParkingLotId,
                ParkingSpaceID = t.ParkingSpaceID,
                ParkingSectionId = t.ParkingSectionId,
                RefId = t.RefId,
                RRN = t.RRN,
                TicketStatus = t.TicketStatus,
                VehicleColor = t.VehicleColor,
                VehicleManufacturerName = t.VehicleManufacturerName,
                VehicleModel = t.VehicleModel,
                TotalAmountWithDiscount = t.TotalAmountWithDiscount,
                TraceNo = t.TraceNo,
                DeviceId = Settings.Default.Application_DeviceId,
                IP = t.IP
            }).FirstOrDefault();
            if (ticket != null && ticket.IsExited == false)
            {
                var segment = unitOfWork.VehicleSegments.Find(p => p.Id == ticket.VehicleSegmentId).FirstOrDefault();
                var SegmentPrice = unitOfWork.ParkingVehicleSegmentPrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToList();
                var segmentVariablePrice = unitOfWork.ParkingVehicleSegmentVariablePrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToList();


                if (SegmentPrice != null)
                {
                    List<(int, int, int)> hourlyrate = new List<(int, int, int)>(); // Initialize the list
                    foreach (var item in SegmentPrice)
                    {
                        hourlyrate.Add((item.TimeFrom.Hour, item.TimeTo.Hour, (int)item.HourlyRate));
                    }
                    var discount = GetLicensePlateDiscountPercent(ticket.EnLicensePlate ?? "_");

                    var card = unitOfWork.Cards.Find(c => c.CardSerialNo == ticket.CardUid).FirstOrDefault();

                    if (card != null)
                    {
                        if (card.PercentDiscount > 0)
                        {
                            discount = (short)card.PercentDiscount;
                        }
                    }

                    _parkingCostCalculator = new ParkingCostCalculator((int)segment.ParkingEntranceFixedFee,
                             (int)segment.DailyRate,
                             segment.FreeEntranceMinutes, segment.ThresholdNumberOfDays, segment.DailyPriceAfterCrossingThreshold, segment.ThresholdHoursPerDay,
                             discount, segment.TaxPercentage, SegmentPrice, segmentVariablePrice);


                    var result = _parkingCostCalculator.CalculateCost(ticket.StartTime, DateTime.Now);


                    TimeSpan varTime = DateTime.Now - ticket.StartTime;
                    string description = $"{varTime.Days} روز و {varTime.Hours} ساعت و {varTime.Minutes} دقیقه در {segment.NameFa}";
                    if (ticket.CardUid != null)
                    {

                        if (card != null)
                        {

                            if (card.PercentDiscount > 0)
                            {
                                description = description + " | " + $"کارت دارای تخفیف {card.PercentDiscount} درصدی میباشد ";
                            }
                            if (card.FixDiscount > 0)
                            {
                                if (ticket.TotalAmount > card.FixDiscount)
                                {
                                    result.PayableAmount = result.PayableAmount - card.FixDiscount;
                                }
                                else
                                {
                                    result.PayableAmount = 0;
                                }
                                description = description + " | " + $"کارت دارای تخفیف {card.FixDiscount} ريال میباشد ";
                            }

                        }
                    }

                    unitOfWork.ParkingTickets.ExecuteUpdate(p => p.Id == ticketId, update => update
                    .SetProperty(p => p.DurationMinutes, (int)varTime.TotalMinutes)
                    .SetProperty(ticket => ticket.DiscountPercent, ticket => (byte)discount)
                    .SetProperty(ticket => ticket.TotalAmount, ticket => result.PayableAmount)
                    .SetProperty(ticket => ticket.Description, ticket => description)
                    .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Unsynced));

                    //unitOfWork.Commit();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
    public async Task PaymentAmountCalculationAsync(Guid ticketId)
    {
        try
        {

            var ticket = await unitOfWork.ParkingTickets.Find(t => t.Id == ticketId).Select(t => new ParkingTicket
            {
                BarcodeId = t.BarcodeId,
                CardUid = t.CardUid,
                DiscountPercent = t.DiscountPercent,
                EnLicensePlate = t.EnLicensePlate,
                StartTime = t.StartTime,
                VehicleSegmentId = t.VehicleSegmentId,
                Discount = t.Discount,
                TotalAmount = t.TotalAmount,
                IsExited = t.IsExited,
                EndTime = t.EndTime,
                Id = t.Id,
                IsPaid = t.IsPaid,
                LicensePlate = t.LicensePlate,
                LicensePlateGroupId = t.LicensePlateGroupId,
                PaidAmount = t.PaidAmount,
                PaidDate = t.PaidDate,
                PaidType = t.PaidType,
                ParkingLotId = t.ParkingLotId,
                ParkingSpaceID = t.ParkingSpaceID,
                ParkingSectionId = t.ParkingSectionId,
                RefId = t.RefId,
                RRN = t.RRN,
                TicketStatus = t.TicketStatus,
                VehicleColor = t.VehicleColor,
                VehicleManufacturerName = t.VehicleManufacturerName,
                VehicleModel = t.VehicleModel,
                TotalAmountWithDiscount = t.TotalAmountWithDiscount,
                TraceNo = t.TraceNo,
                DeviceId = Settings.Default.Application_DeviceId,
                IP = t.IP
            }).FirstOrDefaultAsync();
            if (ticket != null && ticket.IsExited == false)
            {
                var segmentTask = Task.Run(() => unitOfWork.VehicleSegments.Find(p => p.Id == ticket.VehicleSegmentId).FirstOrDefault());
                var segmentPriceTask = Task.Run(() => unitOfWork.ParkingVehicleSegmentPrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToList());
                var discountTask = Task.Run(() => GetLicensePlateDiscountPercent(ticket.EnLicensePlate ?? "_"));
                var cardTask = Task.Run(() => unitOfWork.Cards.Find(c => c.CardSerialNo == ticket.CardUid).FirstOrDefault());
                var segmentVariablePriceTask = Task.Run(() => unitOfWork.ParkingVehicleSegmentVariablePrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToList());


                await Task.WhenAll(segmentTask, segmentPriceTask, discountTask);
                var segment = await segmentTask;
                var segmentPrices = await segmentPriceTask;
                var segmentVariablePrice = await segmentVariablePriceTask;

                if (segmentPrices != null)
                {
                    List<(int, int, int)> hourlyrate = new List<(int, int, int)>(); // Initialize the list
                    foreach (var item in segmentPrices)
                    {
                        hourlyrate.Add((item.TimeFrom.Hour, item.TimeTo.Hour, (int)item.HourlyRate));
                    }
                    var discount = await discountTask;

                    var card = await cardTask;

                    if (card != null)
                    {
                        if (card.PercentDiscount > 0)
                        {
                            discount = (short)card.PercentDiscount;
                        }
                    }

                    _parkingCostCalculator = new ParkingCostCalculator((int)segment.ParkingEntranceFixedFee,
                                                (int)segment.DailyRate,
                                                segment.FreeEntranceMinutes, segment.ThresholdNumberOfDays, segment.DailyPriceAfterCrossingThreshold, segment.ThresholdHoursPerDay,
                                                discount, segment.TaxPercentage, segmentPrices, segmentVariablePrice);



                    var result = _parkingCostCalculator.CalculateCost(ticket.StartTime, DateTime.Now);


                    TimeSpan varTime = DateTime.Now - ticket.StartTime;
                    string description = $"{varTime.Days} روز و {varTime.Hours} ساعت و {varTime.Minutes} دقیقه در {segment.NameFa}";
                    if (ticket.CardUid != null)
                    {

                        if (card != null)
                        {

                            if (card.PercentDiscount > 0)
                            {
                                description = description + " | " + $"کارت دارای تخفیف {card.PercentDiscount} درصدی میباشد ";
                            }
                            if (card.FixDiscount > 0)
                            {
                                if (ticket.TotalAmount > card.FixDiscount)
                                {
                                    result.PayableAmount = result.PayableAmount - card.FixDiscount;
                                }
                                else
                                {
                                    result.PayableAmount = 0;
                                }
                                description = description + " | " + $"کارت دارای تخفیف {card.FixDiscount} ريال میباشد ";
                            }

                        }
                    }

                    unitOfWork.ParkingTickets.ExecuteUpdate(p => p.Id == ticketId, update => update
                    .SetProperty(p => p.DurationMinutes, (int)varTime.TotalMinutes)
                    .SetProperty(ticket => ticket.DiscountPercent, ticket => (byte)discount)
                    .SetProperty(ticket => ticket.TotalAmount, ticket => result.PayableAmount)
                    .SetProperty(ticket => ticket.Description, ticket => description)
                    .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Unsynced));

                    //unitOfWork.Commit();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    public short GetLicensePlateDiscountPercent(string licenseEnPlate)
    {
        try
        {
            var licensePlate = unitOfWork.LicensePlates
                .Find(l => l.EnLicensePlate == licenseEnPlate)
                .FirstOrDefault();

            if (licensePlate == null)
                return 0;

            var now = DateTime.Now;

            var validGroup = unitOfWork.LicensePlateGroups
                .Find(g =>
                    g.LicensePlates.Any(x => x.EnLicensePlate == licenseEnPlate) &&
                    g.StartDate <= now &&
                    g.EndDate >= now
                )
                .OrderByDescending(g => g.StartDate)
                .ThenByDescending(g => g.EndDate)
                .FirstOrDefault();

            return validGroup?.DiscountPercent ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return 0;
        }
    }
    public async Task<short> GetLicensePlateDiscountPercentAsync(string licenseEnPlate)
    {
        try
        {
            var licensePlate = await unitOfWork.LicensePlates
                .Find(l => l.EnLicensePlate == licenseEnPlate)
                .FirstOrDefaultAsync();

            if (licensePlate == null)
                return 0;

            var now = DateTime.Now;

            var validGroup = await unitOfWork.LicensePlateGroups
                .Find(g =>
                    g.LicensePlates.Any(x => x.EnLicensePlate == licenseEnPlate) &&
                    g.StartDate <= now &&
                    g.EndDate >= now
                )
                .OrderByDescending(g => g.StartDate)
                .ThenByDescending(g => g.EndDate)
                .FirstOrDefaultAsync();

            return validGroup?.DiscountPercent ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;  // Return 0 in case of any error
        }
    }


    public VehicleSegmentModel? GetVehicleSegmentById(int Id)
    {
        try
        {
            var vs = unitOfWork.VehicleSegments.GetById(Id);
            if (vs != null)
            {
                VehicleSegmentModel vsInfo = new VehicleSegmentModel()
                {
                    Id = vs.Id,
                    CreatorUserId = vs.CreatorUserId,
                    DailyPriceAfterCrossingThreshold = vs.DailyPriceAfterCrossingThreshold,
                    DailyRate = vs.DailyRate,
                    Description = vs.Description,
                    FreeEntranceMinutes = vs.FreeEntranceMinutes,
                    Image = vs.Image,
                    NameFa = vs.NameFa,
                    ParkingEntranceFixedFee = vs.ParkingEntranceFixedFee,
                    ParkingLotId = vs.ParkingLotId,
                    TaxPercentage = vs.TaxPercentage,
                    ThresholdHoursPerDay = vs.ThresholdHoursPerDay,
                    ThresholdNumberOfDays = vs.ThresholdNumberOfDays
                };
                return vsInfo;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public string GetVehicleSegmentNameById(int Id)
    {
        try
        {
            var ticket = _vehicleSegmentsList.FirstOrDefault(i => i.Id == Id);
            if (ticket != null)
            {
                return ticket.NameFa ?? "- -";
            }
            return "-";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return "";
        }

    }

    public LicensePlateGroupModel? GetLicensePlateGroupByPlate(string licenseEnPlate)
    {
        try
        {

            var licensePlate = unitOfWork.LicensePlates.Find(l => l.EnLicensePlate == licenseEnPlate).FirstOrDefault();
            if (licensePlate == null)
            {
                return new LicensePlateGroupModel();
            }
            var licensePlateGroup = unitOfWork.LicensePlateGroups.Find(l => l.Id == licensePlate.GroupId)
                .Select(l => new LicensePlateGroupModel
                {
                    Id = l.Id,
                    DiscountPercent = l.DiscountPercent,
                    CreatorUserId = l.CreatorUserId,
                    Description = l.Description,
                    EndDate = l.EndDate,
                    IsActive = l.IsActive,
                    Name = l.Name,
                    StartDate = l.StartDate
                }).FirstOrDefault();
            return licensePlateGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public LicensePlateGroupModel? GetLicensePlateGroup(Guid id)
    {
        try
        {
            var licensePlateGroup = unitOfWork.LicensePlateGroups.Find(l => l.Id == id)

                .Select(l => new LicensePlateGroupModel
                {
                    Id = l.Id,
                    DiscountPercent = l.DiscountPercent,
                    CreatorUserId = l.CreatorUserId,
                    Description = l.Description,
                    EndDate = l.EndDate,
                    IsActive = l.IsActive,
                    Name = l.Name,
                    StartDate = l.StartDate
                }).FirstOrDefault();
            return licensePlateGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public async Task<List<LicensePlateGroupModel>> GetLicensePlateList(int Page = 1, int Take = 10, string? q = "")
    {
        var query = unitOfWork
            .LicensePlateGroups
            .GetAll()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.Name.Contains(q));
        }

        query = query.OrderBy(x => x.Name);

        if (Page > 0 && Take > 0)
            query = query.Skip((Page - 1) * Take).Take(Take);

        var result = await query
            .Select(l => new LicensePlateGroupModel
            {
                Id = l.Id,
                Name = l.Name,
                DiscountPercent = l.DiscountPercent,
                CreatorUserId = l.CreatorUserId,
                Description = l.Description,
                EndDate = l.EndDate,
                IsActive = l.IsActive,
                StartDate = l.StartDate,
                LicensePlates = l.LicensePlates.Select(ll => new LicensePlateModel
                {
                    Id = ll.Id,
                    EnLicensePlate = ll.EnLicensePlate,
                    FaLicensePlate = ll.FaLicensePlate,
                    GroupId = (Guid)ll.GroupId
                }).ToList()
            })
            .ToListAsync();

        return result;
    }



    public (List<LicensePlateListItemViewModel> Data, int TotalCount) GetLicensePlateGroupList(string? EnLicensePlate, int Page, int PageSize, Guid? SelectedGroupId)
    {
        try
        {
            var licensePlateGroupList = unitOfWork
               .LicensePlateGroups
               .GetAll()
               .Select(l => new LicensePlateGroupModel
               {
                   Id = l.Id,
                   DiscountPercent = l.DiscountPercent,
                   CreatorUserId = l.CreatorUserId,
                   Description = l.Description,
                   EndDate = l.EndDate,
                   IsActive = l.IsActive,
                   Name = l.Name,
                   StartDate = l.StartDate,
                   LicensePlates = l.LicensePlates.Select(ll => new LicensePlateModel
                   {
                       Id = ll.Id,
                       EnLicensePlate = ll.EnLicensePlate,
                       FaLicensePlate = ll.FaLicensePlate,
                       GroupId = (Guid)ll.GroupId
                   }).ToList()
               });

            if (EnLicensePlate != null)
            {
                licensePlateGroupList = licensePlateGroupList.Where(g => g.LicensePlates.Any(lp => lp.EnLicensePlate == EnLicensePlate));
            }

            if (SelectedGroupId != null)
            {
                licensePlateGroupList = licensePlateGroupList.Where(g => g.LicensePlates.Any(lp => lp.GroupId == SelectedGroupId));
            }

            var x = licensePlateGroupList.ToList();
            List<LicensePlateListItemViewModel> list = new List<LicensePlateListItemViewModel>();
            foreach (var item in licensePlateGroupList)
            {
                foreach (var sub in item?.LicensePlates)
                {
                    list.Add(new LicensePlateListItemViewModel
                    {
                        Id = sub.Id,
                        Name = item.Name,
                        Description = item.Description,
                        DiscountPercent = item.DiscountPercent,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        FaLicensePlate = sub.FaLicensePlate,
                        IsActive = item.IsActive,
                        StartDateString = item.StartDate.ToLongShamsiString(),
                        EndDateString = item.EndDate.ToLongShamsiString()
                    });
                }
            }


            int TotalCount = list.Count();

            list = list.Skip((Page - 1) * PageSize)
                       .Take(PageSize)
                       .ToList();


            return (list, TotalCount);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (new List<LicensePlateListItemViewModel>(), 0);
        }
    }

    public bool IsSeizedLicensePlate(string licenseEnPlate)
    {
        try
        {
            var plate = unitOfWork.SeizedLicensePlates.Find(s => s.EnLicensePlate == licenseEnPlate).Any();
            return plate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }

    }

    public List<SeizedLicensePlateModel> GetSeizedLicensePlatesList()
    {
        try
        {
            var licensePlateGroupList = unitOfWork.SeizedLicensePlates
               .GetAll()
               .Select(s => new SeizedLicensePlateModel
               {
                   Id = s.Id,
                   CreateDate = s.CreateDate,
                   CreatorUserId = s.CreatorUserId,
                   EnLicensePlate = s.EnLicensePlate,
                   FaLicensePlate = s.FaLicensePlate,
                   SeizedReason = s.SeizedReason,
                   IsLocal = s.IsLocal,
                   CreateDateShamsi = s.CreateDate.ToLongShamsiString(),
               })
               .ToList();
            return licensePlateGroupList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<SeizedLicensePlateModel>();
        }
    }

    public Guid? GetGroupIdByEnLicensePlate(string enLicensePlate)
    {
        try
        {
            var licensePlate = unitOfWork.LicensePlates.Find(g => g.EnLicensePlate == enLicensePlate).FirstOrDefault();
            if (licensePlate != null)
            {
                var group = unitOfWork.LicensePlateGroups.Find(g => g.Id == licensePlate.GroupId && g.StartDate < DateTime.Now && g.EndDate > DateTime.Now).FirstOrDefault();
                if (group != null)
                {
                    return group.Id;
                }
                return null;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public async Task<bool> SetTicketPaidInfo(TicketPaidInfoModel request)
    {
        try
        {

            await unitOfWork.ParkingTickets.ExecuteUpdateAsync(g => g.Id == request.TicketId, update => update
                                                   .SetProperty(ticket => ticket.IsExited, product => true)
                                                   .SetProperty(ticket => ticket.EndTime, product => DateTime.Now)
                                                   .SetProperty(ticket => ticket.IsPaid, product => true)
                                                   .SetProperty(ticket => ticket.PaidAmount, product => request.PaidAmount)
                                                   .SetProperty(ticket => ticket.TotalAmount, ticket => request.TotalAmount ?? ticket.TotalAmount)
                                                   .SetProperty(ticket => ticket.PaidCreditCard, product => request.PaidCreditCard.Replace(@"\0", ""))
                                                   .SetProperty(ticket => ticket.RefId, product => request.RefId)
                                                   .SetProperty(ticket => ticket.IsCustomPaid, product => request.IsCustomPaid)
                                                   .SetProperty(ticket => ticket.PaidType, product => request.PaidType)
                                                   .SetProperty(ticket => ticket.PaidDate, product => request.PaidDate)
                                                   .SetProperty(ticket => ticket.RRN, product => request.RRN)
                                                   .SetProperty(ticket => ticket.TraceNo, product => request.TraceNo)
                                                   .SetProperty(ticket => ticket.DeviceId, product => Settings.Default.Application_DeviceId)
                                                   .SetProperty(ticket => ticket.MerchantNumber, product => request.MerchantNumber)
                                                   .SetProperty(ticket => ticket.ExitGate, product => request.ExitGate)
                                                   .SetProperty(ticket => ticket.ExitImage, product => request.ExitImage)
                                                   .SetProperty(ticket => ticket.IsCardMissing, product => request.IsMissingCard)
                                                   .SetProperty(ticket => ticket.ExitRegistrarUserId, product => request.ExitRegistrarUserId)
                                                   .SetProperty(ticket => ticket.TicketStatus, product => TicketStatus.Unsynced));
            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == request.CardUid, update => update.SetProperty(s => s.IsInUse, false));
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public LicensePlateGroup? GetLicensePlateGroupById(Guid id)
        => unitOfWork.LicensePlateGroups.GetById(id);

    public decimal GetCardCreditAsync(long cardSerialNo)
    {
        try
        {

            return (unitOfWork.Cards.Find(s => s.CardSerialNo == cardSerialNo).FirstOrDefault())?.Credit ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }

    public CardModel? GetCardInfo(long cardSerialNo)
    {
        try
        {

            return unitOfWork.Cards.Find(s => s.CardSerialNo == cardSerialNo).Select(s => new CardModel
            {
                Id = s.Id,
                Credit = s.Credit,
                CardSerialNo = s.CardSerialNo,
                ActiveDate = s.ActiveDate,
                DeactiveDate = s.DeactiveDate,
                FixDiscount = s.FixDiscount,
                OwnerAddress = s.OwnerAddress,
                OwnerFirstName = s.OwnerFirstName,
                OwnerLastName = s.OwnerLastName,
                OwnerNationalCode = s.OwnerNationalCode,
                OwnerPic = s.OwnerPic,
                PercentDiscount = s.PercentDiscount,
                IsGuest = s.IsGuest,
                IsActive = s.IsActive,
                LicensePlateGroupId = s.LicensePlateGroupId,
                VehicleSegmentId = s.VehicleSegmentId,
                EnLicensePlate = s.EnLicensePlate
            }).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public VehicleSegment? GetCardVehicleSegment(long cardSerialNo)
    {
        try
        {
            var card = unitOfWork.Cards.Find(s => s.CardSerialNo == cardSerialNo).FirstOrDefault();
            if (card != null)
            {
                return unitOfWork.VehicleSegments.Find(s => s.Id == card.VehicleSegmentId).FirstOrDefault();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public LicensePlateGroup? GetCardLicensePlateGroup(long cardSerialNo)
    {
        try
        {

            var card = unitOfWork.Cards.FirstOrDefault(s => s.CardSerialNo == cardSerialNo);
            if (card != null)
            {
                return unitOfWork.LicensePlateGroups.Find(s => s.Id == card.LicensePlateGroupId).FirstOrDefault();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public bool AddCardToGroup(long cardSerialNo, Guid groupId)
    {
        try
        {

            var group = unitOfWork.LicensePlateGroups.FirstOrDefault(s => s.Id == groupId);
            if (group == null)
            {
                return false;
            }
            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == cardSerialNo, update => update.SetProperty(s => s.LicensePlateGroupId, groupId));
            //unitOfWork.Commit();
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }
    public bool AddCardToVehicleSegment(long cardSerialNo, int vehicleSegmentId)
    {
        try
        {

            var vehicleSegment = unitOfWork.VehicleSegments.Find(s => s.Id == vehicleSegmentId).FirstOrDefault();
            if (vehicleSegment == null)
            {
                return false;
            }
            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == cardSerialNo, update => update.SetProperty(s => s.VehicleSegmentId, vehicleSegmentId));
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }
    public bool IsGuestCard(long cardSerialNo)
    {
        try
        {

            var card = unitOfWork.Cards.Find(s => s.CardSerialNo == cardSerialNo).FirstOrDefault();

            if (card != null)
            {
                return card.IsGuest;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }

    }
    public bool CardActiveStatus(long cardSerialNo)
    {
        try
        {
            var card = unitOfWork.Cards.FirstOrDefault(s => s.CardSerialNo == cardSerialNo);

            if (card != null)
            {
                if (card.ActiveDate < DateTime.Now && DateTime.Now < card.DeactiveDate && card.IsActive)
                    return card.IsActive;
                return false;
            }
            return false;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }
    public bool CardExistStatus(long cardSerialNo)
    {
        try
        {

            var card = unitOfWork.Cards.FirstOrDefault(s => s.CardSerialNo == cardSerialNo);

            if (card != null)
                return true;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public int GetCardDiscountPercent(long cardSerialNo)
    {
        try
        {

            var card = unitOfWork.Cards.FirstOrDefault(s => s.CardSerialNo == cardSerialNo);

            if (card != null)
                return card.PercentDiscount;
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }

    public List<CardCreditHistoryModel> GetCardCreditHistory(long cardSerialNo, int page, int pageSize)
    {
        try
        {
            var cardCredits = unitOfWork.CardCreditHistories.Find(s => s.CardSerialNo == cardSerialNo).Select(s => new CardCreditHistoryModel
            {
                Amount = s.Amount,
                CardId = s.CardId,
                CardSerialNo = s.CardSerialNo,
                CreateDate = s.CreateDate,
                Description = s.Description,
                Id = s.Id,
                Type = s.Type,
                TypeName = s.Type == 1 ? "افزایش اعتبار" : "کاهش اعتبار"
            }).ToList();
            return cardCredits;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<CardCreditHistoryModel>();
        }
    }

    public List<TicketsListViewModel> GetCardUsageHistory(long cardSerialNo)
    {
        try
        {

            var tickets = unitOfWork.ParkingTickets.Find(s => s.CardUid == cardSerialNo).Select(s => new TicketsListViewModel
            {
                BarcodeId = s.BarcodeId,
                Id = s.Id,
                EndTime = s.EndTime,
                LicensePlate = s.LicensePlate,
                ParkingSpaceID = s.ParkingSpaceID,
                StartTime = s.StartTime,
                VehicleManufacturerName = s.VehicleManufacturerName,
                VehicleSegmentId = s.VehicleSegmentId,
                StartRelativeTimeString = s.StartTime.ToRelativeDate(),
                StartTimeString = s.StartTime.ToLongShamsiString(),
                StartTimeOnlyString = s.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                EndTimeString = s.EndTime.ToLongShamsiString(),
                VehicleSegmentName = GetVehicleSegmentNameById((short)s.VehicleSegmentId) ?? "",
                Discount = s.Discount,
                DiscountPercent = s.DiscountPercent,
                DurationMinutes = s.DurationMinutes,
                EndTimeOnlyString = (s.EndTime != null) ? ((DateTime)s.EndTime).ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ") : "",
                IsExited = s.IsExited,
                IsPaid = s.IsPaid,
                PaidAmount = s.PaidAmount,
                PaidCreditCard = s.PaidCreditCard,
                PaidType = s.PaidType,
                RefId = s.RefId,
                TotalAmount = s.TotalAmount,
                ParkingName = ParkingLotInfoStore.ParkingInfo.Name,
                ParkingLotId = s.ParkingLotId,
                EnLicensePlate = s.EnLicensePlate,
                Description = s.Description,
                LicensePlateGroupId = s.LicensePlateGroupId,
                RRN = s.RRN,
                PaidDate = s.PaidDate,
                TraceNo = s.TraceNo,
                MerchantNumber = s.MerchantNumber
            }).ToList();
            return tickets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketsListViewModel>();
        }
    }

    public bool AddCard(CardModel request)
    {
        try
        {
            var cardInfo = unitOfWork.Cards.FirstOrDefault(c => c.CardSerialNo == request.CardSerialNo);
            if (cardInfo == null)
            {
                Card card = new Card
                {
                    ActiveDate = request.ActiveDate,
                    CardSerialNo = request.CardSerialNo,
                    Credit = request.Credit,
                    DeactiveDate = request.DeactiveDate,
                    FixDiscount = request.FixDiscount,
                    IsActive = request.IsActive,
                    IsGuest = request.IsGuest,
                    OwnerAddress = request.OwnerAddress,
                    OwnerFirstName = request.OwnerFirstName,
                    OwnerLastName = request.OwnerLastName,
                    OwnerNationalCode = request.OwnerNationalCode,
                    OwnerPhoneNumber = request.OwnerPhoneNumber,
                    OwnerPic = request.OwnerPic,
                    PercentDiscount = request.PercentDiscount,
                    LicensePlateGroupId = request.LicensePlateGroupId,
                    VehicleSegmentId = request.VehicleSegmentId,
                    EnLicensePlate = request.EnLicensePlate,
                };
                unitOfWork.Cards.Add(card);

                var carditemresult = CreateAddCardHistory(new AddCardItemModel
                {
                    EnLicensePlate = card.EnLicensePlate,
                    ActiveDate = card.ActiveDate,
                    CreateDate = DateTime.Now,
                    DeactiveDate = card.DeactiveDate,
                    OwnerFullName = card.OwnerFirstName + " " + card.OwnerLastName,
                    Description = $"کد ملی: {card.OwnerNationalCode} _ شماره همراه: {card.OwnerPhoneNumber} ",
                    PercentDiscount = card.PercentDiscount,
                    VehicleSegmentId = card.VehicleSegmentId,
                    CardUid = (long)card.CardSerialNo
                });

                return true;
            }
            else
            {
                Card? card = unitOfWork.Cards.GetById(cardInfo.Id);
                if (card is null)
                    return false;

                card.ActiveDate = request.ActiveDate;
                card.CardSerialNo = request.CardSerialNo;
                card.Credit = request.Credit;
                card.DeactiveDate = request.DeactiveDate;
                card.FixDiscount = request.FixDiscount;
                card.IsActive = request.IsActive;
                card.IsGuest = request.IsGuest;
                card.OwnerAddress = request.OwnerAddress;
                card.OwnerFirstName = request.OwnerFirstName;
                card.OwnerLastName = request.OwnerLastName;
                card.OwnerNationalCode = request.OwnerNationalCode;
                card.OwnerPic = request.OwnerPic;
                card.PercentDiscount = request.PercentDiscount;
                card.LicensePlateGroupId = request.LicensePlateGroupId;
                card.VehicleSegmentId = request.VehicleSegmentId;
                card.EnLicensePlate = request.EnLicensePlate;
                card.OwnerPhoneNumber = request.OwnerPhoneNumber;

                unitOfWork.Cards.Update(card);

                var carditemresult = CreateAddCardHistory(new AddCardItemModel
                {
                    EnLicensePlate = card.EnLicensePlate,
                    ActiveDate = card.ActiveDate,
                    CreateDate = DateTime.Now,
                    DeactiveDate = card.DeactiveDate,
                    OwnerFullName = card.OwnerFirstName + " " + card.OwnerLastName,
                    Description = $"کد ملی: {card.OwnerNationalCode} _ شماره همراه: {card.OwnerPhoneNumber}",
                    PercentDiscount = card.PercentDiscount,
                    VehicleSegmentId = card.VehicleSegmentId,
                    CardUid = (long)card.CardSerialNo
                });

                return true;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public bool AddCardCreditHistory(CardCreditHistoryModel request)
    {
        try
        {

            CardCreditHistory cardCreditHistory = new CardCreditHistory
            {
                Amount = request.Amount,
                CardId = request.CardId,
                CardSerialNo = request.CardSerialNo,
                CreateDate = request.CreateDate,
                Description = request.Description,
                Type = request.Type
            };
            unitOfWork.CardCreditHistories.Add(cardCreditHistory);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public List<VehicleSegmentModel> GetVehicleSegments()
    {
        try
        {

            var segmentsList = unitOfWork.VehicleSegments.GetAll().Select(v => new VehicleSegmentModel
            {
                Id = v.Id,
                Description = v.Description,
                NameFa = v.NameFa,
                Image = v.Image,
                CreatorUserId = v.CreatorUserId,
                DailyRate = v.DailyRate,
                FreeEntranceMinutes = v.FreeEntranceMinutes,
                ParkingEntranceFixedFee = v.ParkingEntranceFixedFee,
                ParkingLotId = v.ParkingLotId,
                PlateType = v.PlateType
            });
            return segmentsList.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<VehicleSegmentModel>();
        }
    }

    public int GetCardsCount()
    {
        try
        {

            return unitOfWork.Cards.GetAll().Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }

    public bool IsCardInUse(long cardSerialNo)
    {
        try
        {
            //using (var uow = _unitOfWorkFactory.Create())
            return unitOfWork.Cards.Find(c => c.CardSerialNo == cardSerialNo && c.IsInUse).Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public Guid? GetNotExitedTicketIdByCardSerialNo(long cardSerialNo)
    {
        try
        {
            var ticket = unitOfWork.ParkingTickets.Find(c => c.CardUid == cardSerialNo && c.IsExited == false)
                .OrderByDescending(c => c.StartTime)
                .Select(c => new { Id = c.Id, IsPaid = c.IsPaid })
                .FirstOrDefault();
            if (ticket != null) return ticket.Id;
            else return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public bool AddTicketExtraImage(Guid TicketId, string Image, string Name, bool ShowInPage)
    {
        try
        {
            ParkingTicketExtraImage parkingTicketExtraImage = new ParkingTicketExtraImage()
            {
                Image = Image,
                FaName = Name,
                GateName = Settings.Default.Application_GatePCName ?? System.Environment.MachineName,
                CreateDateTime = DateTime.Now,
                ShowInPage = ShowInPage,
                TicketId = TicketId
            };
            unitOfWork.ParkingTicketExtraImages.Add(parkingTicketExtraImage);
            unitOfWork.ParkingTicketExtraImages.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public async Task<bool> AddTicketExtraImageAsync(Guid TicketId, string Image, string Name, bool ShowInPage)
    {
        try
        {
            ParkingTicketExtraImage parkingTicketExtraImage = new ParkingTicketExtraImage()
            {
                Image = Image,
                FaName = Name,
                GateName = Settings.Default.Application_GatePCName ?? System.Environment.MachineName,
                CreateDateTime = DateTime.Now,
                ShowInPage = ShowInPage,
                TicketId = TicketId
            };
            await unitOfWork.ParkingTicketExtraImages.AddAsync(parkingTicketExtraImage);
            await unitOfWork.ParkingTicketExtraImages.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public bool DeleteTicketExtraImage(Guid TicketId, string Name)
    {
        try
        {
            var result = unitOfWork.ParkingTicketExtraImages.ExecuteDelete(p => p.TicketId == TicketId && p.FaName == Name);

            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public async Task<bool> DeleteTicketExtraImageAsync(Guid TicketId, string Name)
    {
        try
        {
            var result = await unitOfWork.ParkingTicketExtraImages.ExecuteDeleteAsync(p => p.TicketId == TicketId && p.FaName == Name);

            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public List<ParkingTicketExtraImageModel> GetTicketExtraImages(Guid TicketId, bool? ShowInBox)
    {
        try
        {
            var result = unitOfWork.ParkingTicketExtraImages.Find(p => p.TicketId == TicketId);
            if (ShowInBox != null)
            {
                result = result.Where(r => r.ShowInPage == ShowInBox);
            }
            return result.Select(p => new ParkingTicketExtraImageModel
            {
                TicketId = p.TicketId,
                ShowInPage = p.ShowInPage,
                CreateDateTime = p.CreateDateTime,
                FaName = p.FaName,
                GateName = p.GateName,
                Image = p.Image
            }).ToList();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<ParkingTicketExtraImageModel>();
        }
    }

    public async Task<List<ParkingTicketExtraImageModel>> GetTicketExtraImagesAsync(Guid TicketId, bool? ShowInBox)
    {
        try
        {
            var result = unitOfWork.ParkingTicketExtraImages.Find(p => p.TicketId == TicketId);
            if (ShowInBox != null)
            {
                result = result.Where(r => r.ShowInPage == ShowInBox);
            }
            return await result.Select(p => new ParkingTicketExtraImageModel
            {
                TicketId = p.TicketId,
                ShowInPage = p.ShowInPage,
                CreateDateTime = p.CreateDateTime,
                FaName = p.FaName,
                GateName = p.GateName,
                Image = p.Image
            }).ToListAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<ParkingTicketExtraImageModel>();
        }
    }

    public List<ParkingTicketExtraImageSourceModel> GetTicketExtraImageSources(Guid TicketId, bool? ShowInBox)
    {
        List<ParkingTicketExtraImageSourceModel> result = new List<ParkingTicketExtraImageSourceModel>();
        try
        {
            var images = unitOfWork.ParkingTicketExtraImages.Find(p => p.TicketId == TicketId);
            if (ShowInBox != null)
            {
                images = images.Where(r => r.ShowInPage == ShowInBox);
            }
            foreach (var p in images)
            {

                if (string.IsNullOrEmpty(p.Image))

                    if (IsValidUrl(p.Image))
                        result.Add(new ParkingTicketExtraImageSourceModel
                        {
                            FaName = p.FaName,
                            GateName = p.GateName,
                            CreateDateTime = p.CreateDateTime,
                            ShowInPage = p.ShowInPage,
                            TicketId = TicketId,
                            ImageSource = new BitmapImage(new Uri(p.Image, UriKind.Absolute))
                        });

                if (IsBase64(p.Image))
                    result.Add(new ParkingTicketExtraImageSourceModel
                    {
                        FaName = p.FaName,
                        GateName = p.GateName,
                        CreateDateTime = p.CreateDateTime,
                        ShowInPage = p.ShowInPage,
                        TicketId = TicketId,
                        ImageSource = ImageHelper.Base64ToImageSource(p.Image)
                    });

            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return result;
        }
    }

    public async Task<List<ParkingTicketExtraImageSourceModel>> GetTicketExtraImageSourcesAsync(Guid TicketId, bool? ShowInBox)
    {
        List<ParkingTicketExtraImageSourceModel> result = new List<ParkingTicketExtraImageSourceModel>();
        try
        {
            var images = unitOfWork.ParkingTicketExtraImages.Find(p => p.TicketId == TicketId);
            if (ShowInBox != null)
            {
                images = images.Where(r => r.ShowInPage == ShowInBox);
            }
            var x = await images.ToListAsync();
            foreach (var p in (await images.ToListAsync()))
            {

                if (string.IsNullOrEmpty(p.Image))
                    break;

                if (IsValidUrl(p.Image))
                {
                    client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
                    var response = await client.GetAsync(new Uri(p.Image));
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    result.Add(new ParkingTicketExtraImageSourceModel
                    {
                        FaName = p.FaName,
                        GateName = p.GateName,
                        CreateDateTime = p.CreateDateTime,
                        ShowInPage = p.ShowInPage,
                        TicketId = TicketId,
                        ImageSource = bitmap
                    });

                }


                if (IsBase64(p.Image))
                    result.Add(new ParkingTicketExtraImageSourceModel
                    {
                        FaName = p.FaName,
                        GateName = p.GateName,
                        CreateDateTime = p.CreateDateTime,
                        ShowInPage = p.ShowInPage,
                        TicketId = TicketId,
                        ImageSource = ImageHelper.Base64ToImageSource(p.Image)
                    });

            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return result;
        }
    }

    private static bool IsValidUrl(string input)
    {
        return Uri.TryCreate(input, UriKind.Absolute, out _);
    }

    private static bool IsBase64(string input)
    {
        string pattern = @"^data:image\/(png|jpeg|jpg|gif);base64,";
        return Regex.IsMatch(input, pattern) || (input.Length % 4 == 0 && Convert.TryFromBase64String(input, new Span<byte>(new byte[input.Length]), out _));
    }

    public int GetDiscountedCardsCount()
    {
        try
        {
            return unitOfWork.Cards.Find(c => c.PercentDiscount > 0).Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }

    public (bool Result, string ResultMSG) CreateAddCardHistory(AddCardItemModel request)
    {
        try
        {
            AddCardItem CardItem = new AddCardItem();
            CardItem.OwnerFullName = request.OwnerFullName;
            CardItem.EnLicensePlate = request.EnLicensePlate;
            CardItem.PercentDiscount = request.PercentDiscount;
            CardItem.Description = request.Description;
            CardItem.ActiveDate = request.ActiveDate;
            CardItem.DeactiveDate = request.DeactiveDate;
            CardItem.VehicleSegmentId = request.VehicleSegmentId;
            CardItem.CreateDate = request.CreateDate;
            CardItem.CardUid = request.CardUid;

            unitOfWork.AddCardItems.Add(CardItem);
            unitOfWork.AddCardItems.Commit();
            return (true, "ثبت در تاریخچه با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (false, "خطا در ثبت تاریخچه");
        }
    }

    public (List<AddCardItemModel> Result, int ResultCount, string ResultMSG) SearchInCardHistory(string? FullName, long? CardUid, string? EnLicensePlate, int? PercentDiscount, DateTime? StartCreateDate, DateTime? EndCreateDate, string? Description, int Page, int PageSize)
    {
        try
        {
            List<AddCardItemModel> Result = new List<AddCardItemModel>();

            var result = unitOfWork.AddCardItems.GetAll()
                .Select(a => new AddCardItemModel
                {
                    Id = a.Id,
                    ActiveDate = a.ActiveDate,
                    CreateDate = a.CreateDate,
                    DeactiveDate = a.DeactiveDate,
                    Description = a.Description,
                    EnLicensePlate = a.EnLicensePlate,
                    OwnerFullName = a.OwnerFullName,
                    PercentDiscount = a.PercentDiscount,
                    VehicleSegmentId = a.VehicleSegmentId,
                    CardUid = a.CardUid
                });

            if (FullName != null)
            {
                result = result.Where(r => r.OwnerFullName.Contains(FullName));
            }
            if (Description != null)
            {
                result = result.Where(r => r.Description.Contains(Description));
            }
            if (EnLicensePlate != null)
            {
                result = result.Where(r => r.EnLicensePlate == EnLicensePlate);
            }
            if (PercentDiscount != null)
            {
                result = result.Where(r => r.PercentDiscount == PercentDiscount);
            }
            if (StartCreateDate != null)
            {
                result = result.Where(r => r.CreateDate >= StartCreateDate);
            }
            if (EndCreateDate != null)
            {
                EndCreateDate = EndCreateDate.Value.AddDays(1);
                result = result.Where(r => r.CreateDate <= EndCreateDate);
            }
            if (CardUid != null)
            {
                result = result.Where(r => r.CardUid == CardUid);
            }
            return (result.Skip((Page - 1) * PageSize).Take(PageSize).ToList(), result.Count(), "جستجو در تاریخچه");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (new List<AddCardItemModel>(), 0, "خطا در جستجو در تاریخچه");
        }
    }

    public Guid? GetTicketIdByBarcode(long barcode)
    {
        var ticket = unitOfWork.ParkingTickets.Find(x => x.BarcodeId == barcode).FirstOrDefault();
        if (ticket is null)
            return Guid.Empty;
        return ticket.Id;
    }

    public List<TicketDescriptionItemModel> GetAllTicketDescriptionItems()
    {
        try
        {
            var items = unitOfWork.TicketDescriptionItems.GetAll().Select(t => new TicketDescriptionItemModel
            {
                Id = t.Id,
                CreateDate = t.CreateDate,
                IsQueueEnabled = t.IsQueueEnabled,
                Text = t.Text
            }).ToList();
            return items;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketDescriptionItemModel>();
        }
    }

    public TicketDescriptionItemModel? GetTicketDescriptionItemById(int id)
    {
        try
        {
            var item = unitOfWork.TicketDescriptionItems.GetById(id);
            if (item != null)
            {
                var model = new TicketDescriptionItemModel
                {
                    Id = item.Id,
                    CreateDate = item.CreateDate,
                    IsQueueEnabled = item.IsQueueEnabled,
                    Text = item.Text
                };
                return model;
            }
            return null;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public bool AddTicketDescriptionItem(TicketDescriptionItemModel request)
    {
        try
        {
            TicketDescriptionItem item = new TicketDescriptionItem
            {
                CreateDate = DateTime.Now,
                IsQueueEnabled = request.IsQueueEnabled,
                Text = request.Text
            };
            unitOfWork.TicketDescriptionItems.Add(item);
            unitOfWork.TicketDescriptionItems.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public bool UpdateTicketDescriptionItem(TicketDescriptionItemModel request)
    {
        try
        {
            var item = unitOfWork.TicketDescriptionItems.GetById(request.Id);
            if (item != null)
            {
                unitOfWork.TicketDescriptionItems.ExecuteUpdate(p => p.Id == request.Id, update => update
            .SetProperty(p => p.IsQueueEnabled, request.IsQueueEnabled)
            .SetProperty(p => p.Text, request.Text)
            );
                //unitOfWork.TicketDescriptionItems.Update(item);
                //unitOfWork.TicketDescriptionItems.Commit();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }


    public async Task<bool> ResetTicketDescriptionInterval(int Id)
    {
        try
        {
            var item = unitOfWork.TicketDescriptionItems.GetById(Id);
            if (item != null)
            {
                await _ticketQueueService.ResetQueueAsync(Id);
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public bool ChangeTicketDescriptionItemQueueStatus(int id, bool status)
    {
        try
        {
            var item = unitOfWork.TicketDescriptionItems.GetById(id);
            if (item != null)
            {
                item.IsQueueEnabled = status;
                //unitOfWork.TicketDescriptionItems.Update(item);
                unitOfWork.TicketDescriptionItems.ExecuteUpdate(p => p.Id == id, update => update
                            .SetProperty(p => p.IsQueueEnabled, status));
                //_ticketQueueService.SetQueueEnabledAsync(item.Id, status);
                if (status == true)
                {
                    _ticketQueueService.SetResetIntervalAsync(item.Id, 30);
                }
                unitOfWork.TicketDescriptionItems.Commit();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public (bool IsSuccess, bool IsExsist) DeleteTicketDescriptionItem(int id)
    {
        try
        {
            var item = unitOfWork.TicketDescriptionItems.GetById(id);
            if (item != null)
            {
                item.IsDeleted = true;
                unitOfWork.TicketDescriptionItems.Update(item);
                return (true, false);
            }
            return (false, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return (false, false);
        }
    }

    public List<string?> GetEntryRegistrars()
    {
        return unitOfWork.ParkingTickets.GetAll()
            .Where(t => !t.IsExited)
            .Select(t => t.EntranceGate)
            .Distinct()
            .ToList();
    }

    public List<string?> GetExitRegistrars()
    {
        return unitOfWork.ParkingTickets.GetAll()
            .Where(t => t.IsExited)
            .Select(t => t.ExitGate)
            .Distinct()
            .ToList();
    }

    public async Task<TicketSummaryReportModel> GetSummaryReport(GetTicketListRequestModel request)
    {
        var tickets = TicketListBaseQuery();


        if (request.ExitFrom != null)
        {
            var from = request.ExitFrom.Value.Date;
            tickets = tickets.Where(x =>
                x.EndTime.HasValue &&
                x.EndTime.Value >= from
            );
        }

        if (request.ExitTo != null)
        {
            var to = request.ExitTo.Value.Date.AddDays(1).AddTicks(-1);
            tickets = tickets.Where(x =>
                x.EndTime.HasValue &&
                x.EndTime.Value <= to
            );
        }

        if (!string.IsNullOrEmpty(request.EntryRegistrar))
            tickets = tickets.Where(x => x.EntranceGate == request.EntryRegistrar);

        if (!string.IsNullOrEmpty(request.ExitRegistrar))
            tickets = tickets.Where(x => x.ExitGate == request.ExitRegistrar);
        tickets = tickets.Where(x => x.IsExited == true && x.IsPaid == true);

        var result = new TicketSummaryReportModel()
        {
            TotalTickets = await tickets.CountAsync(),
            TotalAmount = await tickets.SumAsync(x => x.TotalAmount),
            TotalPaidAmount = await tickets.SumAsync(x => x.PaidAmount),
            TotalCreditPaid = await tickets.CountAsync(x => x.PaidType == "NAGHDI"),
            TotalPosPaid = await tickets.CountAsync(x => x.PaidType == "POS"),
            TotalCreditPaidAmount = await tickets
                .Where(x => x.PaidType == "NAGHDI")
                .SumAsync(x => x.PaidAmount),
            TotalPosPaidAmount = await tickets
                .Where(x => x.PaidType == "POS")
                .SumAsync(x => x.PaidAmount),
            TotalDiscountAmount = await tickets
                .SumAsync(x => (x.TotalAmount * x.DiscountPercent) / 100),
            TotalEntries = await tickets.CountAsync(x => !x.IsExited.Value),
            TotalExits = await tickets.CountAsync(x => x.IsExited.Value)
        };


        return result;
    }


    public async Task<(List<TicketsListViewModel> Data, int TotalCount)> GetTicketListReportAsync(GetTicketListRequestModel request)
    {
        try
        {
            IQueryable<TicketsListViewModel> tickets = TicketListBaseQuery();

            tickets = ApplyTicketListFilter(request, tickets);

            var totalCount = tickets.Count();

            var result = await tickets.ToListAsync();

            return (result, totalCount);
        }
        catch (Exception)
        {
            return (null, 0);
        }
    }

    public async Task<bool> DeleteSeizedVehicleAsync(Guid Id)
    {
        try
        {
            var existingPlate = unitOfWork.SeizedLicensePlates.FirstOrDefault(x => x.Id == Id);
            if (existingPlate == null)
                return false;
            await unitOfWork.SeizedLicensePlates.ExecuteDeleteAsync(x => x.Id == existingPlate.Id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public async Task<(bool Exists, bool IsSuccess)> AddSeizedVehicleAsync(string plate, string reason)
    {
        if (string.IsNullOrWhiteSpace(plate))
            return (false, false);

        var existingPlate = unitOfWork.SeizedLicensePlates
            .FirstOrDefault(x => x.EnLicensePlate == plate);

        if (existingPlate != null)
            return (true, false);

        var parsePlate = plate.ParsePlate();
        var seizedPlate = new SeizedLicensePlate
        {
            CreatorUserId = TokenStore.UserId,
            CreateDate = DateTime.Now,
            SeizedReason = reason,
            IsLocal = true,
            EnLicensePlate = plate,
            FaLicensePlate = parsePlate.IsIranianPlate ? "ایران" + parsePlate.IranCode.Replace("IR", "") + "_" + parsePlate.RightThreeDigits + parsePlate.Letter.ToLower()?.ConvertEnCharToFaCharIndex().Replace("ه", "هـ")
      + parsePlate.LeftTwoDigits : parsePlate.OriginalPlate
        };

        await unitOfWork.SeizedLicensePlates.AddAsync(seizedPlate);
        return (false, true);
    }

    public async Task<bool> AddLicensePlateGroup(LicensePlateGroup licensePlate)
    {
        try
        {
            await unitOfWork.LicensePlateGroups.AddAsync(new LicensePlateGroup()
            {
                Name = licensePlate.Name,
                ParkingLotId = TokenStore.ParkingLotId,
                Description = licensePlate.Description,
                DiscountPercent = licensePlate.DiscountPercent,
                StartDate = licensePlate.StartDate,
                EndDate = licensePlate.EndDate,
                IsActive = true,
                IsLocal = true
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public async Task<(bool IsSuccess, bool IsExsist)> AddLicensePlate(LicensePlate licensePlate)
    {
        try
        {
            var plate = unitOfWork.LicensePlates.FirstOrDefault(x => x.EnLicensePlate == licensePlate.EnLicensePlate);
            if (plate != null)
                return (false, true);

            var parsePlate = licensePlate.EnLicensePlate.ParsePlate();

            await unitOfWork.LicensePlates.AddAsync(new LicensePlate()
            {
                FaLicensePlate = parsePlate.IsIranianPlate ? "ایران" + parsePlate.IranCode.Replace("IR", "") + "_" + parsePlate.RightThreeDigits + parsePlate.Letter.ToLower()?.ConvertEnCharToFaCharIndex().Replace("ه", "هـ") + parsePlate.LeftTwoDigits : parsePlate.OriginalPlate,
                EnLicensePlate = licensePlate.EnLicensePlate,
                GroupId = licensePlate.GroupId,
                IsLocal = true
            });
            return (true, false);
        }
        catch (Exception)
        {
            return (false, false);
        }
    }
    public async Task<(bool IsSuccess, bool IsExsist)> DeleteLicensePlateGroup(Guid Id)
    {
        var licensePlateGroup = GetLicensePlateGroupById(Id);
        if (licensePlateGroup is null)
            return (false, false);

        var licensePlateAssigned = unitOfWork.LicensePlates.FirstOrDefault(x => x.GroupId == licensePlateGroup.Id);
        if (licensePlateAssigned != null)
            return (false, true);

        await unitOfWork.LicensePlateGroups.ExecuteDeleteAsync(x => x.Id == licensePlateGroup.Id);
        return (true, false);
    }

    public async Task<bool> DeleteLicensePlate(Guid Id)
    {
        var licensePlate = unitOfWork.LicensePlates.FirstOrDefault(x => x.Id == Id);
        if (licensePlate is null)
            return false;

        await unitOfWork.LicensePlates.ExecuteDeleteAsync(x => x.Id == licensePlate.Id);
        return true;
    }

    public async Task<bool> UpdateLicensePlateGroup(LicensePlateGroup licensePlateGroup)
    {
        try
        {
            await unitOfWork.LicensePlateGroups.ExecuteUpdateAsync(
                group => group.Id == licensePlateGroup.Id,
                g => g.SetProperty(x => x.Name, licensePlateGroup.Name)
              .SetProperty(x => x.Description, licensePlateGroup.Description)
              .SetProperty(x => x.DiscountPercent, licensePlateGroup.DiscountPercent)
              .SetProperty(x => x.StartDate, licensePlateGroup.StartDate)
              .SetProperty(x => x.EndDate, licensePlateGroup.EndDate));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }


    public async Task<int> GetActiveLicensePlateGroupCountAsync()
        => await unitOfWork.LicensePlateGroups.GetAll().CountAsync(x => x.IsActive);

    public async Task<int> GetInactiveLicensePlateGroupCountAsync()
        => await unitOfWork.LicensePlateGroups.GetAll().CountAsync(x => !x.IsActive);

    public async Task<int> GetTotalLicensePlateGroupCountAsync()
        => await unitOfWork.LicensePlateGroups.GetAll().CountAsync();

    public async Task<(List<LicensePlateGroupModel> Data, int TotalCount)> GetLicensePlatePaginatedList(
        int page,
        int pageSize,
        string? filterName,
        int? filterDiscount,
        DateTime? filterStartDate,
        DateTime? filterEndDate)
    {
        var baseQuery = unitOfWork
            .LicensePlateGroups
            .GetAll();


        if (!string.IsNullOrWhiteSpace(filterName))
            baseQuery = baseQuery.Where(l => l.Name.Contains(filterName));

        if (filterDiscount.HasValue)
            baseQuery = baseQuery.Where(l => l.DiscountPercent == filterDiscount.Value);

        if (filterStartDate.HasValue)
            baseQuery = baseQuery.Where(l => l.StartDate >= filterStartDate.Value);

        if (filterEndDate.HasValue)
            baseQuery = baseQuery.Where(l => l.EndDate <= filterEndDate.Value);

        var totalCount = await baseQuery.CountAsync();

        var pagedData = await baseQuery
            .OrderBy(l => l.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LicensePlateGroupModel
            {
                Id = l.Id,
                DiscountPercent = l.DiscountPercent,
                CreatorUserId = l.CreatorUserId,
                Description = l.Description,
                EndDate = l.EndDate,
                IsActive = l.IsActive,
                Name = l.Name,
                StartDate = l.StartDate,
                LicensePlates = l.LicensePlates.Select(ll => new LicensePlateModel
                {
                    Id = ll.Id,
                    EnLicensePlate = ll.EnLicensePlate,
                    FaLicensePlate = ll.FaLicensePlate,
                    GroupId = (Guid)ll.GroupId
                }).ToList()
            })
            .ToListAsync();

        return (pagedData, totalCount);
    }

    public async Task<LicensePlateGroupWithPlatesPaginatedResult> GetLicensePlateGroupWithPlatesPaginatedList(
    int page,
    int take,
    string? name,
    int? discount,
    DateTime? startDate,
    DateTime? endDate)
    {
        if (page < 1) page = 1;
        if (take <= 0) take = 10;

        var query = unitOfWork
            .LicensePlateGroups
            .GetAll()
            .Include(g => g.LicensePlates)
            .AsQueryable();

        // FILTER: NAME
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(g => g.Name != null && g.Name.Contains(name));

        // FILTER: DISCOUNT
        if (discount.HasValue)
            query = query.Where(g => g.DiscountPercent == discount.Value);

        // FILTER: DATE RANGE (only StartDate matters)
        if (startDate.HasValue)
        {
            var start = startDate.Value.Date; // 00:00:00
            query = query.Where(g => g.StartDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(g => g.StartDate <= end);
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(g => g.StartDate)
            .Skip((page - 1) * take)
            .Take(take)
            .Select(g => new LicensePlateGroupWithPlatesDto
            {
                Id = g.Id,
                Name = g.Name ?? string.Empty,
                Description = g.Description ?? string.Empty,
                DiscountPercent = g.DiscountPercent,
                IsActive = g.IsActive,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                Plates = g.LicensePlates != null
                    ? g.LicensePlates.Select(p => new LicensePlatePlateItemDto
                    {
                        Id = p.Id,
                        PersianPlate = p.FaLicensePlate ?? string.Empty,
                        EnglishPlate = p.EnLicensePlate ?? string.Empty
                    }).ToList()
                    : new List<LicensePlatePlateItemDto>()
            })
            .ToListAsync();

        var totalGroupsQuery = unitOfWork.LicensePlateGroups.GetAll();
        var totalGroups = await totalGroupsQuery.CountAsync();
        var totalActiveGroups = await totalGroupsQuery.Where(g => g.IsActive).CountAsync();
        var totalInactiveGroups = totalGroups - totalActiveGroups;

        return new LicensePlateGroupWithPlatesPaginatedResult
        {
            TotalCount = totalCount,
            TotalGroups = totalGroups,
            TotalActiveGroups = totalActiveGroups,
            TotalInactiveGroups = totalInactiveGroups,
            Data = data
        };
    }
}

