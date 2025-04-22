using MiFare.Classic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers.Card;

public static class CardUIDHelper
{
    public static long GetCardUID(this byte[] cardUID)
    {
        return Convert.ToInt64((cardUID).Reverse().ToArray().ByteArrayToString(), 16);
    }
}
