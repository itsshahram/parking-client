using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.GeneralServiceResponse;

public class TServicePageResponse<T> : TServiceResponse<T>
{
    public TServicePageResponse(string message) : base(false, message)
    {
    }

    public TServicePageResponse(T result, long totalRecords, string message) : base(true, message)
    {
        Result = result;
        TotalRecords = totalRecords;
    }

    public TServicePageResponse(T result, long totalRecords) : this(result, totalRecords, string.Empty)
    {
    }

    public long TotalRecords { get; set; }
}
