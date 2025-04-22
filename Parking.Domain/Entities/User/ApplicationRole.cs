using Microsoft.AspNetCore.Identity;

namespace Parking.Domain.Entities.User;
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string name, string faName) : base(name)
    {
        FaName = faName;
    }
    public required string FaName { get; set; }

}

