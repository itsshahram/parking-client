using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public static class RandomNumberGenerator
{
    private static readonly Random _random = new Random();

    public static string GenerateRandomNumber()
    {
        const int length = 13;
        var randomNumber = new char[length];
        for (int i = 0; i < length; i++)
        {
            randomNumber[i] = (char)('0' + _random.Next(0, 10));
        }
        return new string(randomNumber);
    }
    public static long GenerateLongRandomNumber()
    {
        // Generate a random number between 1_000_000_000_000 and 9_999_999_999_999
        long result = _random.Next(1_000, 10_000) * 1_000_000_000_000L + (long)_random.Next(0, 1_000_000_000);
        return result;
    }
}
