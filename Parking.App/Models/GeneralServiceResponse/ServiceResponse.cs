using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.GeneralServiceResponse;

public class ServiceResponse
{

    public ServiceResponse()
    {
        Succeeded = false;
        Message = string.Empty;
    }

    public ServiceResponse(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; set; }
    public string Message { get; set; }
}
