using System.Security.Cryptography;

namespace Parking.App.Helpers
{
    public static class AesEncryption
    {
        public static byte[] Encrypt(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return ms.ToArray();
        }

        public static string Decrypt(byte[] cipherData, byte[] key)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;

            // Extract IV (first block size bytes)
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(cipherData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }

        public static byte[] GenerateKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            return aes.Key;
        }




        public static void SaveKey(byte[] key, string path)
        {
            var protectedKey = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, protectedKey);
        }

        public static byte[] LoadKey(string path)
        {
            var protectedKey = File.ReadAllBytes(path);
            return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
        }


        public static byte[] LoadOrCreateAesKey(string path)
        {
            if (File.Exists(path))
                return LoadKey(path);

            var newKey = GenerateKey();
            SaveKey(newKey, path);
            return newKey;
        }
    }
}
