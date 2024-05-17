using NCloud.ConstantData;
using System.Security.Cryptography;
using System.Text;

namespace NCloud.Security
{
    /// <summary>
    /// Class to encrpypt shared folders and files path in url
    /// </summary>
    public static class HashManager
    {
        /// <summary>
        /// Static method to encrypt path for web shared files and folders
        /// </summary>
        /// <param name="input">Path of file / folder</param>
        /// <returns>Encrypted path as string</returns>
        public static string EncryptString(string? input)
        {
            if (String.IsNullOrWhiteSpace(input))
                return String.Empty;

            try
            {
                using (Aes aesManager = Aes.Create())
                {
                    aesManager.Key = Convert.FromBase64String(Constants.AesKey);
                    aesManager.IV = Convert.FromBase64String(Constants.IV);

                    ICryptoTransform encryptor = aesManager.CreateEncryptor(aesManager.Key, aesManager.IV);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(Encoding.UTF8.GetBytes(input),0, input.Length);
                            cryptoStream.FlushFinalBlock();
                            return Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Static method to decrypt path for web shared files and folders
        /// </summary>
        /// <param name="input">Path of file / folder</param>
        /// <returns>Decrypted path as string</returns>
        public static string DecryptString(string? input)
        {
            if (String.IsNullOrWhiteSpace(input))
                return String.Empty;

            try
            {
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
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
