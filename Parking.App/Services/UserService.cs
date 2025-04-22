using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic.ApplicationServices;
using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace Parking.App.Services;

public class UserService(IUnitOfWork _unitOfWork, ILogger<UserService> logger, UserManager<ApplicationUser> usermanager, RoleManager<ApplicationRole> roleManager) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;
    private IUnitOfWork unitOfWork = _unitOfWork;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();
    private readonly UserManager<ApplicationUser> _usermanager = usermanager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    public bool ChangePassword(Guid id, string newPassword)
    {
        try
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            unitOfWork.Users.Update(user);
            //_unitOfWork.Complete();
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
            //_unitOfWork.Complete();
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
            var users = _unitOfWork.Users.GetAll().ToList();
            var roles = _unitOfWork.Roles.GetAll().ToList();
            
            foreach (var item in users)
            {
                //var __roleManager = App.GetService<RoleManager<ApplicationRole>>();
                //var __usermanager = App.GetService<UserManager<ApplicationUser>>();
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
            if(userRole == null || !userRole.Any())
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
    public ApplicationUser GetUserById(Guid id)
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

    public ApplicationUser GetUserByUsername(string username)
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

    public bool Login(string username, string password)
    {
        try
        {
            var user = _unitOfWork.Users.FirstOrDefault(u => u.UserName == username && u.IsActive == true);
            if (user == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user with username: {Username}", username);
            return false;
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


}
