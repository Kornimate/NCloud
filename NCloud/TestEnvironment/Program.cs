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
        var data = JsonSerializer.Serialize("&_oksa");
        var write = JsonSerializer.Deserialize<string>(data);
        Console.Write(write);
    }
}