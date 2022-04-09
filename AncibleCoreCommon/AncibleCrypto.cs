using System.IO;
using System.Security.Cryptography;

namespace AncibleCoreCommon
{
    public static class AncibleCrypto
    {
        public static byte[] Encrypt(byte[] data, byte[] key, out byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                iv = aes.IV;
                using (var cipher = new MemoryStream())
                using (var cryptoStream = new CryptoStream(cipher, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.Close();
                    return cipher.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                using (var cipher = new MemoryStream())
                {
                    using (var cs = new CryptoStream(cipher, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                        return cipher.ToArray();
                    }
                }
            }
        }
    }
}