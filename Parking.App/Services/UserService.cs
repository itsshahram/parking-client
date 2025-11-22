using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using Parking.Domain.General;

namespace Parking.App.Services;

public class UserService(IUnitOfWork _unitOfWork, ILogger<UserService> logger, UserManager<ApplicationUser> usermanager, RoleManager<ApplicationRole> roleManager) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;
    private IUnitOfWork unitOfWork = _unitOfWork;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();
    private readonly UserManager<ApplicationUser> _userManager = usermanager;
    public bool ChangePassword(Guid id, string newPassword)
    {
        try
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            unitOfWork.Users.Update(user);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user with ID: {UserId}", id);
            return false;
        }
    }

    public bool DeleteUser(Guid id)
    {
        try
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
                return false;

            _unitOfWork.Users.Delete(user);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return false;
        }
    }

    public List<ApplicationUser> GetAllUsers()
    {
        List<UserListItemModel> result = new List<UserListItemModel>();
        try
        {
            return unitOfWork.Users.GetAll().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return new List<ApplicationUser>();
        }
    }
    public async Task<List<UserListItemModel>> GetAllUsersAsync()
    {
        List<UserListItemModel> result = new List<UserListItemModel>();
        try
        {
            var users = _unitOfWork.Users.ToList();
            var roles = _unitOfWork.Roles.ToList();

            foreach (var item in users)
            {
                var userRole = _unitOfWork.ExecuteRawQuery<ApplicationUserRole>("SELECT * FROM AspNetUserRoles WHERE UserId = @p0", item.Id);
                var role = _unitOfWork.Roles.GetById(userRole.FirstOrDefault().RoleId);

                result.Add(new UserListItemModel
                {
                    Avatar = item.Avatar,
                    EmailConfirmed = item.EmailConfirmed,
                    Firstname = item.Firstname,
                    Lastname = item.Lastname,
                    Id = item.Id,
                    IsActive = item.IsActive,
                    PhoneNumberConfirmed = item.PhoneNumberConfirmed,
                    RegisterDate = item.RegisterDate,
                    UserName = item.UserName,
                    RoleFaName = role?.FaName,
                    RoleEnName = role?.Name,
                    RoleId = role?.Id,
                });
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return result;
        }
    }
    public string GetUserRoleByUserId(Guid userId)
    {
        try
        {
            var userRole = _unitOfWork.ExecuteRawQuery<ApplicationUserRole>("SELECT * FROM AspNetUserRoles WHERE UserId = @p0", userId);
            if (userRole == null || !userRole.Any())
                return string.Empty;

            var role = _unitOfWork.Roles.GetById(userRole.FirstOrDefault().RoleId);
            return role.Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return "";
        }
    }
    public ApplicationUser? GetUserById(Guid id)
    {
        try
        {
            return _unitOfWork.Users.GetById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return null;
        }
    }

    public ApplicationUser? GetUserByUsername(string username)
    {
        try
        {
            return _unitOfWork.Users.FirstOrDefault(u => u.UserName == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username: {Username}", username);
            return null;
        }
    }

    public LoginStatus Login(string username, string password)
    {
        try
        {
            ApplicationUser? user = new();
            if (username.IsMobile())
                user = unitOfWork.Users.FirstOrDefault(x => x.PhoneNumber == x.UserName);
            else
                user = _unitOfWork.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
                return LoginStatus.NotFound;
            //check format is mobile

            if (!user.IsActive)
                return LoginStatus.NotActice;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
                return LoginStatus.Success;

            return LoginStatus.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user with username: {Username}", username);
            return LoginStatus.Failed;
        }
    }

    public bool Register(string username, string password)
    {
        try
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = username
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            _unitOfWork.Users.Add(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with username: {Username}", username);
            return false;
        }
    }
    public async Task<(bool IsSuccess, bool IsExist)> CreateUser(ApplicationUser user, string role, string password)
    {
        try
        {
            var isExist = await _userManager.FindByEmailAsync(user.UserName);
            if (isExist != null)
                return (false, true);
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, false);

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                return (false, false);

            return (true, false);
        }
        catch (Exception ex)
        {
            return (false, false);
        }
    }


    public bool UpdateUser(ApplicationUser user)
    {
        try
        {
            var existingUser = _unitOfWork.Users.GetById(user.Id);
            if (existingUser == null)
                return false;
            existingUser.Firstname = user.Firstname;
            existingUser.Lastname = user.Lastname;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UserName = user.UserName;
            existingUser.PasswordHash = user.PasswordHash;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
            return false;
        }
    }

    public async Task<bool> ChangeStaus(Guid id, bool status)
    {
        var existingUser = await _userManager.FindByIdAsync(id.ToString());
        if (existingUser == null)
            return false;

        existingUser.IsActive = status;

        await _userManager.UpdateAsync(existingUser);

        return true;
    }

    public async Task<ApplicationUserRole?> GetUserRole(Guid UserId)
    {
        var userRole = await _unitOfWork.ExecuteRawQueryAsync<ApplicationUserRole>("SELECT * FROM AspNetUserRoles WHERE UserId = @p0", UserId);
        return userRole.FirstOrDefault();
    }
}
