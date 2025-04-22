using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public static class StringValidator
{
    public static bool IsNumeric(this string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
    }
}
