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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using ZXing;
using Card = Parking.Domain.Entities.Parkings.Card;
using RandomNumberGenerator = Parking.App.Helpers.RandomNumberGenerator;

namespace Parking.App.Services;

public class ParkingService : IParkingService
{
    private readonly ILogger<ParkingService> _logger;
    //private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IUnitOfWork unitOfWork;
    private IHttpClientFactory _httpClientFactory;
    private HttpClient client = new HttpClient();

    private ParkingCostCalculator? _parkingCostCalculator;
    private List<VehicleSegment> _vehicleSegmentsList;
    public ParkingService(ILogger<ParkingService> logger, IUnitOfWork _unitOfWork, IHttpClientFactory httpClientFactory)
    {
        //this.unitOfWork = unitOfWork;
        //_unitOfWorkFactory = unitOfWorkFactory;                                     Remove All Comments
        _logger = logger;

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
                //result.Sections = GetSectionsAsync();
                //result.ParkingSpaces = GetSpacesAsync();
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
            //var vehicleSegmentsTask = Task.Run(async () =>
            //{
            //    //using (var uow = _unitOfWorkFactory.Create())
            //    //{

            //    //}
            //    return await unitOfWork.VehicleSegments.GetAll().Select(v => new VehicleSegmentPriceListItemModel
            //    {
            //        DailyRate = v.DailyRate,
            //        Description = v.Description,
            //        FreeEntranceMinutes = v.FreeEntranceMinutes,
            //        Image = v.Image,
            //        NameFa = v.NameFa,
            //        ParkingEntranceFixedFee = v.ParkingEntranceFixedFee,
            //        ParkingLotId = v.ParkingLotId,
            //        Id = v.Id
            //    }).ToListAsync();

            //});
            //var vehicleSegmentPricesTask = Task.Run(async () =>
            //{
            //    using (var uow = _unitOfWorkFactory.Create())
            //    {
            //        return await uow.ParkingVehicleSegmentPrices.GetAll().ToListAsync();
            //    }
            //});
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


            //var vehicleSegmentVariablePricesTask = Task.Run(async () =>
            //{
            //    using (var uow = _unitOfWorkFactory.Create())
            //    {
            //        return await uow.ParkingVehicleSegmentVariablePrices.GetAll().ToListAsync();
            //    }
            //});

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
    public int GetFreeSpacesCount()
    {
        try
        {

            var count = unitOfWork.ParkingSpaces.Find(p => p.IsOccupied == false).Count();
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }
    public int GetSpacesCount()
    {
        try
        {

            var count = unitOfWork.ParkingSpaces.GetAll().Count();
            return count;
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
    public async Task<(ImageSource? StartImage, ImageSource? ExitImage)> GetTicketImages(Guid ticketId)
    {
        try
        {
            var ticketImage = await unitOfWork.ParkingTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            (ImageSource? StartImage, ImageSource? ExitImage) result =  (null, null);
            if (ticketImage != null)
            {
                if (ticketImage.StartImage !=null)
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
            return (null,null);
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
            //using (var uow = _unitOfWorkFactory.Create())
            //{

            //}
            var ticket = unitOfWork.ParkingTickets.Find(s => s.Id == ticketId)
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
    Description = s.Description,
    CardUid = s.CardUid
}).FirstOrDefault();
            if (ticket != null && (ticket?.IsExited ?? false) == false)
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
                    ticket.DiscountPercent = (byte)discount;
                    ticket.TotalAmount = result.PayableAmount;
                    ticket.Description = description;

                    unitOfWork.ParkingTickets.ExecuteUpdate(p => p.Id == ticketId, update => update
                    .SetProperty(p => p.DurationMinutes, (int)varTime.TotalMinutes)
                    .SetProperty(ticket => ticket.DiscountPercent, ticket => (byte)discount)
                    .SetProperty(ticket => ticket.TotalAmount, ticket => result.PayableAmount)
                    .SetProperty(ticket => ticket.Description, ticket => description)
                    .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Unsynced));
                    return ticket;
                    //unitOfWork.Commit();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return ticket;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
    public async Task<TicketsListViewModel?> GetTicketDetailsAsync(Guid ticketId)
    {
        try
        {

            var ticket = await unitOfWork.ParkingTickets.Find(s => s.Id == ticketId)
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
                    Description = s.Description,
                    CardUid = s.CardUid,
                    EntranceGate = s.EntranceGate,
                    ExitGate = s.ExitGate,
                }).FirstOrDefaultAsync();
            if (ticket != null && (ticket?.IsExited ?? false) == false)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var segmentTask = unitOfWork.VehicleSegments.FirstOrDefaultAsync(p => p.Id == ticket.VehicleSegmentId);
                var discountTask = GetLicensePlateDiscountPercentAsync(ticket.EnLicensePlate ?? "_");
                var segmentPriceTask = unitOfWork.ParkingVehicleSegmentPrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToListAsync();
                var cardTask = unitOfWork.Cards.FirstOrDefaultAsync(c => c.CardSerialNo == ticket.CardUid);
                var segmentVariablePriceTask = unitOfWork.ParkingVehicleSegmentVariablePrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToListAsync();
                await Task.WhenAll(segmentVariablePriceTask, segmentPriceTask, cardTask, segmentTask, discountTask);
                var segment = segmentTask.Result;
                var segmentPrices = segmentPriceTask.Result;
                var segmentVariablePrice = segmentVariablePriceTask.Result;
                var card = cardTask.Result;
                stopwatch.Stop();
                _logger.LogError($"GetTicketDetailsAsync  Run Time: {stopwatch.ElapsedMilliseconds} ms");
                //var segmentTask = Task.Run(async () =>
                //{
                //    using (var uow = _unitOfWorkFactory.Create())
                //    {
                //        return await uow.VehicleSegments.Find(p => p.Id == ticket.VehicleSegmentId).FirstOrDefaultAsync();
                //    }
                //});

                //var segmentPriceTask = Task.Run(async () =>
                //{
                //    using (var uow = _unitOfWorkFactory.Create())
                //    {
                //        return await uow.ParkingVehicleSegmentPrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToListAsync();
                //    }
                //});

                //var discountTask = Task.Run(() => GetLicensePlateDiscountPercent(ticket.EnLicensePlate ?? "_"));

                //var cardTask = Task.Run(async () =>
                //{
                //    using (var uow = _unitOfWorkFactory.Create())
                //    {
                //        return await uow.Cards.Find(c => c.CardSerialNo == ticket.CardUid).FirstOrDefaultAsync();
                //    }
                //});

                //var segmentVariablePriceTask = Task.Run(async () =>
                //{
                //    using (var uow = _unitOfWorkFactory.Create())
                //    {
                //        return await uow.ParkingVehicleSegmentVariablePrices.Find(p => p.VehicleSegmentId == ticket.VehicleSegmentId).ToListAsync();
                //    }
                //});
                //await Task.WhenAll(segmentTask, segmentPriceTask, discountTask, cardTask);



                //var segmentTask = Task.Run(async() =>
                //{
                //    using (var uow = _unitOfWorkFactory.Create())
                //    {
                //        return await uow.VehicleSegments.Find(p => p.Id == ticket.VehicleSegmentId).FirstOrDefaultAsync();
                //    }
                //});





                if (segmentPrices != null)
                {
                    List<(int, int, int)> hourlyrate = new List<(int, int, int)>(); // Initialize the list
                    foreach (var item in segmentPrices)
                    {
                        hourlyrate.Add((item.TimeFrom.Hour, item.TimeTo.Hour, (int)item.HourlyRate));
                    }
                    var discount = await discountTask;
                    //var discount = GetLicensePlateDiscountPercent(ticket.EnLicensePlate ?? "_");




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
                    ticket.DiscountPercent = (byte)discount;
                    ticket.TotalAmount = result.PayableAmount;
                    ticket.Description = description;
                    ticket.DurationMinutes = (int)varTime.TotalMinutes;

                    unitOfWork.ParkingTickets.ExecuteUpdate(p => p.Id == ticketId, update => update
                    .SetProperty(p => p.DurationMinutes, (int)varTime.TotalMinutes)
                    .SetProperty(ticket => ticket.DiscountPercent, ticket => (byte)discount)
                    .SetProperty(ticket => ticket.TotalAmount, ticket => result.PayableAmount)
                    .SetProperty(ticket => ticket.Description, ticket => description)
                    .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Unsynced));
                    return ticket;
                    //unitOfWork.Commit();
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return ticket;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
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
            return unitOfWork.ParkingTickets.Find(s => s.EnLicensePlate == licenseEnPlate && s.IsExited == false).Select(s => (Guid?)s.Id).FirstOrDefault();
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
                DriverDescription = request.DriverDescription,
                DriverFullName = request.DriverFullName,
                DriverPhoneNumber = request.DriverPhoneNumber,
                DeviceId = Settings.Default.Application_DeviceId,
                IP = LogHelper.GetLocalIPAddress()
            };
            unitOfWork.ParkingTickets.Add(ticket);

            unitOfWork.ParkingSpaces.ExecuteUpdate(s => s.Id == ticket.ParkingSpaceID, update => update.SetProperty(s => s.IsOccupied, true));
            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == request.CardUid, update => update.SetProperty(s => s.IsInUse, true));
            return new TServiceResponse<Guid>() { Succeeded = true, Result = ticket.Id, Message = "بلیط بارکینگ با موفقیت ثبت شد" };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new TServiceResponse<Guid>() { Succeeded = false, Message = "خطا در ثبت اطلاعات" };

        }
    }

    public List<TicketsListViewModel> GetTicketList(GetTicketListRequestModel request)
    {
        try
        {

            var sergments = unitOfWork.VehicleSegments.GetAll().ToList();
            var tickets = unitOfWork.ParkingTickets.GetAll()
                                             .Select(s => new TicketsListViewModel
                                             {
                                                 BarcodeId = s.BarcodeId,
                                                 Id = s.Id,
                                                 LicensePlate = s.LicensePlate,
                                                 ParkingSpaceID = s.ParkingSpaceID,
                                                 StartTime = s.StartTime,
                                                 EndTime = (s.EndTime != s.StartTime) ? s.EndTime : null,
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
                                                 ParkingLotId = s.ParkingLotId,
                                                 TraceNo = s.TraceNo,
                                                 RRN = s.RRN,
                                             });

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
            if (request.IsExited != null)
                tickets = tickets.Where(t => t.IsExited == request.IsExited);
            if (request.IsPaid != null)
                tickets = tickets.Where(t => t.IsPaid == request.IsPaid);
            if (request.StartStartTime != null)
                tickets = tickets.Where(t => t.StartTime >= request.StartStartTime);
            if (request.StartStartTime != null)
                tickets = tickets.Where(t => t.StartTime <= request.EndStartTime);
            if (request.BarcodeId != null)
                tickets = tickets.Where(t => t.BarcodeId == request.BarcodeId);
            if (request.LicensePlate != null && request.LicensePlate.Length > 1)
                tickets = tickets.Where(t => t.EnLicensePlate.Contains(request.LicensePlate));

            var t = tickets.Select(s => new TicketsListViewModel
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
            }).ToList();

            return t;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketsListViewModel>();
        }
    }
    public async Task<List<TicketsListViewModel>> GetTicketListAsync(GetTicketListRequestModel request)
    {
        try
        {

            //var sergments = unitOfWork.VehicleSegments.GetAll().ToList();
            //using (var uow = _unitOfWorkFactory.Create())
            //{

            //}
            var tickets = unitOfWork.ParkingTickets.GetAll()
                                                 .Select(s => new TicketsListViewModel
                                                 {
                                                     BarcodeId = s.BarcodeId,
                                                     Id = s.Id,
                                                     LicensePlate = s.LicensePlate,
                                                     ParkingSpaceID = s.ParkingSpaceID,
                                                     StartTime = s.StartTime,
                                                     EndTime = (s.EndTime != s.StartTime) ? s.EndTime : null,
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
                                                     ParkingLotId = s.ParkingLotId,
                                                     TraceNo = s.TraceNo,
                                                     RRN = s.RRN,
                                                 });

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
            if (request.IsExited != null)
                tickets = tickets.Where(t => t.IsExited == request.IsExited);
            if (request.IsPaid != null)
                tickets = tickets.Where(t => t.IsPaid == request.IsPaid);
            if (request.StartStartTime != null)
                tickets = tickets.Where(t => t.StartTime >= request.StartStartTime);
            if (request.StartStartTime != null)
                tickets = tickets.Where(t => t.StartTime <= request.EndStartTime);
            if (request.BarcodeId != null)
                tickets = tickets.Where(t => t.BarcodeId == request.BarcodeId);
            if (request.LicensePlate != null && request.LicensePlate.Length > 1)
                tickets = tickets.Where(t => t.EnLicensePlate.Contains(request.LicensePlate));

            var t = await tickets.Select(s => new TicketsListViewModel
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
            }).ToListAsync();

            return t;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<TicketsListViewModel>();
        }
    }
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
            var licensePlate = unitOfWork.LicensePlates.Find(l => l.EnLicensePlate == licenseEnPlate).FirstOrDefault();
            if (licensePlate == null)
            {
                return 0;
            }
            var licensePlateGroup = unitOfWork.LicensePlateGroups.Find(l => l.Id == licensePlate.GroupId && l.StartDate < DateTime.Now && l.EndDate > DateTime.Now).FirstOrDefault();
            if (licensePlateGroup == null)
            {
                return 0;
            }

            return licensePlateGroup.DiscountPercent;
            //using (var uow = _unitOfWorkFactory.Create())
            //{
            //    var licensePlate = uow.LicensePlates.Find(l => l.EnLicensePlate == licenseEnPlate).FirstOrDefault();
            //    if (licensePlate == null)
            //    {
            //        return 0;
            //    }
            //    var licensePlateGroup = uow.LicensePlateGroups.Find(l => l.Id == licensePlate.GroupId && l.StartDate < DateTime.Now && l.EndDate > DateTime.Now).FirstOrDefault();
            //    if (licensePlateGroup == null)
            //    {
            //        return 0;
            //    }

            //    return licensePlateGroup.DiscountPercent;
            //}

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }
    public async Task<short> GetLicensePlateDiscountPercentAsync(string licenseEnPlate)
    {
        try
        {
            var licensePlate = await unitOfWork.LicensePlates.Find(l => l.EnLicensePlate == licenseEnPlate).FirstOrDefaultAsync();
            if (licensePlate == null)
            {
                return 0;
            }
            var licensePlateGroup = await unitOfWork.LicensePlateGroups.Find(l => l.Id == licensePlate.GroupId /*&& l.StartDate <= DateTime.Now && l.EndDate >= DateTime.Now*/).FirstOrDefaultAsync();
            if (licensePlateGroup == null)
            {
                return 0;
            }

            return licensePlateGroup.DiscountPercent;
            //using (var uow = _unitOfWorkFactory.Create())
            //{
            //    var licensePlate = uow.LicensePlates.Find(l => l.EnLicensePlate == licenseEnPlate).FirstOrDefault();
            //    if (licensePlate == null)
            //    {
            //        return 0;
            //    }
            //    var licensePlateGroup = uow.LicensePlateGroups.Find(l => l.Id == licensePlate.GroupId && l.StartDate < DateTime.Now && l.EndDate > DateTime.Now).FirstOrDefault();
            //    if (licensePlateGroup == null)
            //    {
            //        return 0;
            //    }

            //    return licensePlateGroup.DiscountPercent;
            //}

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
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

    public List<LicensePlateListItemViewModel> GetLicensePlateGroupList()
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
                    StartDate = l.StartDate
                }).ToList();
            List<LicensePlateListItemViewModel> list = new List<LicensePlateListItemViewModel>();
            foreach (var item in licensePlateGroupList)
            {
                var licensePlates = unitOfWork.LicensePlates.Find(p => p.GroupId == item.Id).ToList();

                foreach (var sub in licensePlates)
                {
                    list.Add(new LicensePlateListItemViewModel
                    {
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

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return new List<LicensePlateListItemViewModel>();
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
                return licensePlate.GroupId;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }

    public bool SetTicketPaidInfo(TicketPaidInfoModel request)
    {
        try
        {

            unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == request.TicketId, update => update
                                                .SetProperty(product => product.IsExited, product => true)
                                                .SetProperty(product => product.EndTime, product => DateTime.Now)
                                                .SetProperty(product => product.IsPaid, product => true)
                                                .SetProperty(product => product.PaidAmount, product => request.PaidAmount)
                                                .SetProperty(product => product.PaidCreditCard, product => request.PaidCreditCard.Replace(@"\0", ""))
                                                .SetProperty(product => product.RefId, product => request.RefId)
                                                .SetProperty(product => product.PaidType, product => request.PaidType)
                                                .SetProperty(product => product.PaidDate, product => request.PaidDate)
                                                .SetProperty(product => product.RRN, product => request.RRN)
                                                .SetProperty(product => product.TraceNo, product => request.TraceNo)
                                                .SetProperty(product => product.DeviceId, product => Settings.Default.Application_DeviceId)
                                                .SetProperty(product => product.MerchantNumber, product => request.MerchantNumber)
                                                .SetProperty(product => product.ExitGate, product => request.ExitGate)
                                                .SetProperty(product => product.ExitImage, product => request.ExitImage)
                                                .SetProperty(product => product.IsCardMissing, product => request.IsMissingCard)
                                                .SetProperty(product => product.ExitRegistrarUserId, product => request.ExitRegistrarUserId)
                                                .SetProperty(product => product.TicketStatus, product => TicketStatus.Unsynced));
            //unitOfWork.Commit();
            unitOfWork.Cards.ExecuteUpdate(s => s.CardSerialNo == request.CardUid, update => update.SetProperty(s => s.IsInUse, false));


            //unitOfWork.Commit();

            var ticket = unitOfWork.ParkingTickets.Find(s => s.Id == request.TicketId).FirstOrDefault();
            if (ticket != null)
            {
                unitOfWork.ParkingSpaces.ExecuteUpdate(s => s.Id == ticket.ParkingSpaceID, update => update.SetProperty(product => product.IsOccupied, product => false));

                //unitOfWork.Commit();
            }
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return false;
        }
    }

    public LicensePlateGroup? GetLicensePlateGroupById(Guid id)
    {

        return unitOfWork.LicensePlateGroups.GetById(id);
    }

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
            //unitOfWork.Commit();
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
                    OwnerPic = request.OwnerPic,
                    PercentDiscount = request.PercentDiscount,
                    LicensePlateGroupId = request.LicensePlateGroupId,
                    VehicleSegmentId = request.VehicleSegmentId,
                    EnLicensePlate = request.EnLicensePlate,
                };
                unitOfWork.Cards.Add(card);
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
                unitOfWork.Cards.Update(card);
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
                ParkingLotId = v.ParkingLotId
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
            var ticket = unitOfWork.ParkingTickets.Find(c => c.CardUid == cardSerialNo && c.IsExited == false).Select(c => new { Id = c.Id, IsPaid = c.IsPaid }).FirstOrDefault();
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
            return unitOfWork.Cards.Find(c=>c.PercentDiscount>0).Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return 0;
        }
    }
}
