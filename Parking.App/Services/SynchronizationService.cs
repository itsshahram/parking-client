using Parking.App.Models;
using Parking.App.Models.Dto.Parking.ParkingLot;
using Parking.App.Models.Dto.Parking.ParkingTicket;
using Parking.App.Models.Dto.Parking.ParkingTicketExtraImage;
using Parking.App.Models.Dto.User;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.GeneralServiceResponse;
using Parking.App.Models.Login;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;
using Parking.Domain.General;



namespace Parking.App.Services;

public class SynchronizationService(IUnitOfWork _unitOfWork,
    ILogger<SynchronizationService> logger,
    IHttpClientFactory _httpClientFactory,
    UserManager<ApplicationUser> _userManager,
    RoleManager<ApplicationRole> _roleManager) : ISynchronizationService
{
    private readonly ILogger<SynchronizationService> _logger = logger;
    private readonly UserManager<ApplicationUser> userManager = _userManager;
    private readonly RoleManager<ApplicationRole> roleManager = _roleManager;
    private IUnitOfWork unitOfWork = _unitOfWork;
    private IHttpClientFactory httpClientFactory = _httpClientFactory;

    public TServiceResponse<bool> CheckToken(string userName, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = userName,
                Password = password
            };
            TokenStore.BaseUrl = Settings.Default.Application_ApiServerAddress;
            var client = httpClientFactory.CreateClient();
            var response = client.PostAsJsonAsync($"{TokenStore.BaseUrl}/Account/login", loginRequest).Result;
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>().Result;

                if (loginResponse != null && loginResponse.StatusCode == 201)
                {
                    TokenStore.BearerToken = loginResponse.Data.Token;
                    TokenStore.UserId = loginResponse.Data.UserId;
                    if (loginResponse.Data.ParkingLotId != null)
                    {
                        TokenStore.ParkingLotId = (int)loginResponse.Data.ParkingLotId;
                        return new TServiceResponse<bool>
                        {
                            Succeeded = true,
                            Message = "شما با موفقست احراز شدید"
                        };
                    }
                    else
                    {
                        return new TServiceResponse<bool>
                        {
                            Succeeded = false,
                            Message = "شما دسترسی به هیچ پارکینگی ندارید"
                        };
                    }
                }
                else
                {
                    return new TServiceResponse<bool>
                    {
                        Succeeded = false,
                        Message = "نام کاربری یا رمز عبور اشتباه است"
                    };
                }

            }
            else
            {
                return new TServiceResponse<bool>
                {
                    Succeeded = false,
                    Message = "خطا در احراز هویت"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>
            {
                Succeeded = false,
                Message = ex.Message
            };
        }
    }

    public async Task<TServiceResponse<bool>> CheckTokenAsync(string userName, string password)
    {
        try
        {
            TokenStore.BaseUrl = Settings.Default.Application_ApiServerAddress;
            var loginRequest = new LoginRequest
            {
                Username = userName,
                Password = password
            };

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{TokenStore.BaseUrl}/Account/login", loginRequest);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

                if (loginResponse != null && loginResponse.StatusCode == 201)
                {
                    TokenStore.BearerToken = loginResponse?.Data?.Token ?? "";
                    TokenStore.UserId = (Guid)loginResponse.Data.UserId;
                    if (loginResponse.Data.ParkingLotId != null)
                    {
                        TokenStore.ParkingLotId = (int)loginResponse.Data.ParkingLotId;
                        return new TServiceResponse<bool>
                        {
                            Succeeded = true,
                            Message = "شما با موفقست احراز شدید"
                        };
                    }
                    else
                    {
                        return new TServiceResponse<bool>
                        {
                            Succeeded = false,
                            Message = "شما دسترسی به هیچ پارکینگی ندارید"
                        };
                    }
                }
                else
                {
                    return new TServiceResponse<bool>
                    {
                        Succeeded = false,
                        Message = "نام کاربری یا رمز عبور اشتباه است"
                    };
                }

            }
            else
            {
                return new TServiceResponse<bool>
                {
                    Succeeded = false,
                    Message = "خطا در احراز هویت"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>
            {
                Succeeded = false,
                Message = ex.Message
            };
        }
    }

    public TServiceResponse<bool> CreateVehicleSegmentsPrice(List<ParkingVehicleSegmentPriceModel> request)
    {
        try
        {
            foreach (var item in request)
            {
                ParkingVehicleSegmentPrice price = unitOfWork.ParkingVehicleSegmentPrices.GetById(item.Id);
                if (price != null)
                {
                    unitOfWork.ParkingVehicleSegmentPrices.ExecuteUpdate(p => p.Id == item.Id,
                        update => update
                        .SetProperty(s => s.ParkingLotId, item.ParkingLotId)
                            .SetProperty(s => s.VehicleSegmentId, item.VehicleSegmentId)
                            .SetProperty(s => s.HourlyRate, item.HourlyRate)
                            .SetProperty(s => s.TimeFrom, item.TimeFrom)
                            .SetProperty(s => s.TimeTo, item.TimeTo)
                            .SetProperty(s => s.IsVariableEnable, item.IsVariableEnable)
                        );
                    //unitOfWork.Commit();

                }
                else
                {
                    price = new ParkingVehicleSegmentPrice()
                    {
                        ParkingLotId = item.ParkingLotId,
                        VehicleSegmentId = item.VehicleSegmentId,
                        HourlyRate = item.HourlyRate,
                        TimeFrom = item.TimeFrom,
                        TimeTo = item.TimeTo,
                        Id = item.Id,
                        IsVariableEnable = item.IsVariableEnable
                    };
                    unitOfWork.ParkingVehicleSegmentPrices.Add(price);
                    //unitOfWork.Commit();
                }
            }
            return new TServiceResponse<bool>(true, "عملیات با موفقیت انجام شد", true);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "عملیات با خطا مواجه شد", false);
        }
    }

    public List<LicensePlateGroup> GetLicensePlateGroups()
    {
        try
        {
            return unitOfWork.LicensePlateGroups.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new List<LicensePlateGroup>();
        }
    }

    public async Task<List<LicensePlateGroup>> GetLicensePlateGroupsAsync()
    {
        try
        {
            return await unitOfWork.LicensePlateGroups.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new List<LicensePlateGroup>();
        }
    }
    public static string GenerateSecurityStamp()
    {
        return Guid.NewGuid().ToString();
    }

    public TServiceResponse<bool> GetParkingLotAccountsFromServer()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var responce = client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-parking-users").Result;
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ParkingUserViewModel>>>(responce);
            if (result?.StatusCode == 200)
            {
                var users = result.Data;
                var roles = users.Select(a => a.Role).Distinct().ToList();
                foreach (var role in roles)
                {
                    var localRole = unitOfWork.Roles.FirstOrDefault(a => a.Name == role);
                    if (localRole == null)
                    {
                        ApplicationRole newRole = new ApplicationRole
                        {
                            Name = role,
                            FaName = role.RoleToPersian(),
                            NormalizedName = role.ToUpper()
                        };
                        roleManager.CreateAsync(newRole).Wait();
                        //unitOfWork.Commit();
                    }
                }
                foreach (var user in users)
                {
                    var localUser = unitOfWork.Users.GetById(user.Id);
                    if (localUser != null)
                    {
                        localUser.Id = user.Id;
                        localUser.Email = user.Email;
                        localUser.NormalizedEmail = user.Email.ToUpper();
                        localUser.IsActive = (bool)user.IsActive;
                        localUser.Firstname = user.FirstName;
                        localUser.Lastname = user.LastName;
                        localUser.PasswordHash = user.PasswordHash;
                        localUser.ParkingLotId = user.ParkingLoId;
                        localUser.UserName = user.UserName;
                        localUser.NormalizedUserName = user.UserName.ToUpper();
                        localUser.RegisterDate = DateTime.Now;
                        unitOfWork.Users.Update(localUser);
                        //unitOfWork.Commit();
                    }
                    else
                    {
                        ApplicationUser userInfo = new ApplicationUser
                        {
                            Id = user.Id,
                            Email = user.Email,
                            IsActive = (bool)user.IsActive,
                            Firstname = user.FirstName,
                            Lastname = user.LastName,
                            PasswordHash = user.PasswordHash,
                            ParkingLotId = user.ParkingLoId,
                            UserName = user.UserName,
                            RegisterDate = DateTime.Now,
                            NormalizedEmail = user.Email.ToUpper(),
                            NormalizedUserName = user.UserName.ToUpper(),
                            SecurityStamp = GenerateSecurityStamp()
                        };
                        unitOfWork.Users.Add(userInfo);
                        //unitOfWork.Commit();
                    }

                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "نام کاربری یا رمز عبور اشتباه است", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public async Task<TServiceResponse<bool>> GetParkingLotAccountsFromServerAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var response = await client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-parking-users");

            var result = JsonConvert.DeserializeObject<ApiResponse<List<ParkingUserViewModel>>>(response);
            if (result?.StatusCode == 200)
            {
                var users = result.Data;
                var roles = users.Select(a => a.Role).Distinct().ToList();
                foreach (var role in roles)
                {
                    var localRole = await unitOfWork.Roles.FirstOrDefaultAsync(a => a.Name == role);
                    if (localRole == null)
                    {
                        ApplicationRole newRole = new ApplicationRole
                        {
                            Name = role,
                            FaName = role.RoleToPersian(),
                            NormalizedName = role.ToUpper()
                        };
                        await roleManager.CreateAsync(newRole);
                    }
                }

                foreach (var user in users)
                {
                    var localUser = await unitOfWork.Users.GetByIdAsync(user.Id);
                    if (localUser != null)
                    {
                        localUser.Id = user.Id;
                        localUser.Email = user.Email;
                        localUser.NormalizedEmail = user.Email.ToUpper();
                        localUser.IsActive = (bool)user.IsActive;
                        localUser.Firstname = user.FirstName;
                        localUser.Lastname = user.LastName;
                        localUser.PasswordHash = user.PasswordHash;
                        localUser.ParkingLotId = user.ParkingLoId;
                        localUser.PhoneNumber = user.PhoneNumber;
                        localUser.UserName = user.UserName;
                        localUser.NormalizedUserName = user.UserName.ToUpper();
                        localUser.RegisterDate = DateTime.Now;
                        await userManager.UpdateAsync(localUser);
                        var newUser = await userManager.FindByIdAsync(localUser.Id.ToString());
                        var x = await userManager.AddToRoleAsync(newUser, user.Role);
                    }
                    else
                    {
                        ApplicationUser userInfo = new ApplicationUser
                        {
                            Id = user.Id,
                            Email = user.Email,
                            IsActive = (bool)user.IsActive,
                            Firstname = user.FirstName,
                            Lastname = user.LastName,
                            PasswordHash = user.PasswordHash,
                            ParkingLotId = user.ParkingLoId,
                            UserName = user.UserName,
                            RegisterDate = DateTime.Now,
                            NormalizedEmail = user.Email.ToUpper(),
                            SecurityStamp = GenerateSecurityStamp(),
                            PhoneNumber = user.PhoneNumber
                        };
                        await userManager.CreateAsync(userInfo);
                        var newUser = await userManager.FindByIdAsync(userInfo.Id.ToString());
                        var x = await userManager.AddToRoleAsync(newUser, user.Role);
                    }
                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "نام کاربری یا رمز عبور اشتباه است", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);

        }
    }

    public TServiceResponse<bool> GetParkingLotDetailsFromServer()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var responce = client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/parking-details-by-ownerId").Result;
            var result = JsonConvert.DeserializeObject<ApiResponse<ParkingLotModel>>(responce);
            if (result?.StatusCode == 200)
            {
                var parking = result.Data;
                var localParking = unitOfWork.ParkingLots.FirstOrDefault();
                if (localParking != null)
                {
                    if (localParking.Id == parking.Id)
                    {
                        localParking.Image = parking.Image;
                        localParking.IsActive = parking.IsActive;
                        localParking.IsOnline = parking.IsOnline;
                        localParking.LastSyncDateTime = DateTime.Now;
                        localParking.FloorsCount = parking.FloorsCount;
                        localParking.EndWorkingHours = parking.EndWorkingHours;
                        localParking.CreatorUserId = parking.CreatorUserId;
                        localParking.Description = parking.Description;
                        localParking.City = parking.City;
                        localParking.Address = parking.Address;
                        localParking.Capacity = parking.Capacity;
                        localParking.CreateDate = parking.CreateDate;
                        localParking.latitude = parking.latitude;
                        localParking.longitude = parking.longitude;
                        localParking.Name = parking.Name;
                        localParking.OwnerUserId = parking.OwnerUserId;
                        localParking.Province = parking.Province;
                        localParking.StartWorkingHours = parking.StartWorkingHours;
                        localParking.Id = parking.Id;
                        unitOfWork.ParkingLots.Update(localParking);
                        //unitOfWork.Commit();
                        foreach (var item in parking.Sections)
                        {
                            var localSection = unitOfWork.ParkingSections.GetById(item.Id);
                            if (localSection == null)
                            {
                                ParkingSection ps = new ParkingSection
                                {
                                    Id = item.Id,
                                    Capacity = item.Capacity,
                                    Description = item.Description,
                                    CreateDate = item.CreateDate,
                                    Floor = item.Floor,
                                    IndexName = item.IndexName,
                                    Name = item.Name,
                                    ParkingId = item.ParkingId,
                                    SectionNumber = item.SectionNumber
                                };
                                unitOfWork.ParkingSections.Add(ps);
                                //unitOfWork.Commit();
                            }
                            else
                            {
                                localSection.Capacity = item.Capacity;
                                localSection.Description = item.Description;
                                localSection.CreateDate = item.CreateDate;
                                localSection.Floor = item.Floor;
                                localSection.IndexName = item.IndexName;
                                localSection.Name = item.Name;
                                localSection.ParkingId = item.ParkingId;
                                localSection.SectionNumber = item.SectionNumber;
                                unitOfWork.ParkingSections.Update(localSection);
                                //unitOfWork.Commit();
                            }
                        }

                        foreach (var item in parking.ParkingSpaces)
                        {
                            var parkingSpace = unitOfWork.ParkingSpaces.GetById(item.Id);
                            if (parkingSpace == null)
                            {
                                ParkingSpace ps = new ParkingSpace
                                {
                                    Id = item.Id,
                                    ParkingId = item.ParkingId,
                                    Name = item.Name,
                                    SpaceNumber = item.SpaceNumber,
                                    IsOccupied = item.IsOccupied,
                                    IsActive = item.IsActive,
                                    ParkingSectionId = item.ParkingSectionId,
                                    CreateDate = DateTime.Now
                                };
                                unitOfWork.ParkingSpaces.Add(ps);
                                //unitOfWork.Commit();
                            }
                            else
                            {
                                parkingSpace.IsActive = item.IsActive;
                                unitOfWork.ParkingSpaces.Update(parkingSpace);
                                //unitOfWork.Commit();
                            }
                        }
                        return new TServiceResponse<bool>(true, "عملیات موفق", true);
                    }
                    else
                    {
                        return new TServiceResponse<bool>(false, "پارکینگ دریافت شده با اطلاعات لوکال مطابقت ندارد", false);
                    }
                }
                else
                {
                    ParkingLot parkingLot = new ParkingLot
                    {
                        Image = parking.Image,
                        IsActive = parking.IsActive,
                        IsOnline = parking.IsOnline,
                        LastSyncDateTime = DateTime.Now,
                        FloorsCount = parking.FloorsCount,
                        EndWorkingHours = parking.EndWorkingHours,
                        CreatorUserId = parking.CreatorUserId,
                        Description = parking.Description,
                        City = parking.City,
                        Address = parking.Address,
                        Capacity = parking.Capacity,
                        CreateDate = parking.CreateDate,
                        latitude = parking.latitude,
                        longitude = parking.longitude,
                        Name = parking.Name,
                        OwnerUserId = parking.OwnerUserId,
                        Province = parking.Province,
                        StartWorkingHours = parking.StartWorkingHours,
                        Id = parking.Id
                    };
                    unitOfWork.ParkingLots.Add(parkingLot);
                    //unitOfWork.Commit();
                    foreach (var item in parking.Sections)
                    {
                        var localSection = unitOfWork.ParkingSections.GetById(item.Id);
                        if (localSection == null)
                        {
                            ParkingSection ps = new ParkingSection
                            {
                                Id = item.Id,
                                Capacity = item.Capacity,
                                Description = item.Description,
                                CreateDate = item.CreateDate,
                                Floor = item.Floor,
                                IndexName = item.IndexName,
                                Name = item.Name,
                                ParkingId = item.ParkingId,
                                SectionNumber = item.SectionNumber
                            };
                            unitOfWork.ParkingSections.Add(ps);
                            //unitOfWork.Commit();
                        }
                        else
                        {
                            localSection.Capacity = item.Capacity;
                            localSection.Description = item.Description;
                            localSection.CreateDate = item.CreateDate;
                            localSection.Floor = item.Floor;
                            localSection.IndexName = item.IndexName;
                            localSection.Name = item.Name;
                            localSection.ParkingId = item.ParkingId;
                            localSection.SectionNumber = item.SectionNumber;
                            unitOfWork.ParkingSections.Update(localSection);
                            //unitOfWork.Commit();
                        }
                    }

                    foreach (var item in parking.ParkingSpaces)
                    {
                        var parkingSpace = unitOfWork.ParkingSpaces.GetById(item.Id);
                        if (parkingSpace == null)
                        {
                            ParkingSpace ps = new ParkingSpace
                            {
                                Id = item.Id,
                                ParkingId = item.ParkingId,
                                Name = item.Name,
                                SpaceNumber = item.SpaceNumber,
                                IsOccupied = item.IsOccupied,
                                IsActive = item.IsActive,
                                ParkingSectionId = item.ParkingSectionId,
                                CreateDate = DateTime.Now
                            };
                            unitOfWork.ParkingSpaces.Add(ps);
                            //unitOfWork.Commit();
                        }
                        else
                        {
                            parkingSpace.IsActive = item.IsActive;
                            unitOfWork.ParkingSpaces.Update(parkingSpace);
                            //unitOfWork.Commit();
                        }
                    }

                    return new TServiceResponse<bool>(true, "عملیات موفق", true);
                }
            }
            return new TServiceResponse<bool>(false, "خطای سیستمی", false);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطای سیستمی", false);
        }
    }

    public async Task<TServiceResponse<bool>> GetParkingLotDetailsFromServerAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var responce = await client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/parking-details-by-ownerId");
            var result = JsonConvert.DeserializeObject<ApiResponse<ParkingLotModel>>(responce);
            if (result?.StatusCode == 200)
            {
                var parking = result.Data;
                var localParking = unitOfWork.ParkingLots.FirstOrDefault();
                if (localParking != null)
                {
                    if (localParking.Id == parking.Id)
                    {
                        localParking.Image = parking.Image;
                        localParking.IsActive = parking.IsActive;
                        localParking.IsOnline = parking.IsOnline;
                        localParking.LastSyncDateTime = DateTime.Now;
                        localParking.FloorsCount = parking.FloorsCount;
                        localParking.EndWorkingHours = parking.EndWorkingHours;
                        localParking.CreatorUserId = parking.CreatorUserId;
                        localParking.Description = parking.Description;
                        localParking.City = parking.City;
                        localParking.Address = parking.Address;
                        localParking.Capacity = parking.Capacity;
                        localParking.CreateDate = parking.CreateDate;
                        localParking.latitude = parking.latitude;
                        localParking.longitude = parking.longitude;
                        localParking.Name = parking.Name;
                        localParking.OwnerUserId = parking.OwnerUserId;
                        localParking.Province = parking.Province;
                        localParking.StartWorkingHours = parking.StartWorkingHours;
                        localParking.Id = parking.Id;
                        unitOfWork.ParkingLots.Update(localParking);
                        //await unitOfWork.CommitAsync(default);
                        foreach (var item in parking.Sections)
                        {
                            var localSection = unitOfWork.ParkingSections.GetById(item.Id);
                            if (localSection == null)
                            {
                                ParkingSection ps = new ParkingSection
                                {
                                    Id = item.Id,
                                    Capacity = item.Capacity,
                                    Description = item.Description,
                                    CreateDate = item.CreateDate,
                                    Floor = item.Floor,
                                    IndexName = item.IndexName,
                                    Name = item.Name,
                                    ParkingId = item.ParkingId,
                                    SectionNumber = item.SectionNumber
                                };
                                unitOfWork.ParkingSections.Add(ps);
                                //await unitOfWork.CommitAsync(default);
                            }
                            else
                            {
                                localSection.Capacity = item.Capacity;
                                localSection.Description = item.Description;
                                localSection.CreateDate = item.CreateDate;
                                localSection.Floor = item.Floor;
                                localSection.IndexName = item.IndexName;
                                localSection.Name = item.Name;
                                localSection.ParkingId = item.ParkingId;
                                localSection.SectionNumber = item.SectionNumber;
                                unitOfWork.ParkingSections.Update(localSection);
                                //await unitOfWork.CommitAsync(default);
                            }
                        }

                        foreach (var item in parking.ParkingSpaces)
                        {
                            var parkingSpace = unitOfWork.ParkingSpaces.GetById(item.Id);
                            if (parkingSpace == null)
                            {
                                ParkingSpace ps = new ParkingSpace
                                {
                                    Id = item.Id,
                                    ParkingId = item.ParkingId,
                                    Name = item.Name,
                                    SpaceNumber = item.SpaceNumber,
                                    IsOccupied = item.IsOccupied,
                                    IsActive = item.IsActive,
                                    ParkingSectionId = item.ParkingSectionId,
                                    CreateDate = DateTime.Now
                                };
                                unitOfWork.ParkingSpaces.Add(ps);
                                //await unitOfWork.CommitAsync(default);
                            }
                            else
                            {
                                parkingSpace.IsActive = item.IsActive;
                                unitOfWork.ParkingSpaces.Update(parkingSpace);
                                //await unitOfWork.CommitAsync(default);
                            }
                        }
                        return new TServiceResponse<bool>(true, "عملیات موفق", true);
                    }
                    else
                    {
                        return new TServiceResponse<bool>(false, "پارکینگ دریافت شده با اطلاعات لوکال مطابقت ندارد", false);
                    }

                }
                else
                {
                    ParkingLot parkingLot = new ParkingLot
                    {
                        Image = parking.Image,
                        IsActive = parking.IsActive,
                        IsOnline = parking.IsOnline,
                        LastSyncDateTime = DateTime.Now,
                        FloorsCount = parking.FloorsCount,
                        EndWorkingHours = parking.EndWorkingHours,
                        CreatorUserId = parking.CreatorUserId,
                        Description = parking.Description,
                        City = parking.City,
                        Address = parking.Address,
                        Capacity = parking.Capacity,
                        CreateDate = parking.CreateDate,
                        latitude = parking.latitude,
                        longitude = parking.longitude,
                        Name = parking.Name,
                        OwnerUserId = parking.OwnerUserId,
                        Province = parking.Province,
                        StartWorkingHours = parking.StartWorkingHours,
                        Id = parking.Id
                    };
                    unitOfWork.ParkingLots.Add(parkingLot);
                    //await unitOfWork.CommitAsync(default);
                    foreach (var item in parking.Sections)
                    {
                        var localSection = unitOfWork.ParkingSections.GetById(item.Id);
                        if (localSection == null)
                        {
                            ParkingSection ps = new ParkingSection
                            {
                                Id = item.Id,
                                Capacity = item.Capacity,
                                Description = item.Description,
                                CreateDate = item.CreateDate,
                                Floor = item.Floor,
                                IndexName = item.IndexName,
                                Name = item.Name,
                                ParkingId = item.ParkingId,
                                SectionNumber = item.SectionNumber
                            };
                            unitOfWork.ParkingSections.Add(ps);
                            //await unitOfWork.CommitAsync(default);
                        }
                        else
                        {
                            localSection.Capacity = item.Capacity;
                            localSection.Description = item.Description;
                            localSection.CreateDate = item.CreateDate;
                            localSection.Floor = item.Floor;
                            localSection.IndexName = item.IndexName;
                            localSection.Name = item.Name;
                            localSection.ParkingId = item.ParkingId;
                            localSection.SectionNumber = item.SectionNumber;
                            unitOfWork.ParkingSections.Update(localSection);
                            //await unitOfWork.CommitAsync(default);
                        }
                    }

                    foreach (var item in parking.ParkingSpaces)
                    {
                        var parkingSpace = unitOfWork.ParkingSpaces.GetById(item.Id);
                        if (parkingSpace == null)
                        {
                            ParkingSpace ps = new ParkingSpace
                            {
                                Id = item.Id,
                                ParkingId = item.ParkingId,
                                Name = item.Name,
                                SpaceNumber = item.SpaceNumber,
                                IsOccupied = item.IsOccupied,
                                IsActive = item.IsActive,
                                ParkingSectionId = item.ParkingSectionId,
                                CreateDate = DateTime.Now
                            };
                            unitOfWork.ParkingSpaces.Add(ps);
                            //await unitOfWork.CommitAsync(default);
                        }
                        else
                        {
                            parkingSpace.IsActive = item.IsActive;
                            unitOfWork.ParkingSpaces.Update(parkingSpace);
                            //await unitOfWork.CommitAsync(default);
                        }
                    }

                    return new TServiceResponse<bool>(true, "عملیات موفق", true);
                }

            }
            return new TServiceResponse<bool>(false, "خطای سیستمی", false);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطای سیستمی", false);
        }
    }

    public TServiceResponse<bool> IsActiveParking()
    {
        var localParking = unitOfWork.ParkingLots.FirstOrDefault();
        if (localParking != null)
        {
            return new TServiceResponse<bool>(true, "عملیات موفق", localParking.IsActive);
        }
        else
        {
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات پارکینگ", false);
        }
    }

    public async Task<TServiceResponse<bool>> IsActiveParkingAsync()
    {
        var localParking = await unitOfWork.ParkingLots.FirstOrDefaultAsync();
        if (localParking != null)
        {
            return new TServiceResponse<bool>(true, "عملیات موفق", localParking.IsActive);
        }
        else
        {
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات پارکینگ", false);
        }
    }

    public TServiceResponse<bool> ReceiveLicensePlateGroupFromServer()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var getListResponse = client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-plate-groupList").Result;
            var getListJsonResult = JsonConvert.DeserializeObject<ApiResponse<List<LicensePlateGroupModel>>>(getListResponse);
            if (getListJsonResult?.StatusCode == 200)
            {
                var localPlateGroups = unitOfWork.LicensePlateGroups.ToList();

                var toRemove = localPlateGroups.Where(a => !getListJsonResult.Data.Any(b => b.Id == a.Id)).ToList();
                foreach (var item in toRemove)
                {
                    var a = unitOfWork.LicensePlates.ExecuteDeleteAsync(l => l.GroupId == item.Id).Result;
                    //unitOfWork.Commit();
                    a = unitOfWork.LicensePlateGroups.ExecuteDeleteAsync(l => l.Id == item.Id).Result;
                    //unitOfWork.Commit();
                }
                foreach (var groupItem in getListJsonResult.Data)
                {
                    var licensePlateGroup = unitOfWork.LicensePlateGroups.GetById(groupItem.Id);

                    if (licensePlateGroup == null)
                    {
                        LicensePlateGroup newLicensePlateGroup = new LicensePlateGroup()
                        {
                            Id = groupItem.Id,
                            CreatorUserId = groupItem.CreatorUserId,
                            Description = groupItem.Description,
                            DiscountPercent = groupItem.DiscountPercent,
                            EndDate = groupItem.EndDate,
                            IsActive = groupItem.IsActive,
                            Name = groupItem.Name,
                            ParkingLotId = groupItem.ParkingLotId,
                            StartDate = groupItem.StartDate
                        };
                        unitOfWork.LicensePlateGroups.Add(newLicensePlateGroup);
                        //unitOfWork.Commit();

                        foreach (var subitem in groupItem.LicensePlates)
                        {
                            var licensePlate = unitOfWork.LicensePlates
                            .FirstOrDefault(p => p.GroupId == subitem.GroupId && p.EnLicensePlate == subitem.EnLicensePlate);
                            if (licensePlate == null)
                            {
                                LicensePlate newLicensePlate = new LicensePlate()
                                {
                                    Id = subitem.Id,
                                    GroupId = subitem.GroupId,
                                    EnLicensePlate = subitem.EnLicensePlate,
                                    FaLicensePlate = subitem.FaLicensePlate
                                };
                                unitOfWork.LicensePlates.Add(newLicensePlate);
                                //unitOfWork.Commit();
                            }
                            else
                            {
                                licensePlate = unitOfWork.LicensePlates.GetById(subitem.Id);
                                licensePlate.GroupId = subitem.GroupId;
                                licensePlate.EnLicensePlate = subitem.EnLicensePlate;
                                licensePlate.FaLicensePlate = subitem.FaLicensePlate;
                                unitOfWork.LicensePlates.Update(licensePlate);
                                //unitOfWork.Commit();
                            }
                        }
                    }
                    else
                    {
                        unitOfWork.LicensePlateGroups.ExecuteUpdate(p => p.Id == groupItem.Id,
                            update => update
                            .SetProperty(ticket => ticket.CreatorUserId, ticket => groupItem.CreatorUserId)
                             .SetProperty(ticket => ticket.Description, ticket => groupItem.Description)
                             .SetProperty(ticket => ticket.DiscountPercent, ticket => groupItem.DiscountPercent)
                             .SetProperty(ticket => ticket.EndDate, ticket => groupItem.EndDate)
                             .SetProperty(ticket => ticket.StartDate, ticket => groupItem.StartDate)
                             .SetProperty(ticket => ticket.IsActive, ticket => groupItem.IsActive)
                             .SetProperty(ticket => ticket.Name, ticket => groupItem.Name)
                             .SetProperty(ticket => ticket.ParkingLotId, ticket => groupItem.ParkingLotId)
                            );

                        //unitOfWork.Commit();
                        var a = unitOfWork.LicensePlates.ExecuteDeleteAsync(l => l.GroupId == licensePlateGroup.Id).Result;
                        //unitOfWork.Commit();
                        foreach (var subitem in groupItem.LicensePlates)
                        {
                            var licensePlate = unitOfWork.LicensePlates
                            .FirstOrDefault(p => p.GroupId == subitem.GroupId && p.EnLicensePlate == subitem.EnLicensePlate);

                            if (licensePlate == null)
                            {
                                LicensePlate newLicensePlate = new LicensePlate()
                                {
                                    GroupId = subitem.GroupId,
                                    EnLicensePlate = subitem.EnLicensePlate,
                                    FaLicensePlate = subitem.FaLicensePlate
                                };
                                unitOfWork.LicensePlates.Add(newLicensePlate);
                                //unitOfWork.Commit();
                            }
                            else
                            {
                                licensePlate = unitOfWork.LicensePlates.GetById(subitem.Id);
                                licensePlate.GroupId = subitem.GroupId;
                                licensePlate.EnLicensePlate = subitem.EnLicensePlate;
                                licensePlate.FaLicensePlate = subitem.FaLicensePlate;
                                unitOfWork.LicensePlates.Update(licensePlate);
                                //unitOfWork.Commit();
                            }
                        }
                    }

                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public async Task<TServiceResponse<bool>> ReceiveLicensePlateGroupFromServerAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var getListResponse = await client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-plate-groupList");
            var getListJsonResult = JsonConvert.DeserializeObject<ApiResponse<List<LicensePlateGroupModel>>>(getListResponse);
            if (getListJsonResult?.StatusCode == 200)
            {
                var localPlateGroups = unitOfWork.LicensePlateGroups.ToList();

                var toRemove = localPlateGroups.Where(a => !getListJsonResult.Data.Any(b => b.Id == a.Id)).ToList();
                foreach (var item in toRemove)
                {
                    await unitOfWork.LicensePlates.ExecuteDeleteAsync(l => l.GroupId == item.Id);
                    //unitOfWork.Commit();
                    await unitOfWork.LicensePlateGroups.ExecuteDeleteAsync(l => l.Id == item.Id);
                    //unitOfWork.Commit();
                }
                foreach (var groupItem in getListJsonResult.Data)
                {
                    var licensePlateGroup = unitOfWork.LicensePlateGroups.GetById(groupItem.Id);

                    if (licensePlateGroup == null)
                    {
                        LicensePlateGroup newLicensePlateGroup = new LicensePlateGroup()
                        {
                            Id = groupItem.Id,
                            CreatorUserId = groupItem.CreatorUserId,
                            Description = groupItem.Description,
                            DiscountPercent = groupItem.DiscountPercent,
                            EndDate = groupItem.EndDate,
                            IsActive = groupItem.IsActive,
                            Name = groupItem.Name,
                            ParkingLotId = groupItem.ParkingLotId,
                            StartDate = groupItem.StartDate
                        };
                        unitOfWork.LicensePlateGroups.Add(newLicensePlateGroup);
                        //await unitOfWork.CommitAsync(default);

                        foreach (var subitem in groupItem.LicensePlates)
                        {
                            var licensePlate = unitOfWork.LicensePlates
                            .FirstOrDefault(p => p.GroupId == subitem.GroupId && p.EnLicensePlate == subitem.EnLicensePlate);
                            if (licensePlate == null)
                            {
                                LicensePlate newLicensePlate = new LicensePlate()
                                {
                                    Id = subitem.Id,
                                    GroupId = subitem.GroupId,
                                    EnLicensePlate = subitem.EnLicensePlate,
                                    FaLicensePlate = subitem.FaLicensePlate
                                };
                                unitOfWork.LicensePlates.Add(newLicensePlate);
                                //await unitOfWork.CommitAsync(default);
                            }
                            else
                            {
                                licensePlate = unitOfWork.LicensePlates.GetById(subitem.Id);
                                licensePlate.GroupId = subitem.GroupId;
                                licensePlate.EnLicensePlate = subitem.EnLicensePlate;
                                licensePlate.FaLicensePlate = subitem.FaLicensePlate;
                                unitOfWork.LicensePlates.Update(licensePlate);
                                //await unitOfWork.CommitAsync(default);
                            }
                        }
                    }
                    else
                    {
                        unitOfWork.LicensePlateGroups.ExecuteUpdate(p => p.Id == groupItem.Id,
                            update => update
                            .SetProperty(ticket => ticket.CreatorUserId, ticket => groupItem.CreatorUserId)
                             .SetProperty(ticket => ticket.Description, ticket => groupItem.Description)
                             .SetProperty(ticket => ticket.DiscountPercent, ticket => groupItem.DiscountPercent)
                             .SetProperty(ticket => ticket.EndDate, ticket => groupItem.EndDate)
                             .SetProperty(ticket => ticket.StartDate, ticket => groupItem.StartDate)
                             .SetProperty(ticket => ticket.IsActive, ticket => groupItem.IsActive)
                             .SetProperty(ticket => ticket.Name, ticket => groupItem.Name)
                             .SetProperty(ticket => ticket.ParkingLotId, ticket => groupItem.ParkingLotId)
                            );

                        //await unitOfWork.CommitAsync(default);
                        var a = await unitOfWork.LicensePlates.ExecuteDeleteAsync(l => l.GroupId == licensePlateGroup.Id);
                        //await unitOfWork.CommitAsync(default);
                        foreach (var subitem in groupItem.LicensePlates)
                        {
                            var licensePlate = unitOfWork.LicensePlates
                            .FirstOrDefault(p => p.GroupId == subitem.GroupId && p.EnLicensePlate == subitem.EnLicensePlate); if (licensePlate == null)
                            {
                                LicensePlate newLicensePlate = new LicensePlate()
                                {
                                    GroupId = subitem.GroupId,
                                    EnLicensePlate = subitem.EnLicensePlate,
                                    FaLicensePlate = subitem.FaLicensePlate
                                };
                                unitOfWork.LicensePlates.Add(newLicensePlate);
                                //await unitOfWork.CommitAsync(default);
                            }
                            else
                            {
                                licensePlate = unitOfWork.LicensePlates.GetById(subitem.Id);
                                licensePlate.GroupId = subitem.GroupId;
                                licensePlate.EnLicensePlate = subitem.EnLicensePlate;
                                licensePlate.FaLicensePlate = subitem.FaLicensePlate;
                                unitOfWork.LicensePlates.Update(licensePlate);
                                //await unitOfWork.CommitAsync(default);
                            }
                        }
                    }

                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public TServiceResponse<bool> ReceiveSeizedLicensePlateFromServer()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var getListResponse = client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-seized-plate-List").Result;
            var getListJsonResult = JsonConvert.DeserializeObject<ApiResponse<List<SeizedLicensePlateModel>>>(getListResponse);
            if (getListJsonResult?.StatusCode == 200)
            {
                var localPlateGroups = unitOfWork.SeizedLicensePlates.ExecuteDeleteAsync(p => true).Result;
                foreach (var item in getListJsonResult.Data)
                {
                    var LicensePlate = unitOfWork.SeizedLicensePlates.FirstOrDefault(p => p.EnLicensePlate == item.EnLicensePlate);
                    if (LicensePlate == null)
                    {
                        SeizedLicensePlate newLicensePlate = new SeizedLicensePlate()
                        {

                            CreateDate = item.CreateDate,
                            CreatorUserId = item.CreatorUserId,
                            EnLicensePlate = item.EnLicensePlate,
                            FaLicensePlate = item.FaLicensePlate,
                            SeizedReason = item.SeizedReason,
                        };

                        unitOfWork.SeizedLicensePlates.Add(newLicensePlate);
                        //unitOfWork.Commit();
                    }
                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }

    }

    public async Task<TServiceResponse<bool>> ReceiveSeizedLicensePlateFromServerAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var getListResponse = await client.GetStringAsync($"{TokenStore.BaseUrl}/ParkingLot/get-seized-plate-List");
            var getListJsonResult = JsonConvert.DeserializeObject<ApiResponse<List<SeizedLicensePlateModel>>>(getListResponse);
            if (getListJsonResult?.StatusCode == 200)
            {
                var localPlateGroups = await unitOfWork.SeizedLicensePlates.ExecuteDeleteAsync(p => true);
                foreach (var item in getListJsonResult.Data)
                {
                    var LicensePlate = unitOfWork.SeizedLicensePlates.FirstOrDefault(p => p.EnLicensePlate == item.EnLicensePlate);
                    if (LicensePlate == null)
                    {
                        SeizedLicensePlate newLicensePlate = new SeizedLicensePlate()
                        {

                            CreateDate = item.CreateDate,
                            CreatorUserId = item.CreatorUserId,
                            EnLicensePlate = item.EnLicensePlate,
                            FaLicensePlate = item.FaLicensePlate,
                            SeizedReason = item.SeizedReason,
                        };

                        unitOfWork.SeizedLicensePlates.Add(newLicensePlate);
                        //await unitOfWork.CommitAsync(default);
                    }
                }
                return new TServiceResponse<bool>(true, "عملیات موفق", true);
            }
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public TServiceResponse<bool> ReceiveVehicleSegmentsListFromServer()
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var responce = client.GetStringAsync($"{TokenStore.BaseUrl}/VehicleSegment/segments-list").Result;
            var result = JsonConvert.DeserializeObject<ApiResponse<List<VehicleSegmentModel>>>(responce);
            if (result.Data != null)
            {
                List<ParkingVehicleSegmentPriceModel> pricesList = new List<ParkingVehicleSegmentPriceModel>();
                foreach (var vehicleSegment in result.Data)
                {
                    var segment = unitOfWork.VehicleSegments.GetById(vehicleSegment.Id);
                    if (segment != null)
                    {
                        segment.Description = vehicleSegment.Description;
                        segment.NameFa = vehicleSegment.NameFa;
                        segment.FreeEntranceMinutes = vehicleSegment.FreeEntranceMinutes;
                        segment.ParkingEntranceFixedFee = vehicleSegment.ParkingEntranceFixedFee;
                        segment.DailyRate = vehicleSegment.DailyRate;
                        segment.TaxPercentage = vehicleSegment.TaxPercentage;
                        segment.ThresholdHoursPerDay = vehicleSegment.ThresholdHoursPerDay;
                        segment.ThresholdNumberOfDays = vehicleSegment.ThresholdNumberOfDays;
                        segment.DailyPriceAfterCrossingThreshold = vehicleSegment.DailyPriceAfterCrossingThreshold;
                        segment.Image = vehicleSegment.Image;
                        unitOfWork.VehicleSegments.Update(segment);
                        //unitOfWork.Commit();

                        foreach (var item in vehicleSegment.VehicleSegmentPrices)
                        {
                            pricesList.Add(item);
                        }
                    }
                    else
                    {
                        VehicleSegment vs = new VehicleSegment()
                        {
                            Id = vehicleSegment.Id,
                            Image = vehicleSegment.Image,
                            NameFa = vehicleSegment.NameFa,
                            Description = vehicleSegment.Description,
                            CreatorUserId = vehicleSegment.CreatorUserId,
                            DailyRate = vehicleSegment.DailyRate,
                            FreeEntranceMinutes = vehicleSegment.FreeEntranceMinutes,
                            ParkingEntranceFixedFee = vehicleSegment.ParkingEntranceFixedFee,
                            ParkingLotId = vehicleSegment.ParkingLotId,
                            DailyPriceAfterCrossingThreshold = vehicleSegment.DailyPriceAfterCrossingThreshold,
                            TaxPercentage = vehicleSegment.TaxPercentage,
                            ThresholdHoursPerDay = vehicleSegment.ThresholdHoursPerDay,
                            ThresholdNumberOfDays = vehicleSegment.ThresholdNumberOfDays
                        };

                        unitOfWork.VehicleSegments.Add(vs);

                        //unitOfWork.Commit();

                        foreach (var item in vehicleSegment.VehicleSegmentPrices)
                        {
                            pricesList.Add(item);
                        }
                    }
                    var vehicleSegmentsResult = CreateVehicleSegmentsPrice(pricesList);



                    foreach (var variable in vehicleSegment.VehicleSegmentVariablePrices)
                    {
                        try
                        {

                            var variablePrice = unitOfWork.ParkingVehicleSegmentVariablePrices.GetById(variable.Id);
                            if (variablePrice != null)
                            {
                                variablePrice.VehicleSegmentId = vehicleSegment.Id;
                                variablePrice.ParkingLotId = variable.ParkingLotId;
                                variablePrice.Minutes = variable.Minutes;
                                variablePrice.Number = variable.Number;
                                variablePrice.Price = variable.Price;

                                unitOfWork.ParkingVehicleSegmentVariablePrices.Update(variablePrice);
                                //unitOfWork.Commit();
                            }
                            else
                            {
                                ParkingVehicleSegmentVariablePrice newVariablePrice = new ParkingVehicleSegmentVariablePrice()
                                {
                                    Id = variable.Id,
                                    VehicleSegmentId = vehicleSegment.Id,
                                    ParkingLotId = variable.ParkingLotId,
                                    Minutes = variable.Minutes,
                                    Number = variable.Number,
                                    Price = variable.Price
                                };
                                unitOfWork.ParkingVehicleSegmentVariablePrices.Add(newVariablePrice);
                                //unitOfWork.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
                        }

                    }


                }


            }
            return new TServiceResponse<bool>(true, "عملیات موفق", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public async Task<TServiceResponse<bool>> ReceiveVehicleSegmentsListFromServerAsync()
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var responce = await client.GetStringAsync($"{TokenStore.BaseUrl}/VehicleSegment/segments-list");
            var result = JsonConvert.DeserializeObject<ApiResponse<List<VehicleSegmentModel>>>(responce);
            if (result?.Data != null)
            {
                List<ParkingVehicleSegmentPriceModel> pricesList = new List<ParkingVehicleSegmentPriceModel>();
                foreach (var vehicleSegment in result.Data)
                {
                    var segment = unitOfWork.VehicleSegments.GetById(vehicleSegment.Id);
                    if (segment != null)
                    {
                        segment.Description = vehicleSegment.Description;
                        segment.NameFa = vehicleSegment.NameFa;
                        segment.FreeEntranceMinutes = vehicleSegment.FreeEntranceMinutes;
                        segment.ParkingEntranceFixedFee = vehicleSegment.ParkingEntranceFixedFee;
                        segment.DailyRate = vehicleSegment.DailyRate;
                        segment.TaxPercentage = vehicleSegment.TaxPercentage;
                        segment.ThresholdHoursPerDay = vehicleSegment.ThresholdHoursPerDay;
                        segment.ThresholdNumberOfDays = vehicleSegment.ThresholdNumberOfDays;
                        segment.DailyPriceAfterCrossingThreshold = vehicleSegment.DailyPriceAfterCrossingThreshold;
                        segment.Image = vehicleSegment.Image;
                        unitOfWork.VehicleSegments.Update(segment);
                        //await unitOfWork.CommitAsync(default);

                        foreach (var item in vehicleSegment.VehicleSegmentPrices)
                        {
                            pricesList.Add(item);
                        }
                    }
                    else
                    {
                        VehicleSegment vs = new VehicleSegment()
                        {
                            Id = vehicleSegment.Id,
                            Image = vehicleSegment.Image,
                            NameFa = vehicleSegment.NameFa,
                            Description = vehicleSegment.Description,
                            CreatorUserId = vehicleSegment.CreatorUserId,
                            DailyRate = vehicleSegment.DailyRate,
                            FreeEntranceMinutes = vehicleSegment.FreeEntranceMinutes,
                            ParkingEntranceFixedFee = vehicleSegment.ParkingEntranceFixedFee,
                            ParkingLotId = vehicleSegment.ParkingLotId,
                            DailyPriceAfterCrossingThreshold = vehicleSegment.DailyPriceAfterCrossingThreshold,
                            TaxPercentage = vehicleSegment.TaxPercentage,
                            ThresholdHoursPerDay = vehicleSegment.ThresholdHoursPerDay,
                            ThresholdNumberOfDays = vehicleSegment.ThresholdNumberOfDays
                        };

                        unitOfWork.VehicleSegments.Add(vs);
                        //await unitOfWork.CommitAsync(default);

                        foreach (var item in vehicleSegment.VehicleSegmentPrices)
                        {
                            pricesList.Add(item);
                        }
                    }
                    var vehicleSegmentsResult = CreateVehicleSegmentsPrice(pricesList);



                    foreach (var variable in vehicleSegment.VehicleSegmentVariablePrices)
                    {
                        try
                        {

                            var variablePrice = unitOfWork.ParkingVehicleSegmentVariablePrices.GetById(variable.Id);
                            if (variablePrice != null)
                            {
                                variablePrice.VehicleSegmentId = vehicleSegment.Id;
                                variablePrice.ParkingLotId = variable.ParkingLotId;
                                variablePrice.Minutes = variable.Minutes;
                                variablePrice.Number = variable.Number;
                                variablePrice.Price = variable.Price;

                                unitOfWork.ParkingVehicleSegmentVariablePrices.Update(variablePrice);
                                //await unitOfWork.CommitAsync(default);
                            }
                            else
                            {
                                ParkingVehicleSegmentVariablePrice newVariablePrice = new ParkingVehicleSegmentVariablePrice()
                                {
                                    Id = variable.Id,
                                    VehicleSegmentId = vehicleSegment.Id,
                                    ParkingLotId = variable.ParkingLotId,
                                    Minutes = variable.Minutes,
                                    Number = variable.Number,
                                    Price = variable.Price
                                };
                                unitOfWork.ParkingVehicleSegmentVariablePrices.Add(newVariablePrice);
                                //await unitOfWork.CommitAsync(default);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
                        }

                    }


                }


            }
            return new TServiceResponse<bool>(true, "عملیات موفق", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public TServiceResponse<bool> SendUnSyncedTicketToServer()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var localUnsyncedTickets = unitOfWork.ParkingTickets.Find(p => p.TicketStatus == TicketStatus.Unsynced).Take(Settings.Default.Application_Sync_Interval_CountOfTake).ToList();
            foreach (var ticket in localUnsyncedTickets)
            {
                SyncTicketRequestModel requestInfo = new SyncTicketRequestModel()
                {
                    Id = ticket.Id,
                    IsExited = ticket.IsExited,
                    BarcodeId = ticket.BarcodeId,
                    Description = ticket.Description,
                    Discount = ticket.Discount,
                    DiscountPercent = ticket.DiscountPercent,
                    DurationMinutes = ticket.DurationMinutes,
                    EndTime = ticket.EndTime,
                    EnLicensePlate = ticket.EnLicensePlate,
                    ExitImage = ticket.ExitImage,
                    IsPaid = ticket.IsPaid,
                    LicensePlate = ticket.LicensePlate,
                    PaidAmount = ticket.PaidAmount,
                    PaidCreditCard = ticket.PaidCreditCard,
                    PaidType = ticket.PaidType,
                    ParkingId = ticket.ParkingLotId,
                    ParkingSectionId = ticket.ParkingSectionId,
                    ParkingSpaceID = ticket.ParkingSpaceID,
                    RefId = ticket.RefId,
                    StartTime = ticket.StartTime,
                    TotalAmount = ticket.TotalAmount,
                    TotalAmountWithDiscount = ticket.TotalAmountWithDiscount,
                    VehicleColor = ticket.VehicleColor,
                    VehicleManufacturerName = ticket.VehicleManufacturerName,
                    VehicleSegmentId = ticket.VehicleSegmentId,
                    VehicleModel = ticket.VehicleModel,
                    EntryGate = ticket.EntranceGate,
                    ExitGate = ticket.ExitGate,
                    MerchantNumber = ticket.MerchantNumber,
                    PaidDate = ticket.PaidDate,
                    RRN = ticket.RRN,
                    TraceNo = ticket.TraceNo,
                    CreatorUserId = ticket.UserId,
                    IsCardMissing = ticket.IsCardMissing ?? false
                };

                if (ticket?.StartImage?.Length > 5)
                {
                    requestInfo.StartImage = ticket.StartImage;
                }

                var response = client.PostAsJsonAsync($"{TokenStore.BaseUrl}/Ticket/sync-ticket", requestInfo).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<ApiResponse<bool>>().Result;
                    if (result?.StatusCode == 200)
                    {
                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == ticket.Id, update => update
                                        .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Synced)
                                       .SetProperty(ticket => ticket.StartImage, ticket => null));
                        //unitOfWork.Commit();
                    }
                }
            }

            return new TServiceResponse<bool>(true, "عملیات موفق", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public async Task<TServiceResponse<bool>> SendUnSyncedTicketToServerAsync()
    {
        try
        {
            var localUnsyncedTickets = unitOfWork.ParkingTickets.Find(p => p.TicketStatus == TicketStatus.Unsynced).Take(Settings.Default.Application_Sync_Interval_CountOfTake).ToList();
            foreach (var ticket in localUnsyncedTickets)
            {
                SyncTicketRequestModel requestInfo = new SyncTicketRequestModel()
                {
                    Id = ticket.Id,
                    IsExited = ticket.IsExited,
                    BarcodeId = ticket.BarcodeId,
                    Description = ticket.Description,
                    Discount = ticket.Discount,
                    DiscountPercent = ticket.DiscountPercent,
                    DurationMinutes = ticket.DurationMinutes,
                    EndTime = ticket.EndTime,
                    EnLicensePlate = ticket.EnLicensePlate,
                    ExitImage = ticket.ExitImage,
                    IsPaid = ticket.IsPaid,
                    LicensePlate = ticket.LicensePlate,
                    PaidAmount = ticket.PaidAmount,
                    PaidCreditCard = ticket.PaidCreditCard,
                    PaidType = ticket.PaidType,
                    ParkingId = ticket.ParkingLotId,
                    ParkingSectionId = ticket.ParkingSectionId,
                    ParkingSpaceID = ticket.ParkingSpaceID,
                    RefId = ticket.RefId,
                    StartTime = ticket.StartTime,
                    TotalAmount = ticket.TotalAmount,
                    TotalAmountWithDiscount = ticket.TotalAmountWithDiscount,
                    VehicleColor = ticket.VehicleColor,
                    VehicleManufacturerName = ticket.VehicleManufacturerName,
                    VehicleSegmentId = ticket.VehicleSegmentId,
                    VehicleModel = ticket.VehicleModel,
                    EntryGate = ticket.EntranceGate,
                    ExitGate = ticket.ExitGate,
                    MerchantNumber = ticket.MerchantNumber,
                    PaidDate = ticket.PaidDate,
                    RRN = ticket.RRN,
                    TraceNo = ticket.TraceNo,
                    CreatorUserId = ticket.UserId,
                    ExitRegistrarUserId = ticket.ExitRegistrarUserId,
                    IsCardMissing = ticket.IsCardMissing ?? false
                };

                if (ticket?.StartImage?.Length > 5)
                {
                    requestInfo.StartImage = ticket.StartImage;
                }

                var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
                var response = await client.PostAsJsonAsync($"{TokenStore.BaseUrl}/Ticket/sync-ticket", requestInfo);

                string message = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    await Task.Delay(200);
                    if (result?.StatusCode == 200)
                    {
                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == ticket.Id, update => update
                                        .SetProperty(ticket => ticket.TicketStatus, ticket => TicketStatus.Synced)
                                       .SetProperty(ticket => ticket.StartImage, ticket => null));
                        //await unitOfWork.CommitAsync(default);
                    }
                }
            }

            return new TServiceResponse<bool>(true, "عملیات موفق", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new TServiceResponse<bool>(false, "خطا در دریافت اطلاعات", false);
        }
    }

    public void SyncTicketImage()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var tickets = unitOfWork.ParkingTickets.Find(p => p.StartImage == null && p.TicketStatus == TicketStatus.Synced).Take(Settings.Default.Application_Sync_Interval_CountOfTake)
                .Select(p => new { p.Id }).ToList();
            foreach (var item in tickets)
            {
                var responce = client.GetStringAsync($"{TokenStore.BaseUrl}/Ticket/get-ticket-image/{item.Id}").Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(responce);
                if (result?.Data != null)
                {
                    if (!unitOfWork.ParkingTicketImages.Find(t => t.TicketId == item.Id).Any())
                    {
                        unitOfWork.ParkingTicketImages.Add(new ParkingTicketImage
                        {
                            TicketId = item.Id,
                            CreateDateTime = DateTime.Now,
                            EntryImageAddress = result.Data
                        });
                        //unitOfWork.Commit();


                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == item.Id,
                            update => update
                            .SetProperty(ticket => ticket.StartImage, ticket => "0")
                            );
                        //unitOfWork.Commit();
                    }
                    else
                    {
                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == item.Id,
                                                update => update
                                                .SetProperty(ticket => ticket.StartImage, ticket => "0")
                                                );
                        //unitOfWork.Commit();
                    }

                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

        }
    }
    public void SyncTicketExitImage()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var tickets = unitOfWork.ParkingTicketImages.Find(p => p.ExitImageAddress == null)
                .Take(Settings.Default.Application_Sync_Interval_CountOfTake)
                .Select(p => new { Id = p.TicketId }).ToList();
            foreach (var item in tickets)
            {
                var responce = client.GetStringAsync($"{TokenStore.BaseUrl}/Ticket/get-ticket-exit-image/{item.Id}").Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(responce);
                if (result?.StatusCode == 200)
                {
                    if (result?.Data != null)
                    {
                        unitOfWork.ParkingTicketImages.ExecuteUpdate(g => g.TicketId == item.Id,
                                    update => update
                                    .SetProperty(image => image.ExitImageAddress, image => result.Data)
                                    );
                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == item.Id,
                            update => update
                            .SetProperty(ticket => ticket.ExitImage, ticket => "0")
                            );
                    }
                }

            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

        }
    }
    public async Task SyncTicketImageAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var tickets = unitOfWork.ParkingTickets.Find(p => p.StartImage == null && p.TicketStatus == TicketStatus.Synced).Take(Settings.Default.Application_Sync_Interval_CountOfTake)
                .Select(p => new { Id = p.Id }).ToList();
            foreach (var item in tickets)
            {
                var responce = await client.GetStringAsync($"{TokenStore.BaseUrl}/Ticket/get-ticket-image/{item.Id}");
                var result = JsonConvert.DeserializeObject<ApiResponse<string>>(responce);
                if (result?.Data != null)
                {
                    if (!unitOfWork.ParkingTicketImages.Find(t => t.TicketId == item.Id).Any())
                    {
                        unitOfWork.ParkingTicketImages.Add(new ParkingTicketImage
                        {
                            TicketId = item.Id,
                            CreateDateTime = DateTime.Now,
                            EntryImageAddress = result.Data
                        });
                        //await unitOfWork.CommitAsync(default);


                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == item.Id,
                            update => update
                            .SetProperty(ticket => ticket.StartImage, ticket => "0")
                            );
                        //await unitOfWork.CommitAsync(default);
                    }
                    else
                    {
                        unitOfWork.ParkingTickets.ExecuteUpdate(g => g.Id == item.Id,
                                                update => update
                                                .SetProperty(ticket => ticket.StartImage, ticket => "0")
                                                );
                        //await unitOfWork.CommitAsync(default);
                    }

                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task<bool> ServerConnectiviyCheckAsync()
    {
        try
        {
            TokenStore.BaseUrl = Settings.Default.Application_ApiServerAddress;
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var response = await client.GetStringAsync($"{TokenStore.BaseUrl}/Account/connection-check");
            var result = JsonConvert.DeserializeObject<string>(response);
            if (result?.ToLower() == "true")
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
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }



    public async Task SyncTicketExtraImagesAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.BearerToken);
            var localUnsyncedImages = unitOfWork.ParkingTicketExtraImages.Find(p => p.Image != null && !p.Image.StartsWith("http")).Take(Settings.Default.Application_Sync_Interval_CountOfTake).ToList();
            foreach (var ticket in localUnsyncedImages)
            {
                SyncTicketExtraImageRequestModel requestInfo = new SyncTicketExtraImageRequestModel()
                {
                    Image = ticket.Image,
                    CreateDateTime = ticket.CreateDateTime,
                    FaName = ticket.FaName,
                    GateName = ticket.GateName,
                    TicketId = ticket.TicketId,
                };

                var response = await client.PostAsJsonAsync($"{TokenStore.BaseUrl}/Ticket/add-extra-images", requestInfo);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    //await Task.Delay(100);
                    if (result?.StatusCode == 200)
                    {
                        unitOfWork.ParkingTicketExtraImages.ExecuteUpdate(g => g.Id == ticket.Id, update => update
                                        .SetProperty(ticket => ticket.Image, ticket => result.Data));
                        //await unitOfWork.CommitAsync(default);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
