using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using Parking.Domain.General;

namespace Parking.App.Services.Interfaces;

public interface IUserService
{
    LoginStatus Login(string username, string password);
    bool Register(string username, string password);
    List<ApplicationUser> GetAllUsers();
    Task<List<UserListItemModel>> GetAllUsersAsync();
    ApplicationUser GetUserById(Guid id);
    string GetUserRoleByUserId(Guid userId);
    ApplicationUser GetUserByUsername(string username);
    Task<(bool IsSuccess, bool IsExist)> CreateUser(ApplicationUser user, string role, string password);
    bool UpdateUser(ApplicationUser user);
    bool DeleteUser(Guid id);
    bool ChangePassword(Guid id, string newPassword);
    Task<bool> ChangeStaus(Guid id, bool status);
}
