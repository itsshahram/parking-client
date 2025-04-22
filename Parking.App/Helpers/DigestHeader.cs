using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers;

public class DigestHeader
{
    private string _ha1;
    private string _ha2;
    private string _cnonce;

    public DigestHeader(string wwwAuthenticateHeader, string username, string password, string uri, string httpMethod)
    {
        var headerParams = wwwAuthenticateHeader.Replace("Digest ", "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var realm = headerParams.FirstOrDefault(p => p.StartsWith("realm="))?.Split('=')[1].Replace("\"", "");
        var nonce = headerParams.FirstOrDefault(p => p.StartsWith("nonce="))?.Split('=')[1].Replace("\"", "");
        var qop = headerParams.FirstOrDefault(p => p.StartsWith("qop="))?.Split('=')[1].Replace("\"", "");
        _cnonce = new Random().Next(123400, 9999999).ToString();

        _ha1 = CalculateMD5Hash($"{username}:{realm}:{password}");
        _ha2 = CalculateMD5Hash($"{httpMethod}:{uri}");
        var response = CalculateMD5Hash($"{_ha1}:{nonce}:00000001:{_cnonce}:{qop}:{_ha2}");

        HeaderValue = $"username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uri}\", qop={qop}, nc=00000001, cnonce=\"{_cnonce}\", response=\"{response}\", opaque=\"\"";
    }

    public string HeaderValue { get; }

    public string GetDigestHeaderValue()
    {
        return HeaderValue;
    }

    private static string CalculateMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}


