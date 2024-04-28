using System;
using System.Security.Cryptography;
using System.Security.Policy;
using NCloud.Security;
using NCloud.Services;

class Program
{
    static void Main()
    {
        for(int i=0; i <10;i++)
        {
            new Thread(() =>
            {
                string s = File.ReadAllText("./Test.txt");

                Console.WriteLine($"Finished {i}.");
            }).Start();
        }
    }
}