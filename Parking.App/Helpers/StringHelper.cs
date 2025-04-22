using System.Security.Cryptography;
using System.Text;

namespace Parking.App.Helpers;
public static class StringHelper
{
    public static string Encrypt(this string plainText, string key)
    {
        var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(this string encryptedText, string key)
    {
        var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
    public static string RoleToPersian(this string role)
    {
        return role switch
        {
            "Admin" => "مدیر",
            "User" => "کاربر",
            "ParkingAdmin" => "مدیر کل پارکینگ",
            "ParkingManager" => "مدیر پارکینگ",
            "ParkingAgent" => "مسئول پارکینگ",
            _ => role
        };
    }
}



