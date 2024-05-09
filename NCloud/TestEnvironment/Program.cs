using System;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Nodes;
using NCloud.Security;
using NCloud.Services;

class Program
{
    static void Main()
    {
        //string path = Path.Combine(Directory.GetCurrentDirectory(), "Test", "..", "Test.txt");

        string path = ".";

        Console.WriteLine(path);
        path = path[..^1];

        Console.WriteLine(path);
    }
}