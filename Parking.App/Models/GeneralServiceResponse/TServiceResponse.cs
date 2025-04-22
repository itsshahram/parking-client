using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.GeneralServiceResponse;

public class TServiceResponse<T> : ServiceResponse
{
    public TServiceResponse() : base(false, "Error")
    {

    }
    public TServiceResponse(string message) : base(false, message)
    {
    }



    public TServiceResponse(T result, string message) : base(true, message)
    {
        Result = result;
    }

    public TServiceResponse(T result) : this(result, string.Empty)
    {
    }
    public TServiceResponse(bool succeeded, string message, T? result = default) : base(succeeded, message)
    {
        Result = result;
    }


    public T? Result { get; set; }

}
