using System;
using System.Security.Cryptography;
using System.Security.Policy;
using NCloud.Security;

class Program
{
    static void Main()
    {
        var temp = HashManager.EncryptString("https://localhost/WebDetails?path=@CloudRoot%5CDetails");
        Console.WriteLine(temp);
        Console.WriteLine(HashManager.DecryptString(temp));
    }
}