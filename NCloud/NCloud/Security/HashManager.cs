using NCloud.ConstantData;
using System.Security.Cryptography;
using System.Text;

namespace NCloud.Security
{
    public static class HashManager
    {
        public static string EncryptString(string? input)
        {
            if (input is null)
                return String.Empty;

            using (Aes aesManager = Aes.Create())
            {
                aesManager.Key = Convert.FromBase64String(Constants.AesKey);
                aesManager.IV = Convert.FromBase64String(Constants.IV);

                ICryptoTransform encryptor = aesManager.CreateEncryptor(aesManager.Key, aesManager.IV);

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(input);

                            return Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }
                }
            }
        }

        public static string DecryptString(string? input)
        {
            if (input is null)
                return String.Empty;

            using (Aes aesManager = Aes.Create())
            {
                aesManager.Key = Convert.FromBase64String(Constants.AesKey);
                aesManager.IV = Convert.FromBase64String(Constants.IV);

                ICryptoTransform decryptor = aesManager.CreateDecryptor(aesManager.Key, aesManager.IV);

                using (var memoryStream = new MemoryStream(Convert.FromBase64String(input)))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(cryptoStream))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
