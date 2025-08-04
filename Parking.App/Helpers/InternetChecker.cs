using System.Net.NetworkInformation;

namespace Parking.App.Helpers
{
    public static class InternetChecker
    {
        public static bool IsInternetAvailable()
        {
            if (IsPingSuccessful("8.8.8.8"))
                return true;

            return false;
        }

        private static bool IsPingSuccessful(string host)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(host, 1000);
                return reply?.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
