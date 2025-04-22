using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.User;

public class UserListItemModel
{
    public UserListItemModel()
    {
        Fullname = Firstname + " " + Lastname;  
    }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Fullname { get; set; }
    public DateTime RegisterDate { get; set; }
    public string? UserName { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? Avatar { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? Role { get; set; }
    public string? RoleFaName { get; set; }
    public string? RoleEnName { get; set; }
}
