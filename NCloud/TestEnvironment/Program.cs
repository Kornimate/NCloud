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
        string file = "./Test2.txt";
        string file2 = file;

        Console.WriteLine(file);
        Console.WriteLine(file2);

        file = "ok";

        Console.WriteLine(file);
        Console.WriteLine(file2);

        //FileInfo f = new FileInfo(file);

        //Console.WriteLine(f.Exists.ToString());

        //using (FileStream fs = File.Create(file)) { }

        //f = new FileInfo(file);

        //Console.WriteLine(f.Exists.ToString());
    }
}