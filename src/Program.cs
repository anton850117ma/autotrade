using System;
using Tommy;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace AutoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            EventHandler handler = new EventHandler();

            var login = handler.Login(true, true, 1000, "itstradeuat.pscnet.com.tw", 11002, "A100000261", "AA123456");
            while(login);
            
            

            // StreamReader reader = File.OpenText("configuration.toml");

            // Parse the table
            // TomlTable table = TOML.Parse(reader);

        }
    }
}
