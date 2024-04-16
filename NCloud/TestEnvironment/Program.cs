using System;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        // Generate a random 128-bit key
        byte[] key = GenerateRandomBytes(16);
        string keyString = Convert.ToBase64String(key);
        Console.WriteLine("Generated Key: " + keyString);

        // Generate a random 128-bit IV
        byte[] iv = GenerateRandomBytes(16);
        string ivString = Convert.ToBase64String(iv);
        Console.WriteLine("Generated IV: " + ivString);

        using (Aes aesManager = Aes.Create())
        {
            Console.WriteLine(Convert.ToBase64String(aesManager.IV));
        }

        static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}