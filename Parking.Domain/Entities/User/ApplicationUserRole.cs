using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.User;

public class ApplicationUserRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

}
