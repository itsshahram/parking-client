using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Services.Interfaces;

public interface IUserService
{
    bool Login(string username, string password);
    bool Register(string username, string password);
    List<ApplicationUser> GetAllUsers();
    Task<List<UserListItemModel>> GetAllUsersAsync();
    ApplicationUser GetUserById(Guid id);
    string GetUserRoleByUserId(Guid userId);
    ApplicationUser GetUserByUsername(string username);
    bool UpdateUser(ApplicationUser user);
    bool DeleteUser(Guid id);
    bool ChangePassword(Guid id, string newPassword);

}
