using Parking.App.Models.Dto.Parking.ParkingLot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public class ParkingLotInfoStore
{
    private static ParkingLotModel _parking;
    public static ParkingLotModel ParkingInfo
    {
        get => _parking;
        set => _parking = value;
    }

}
