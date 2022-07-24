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
            

            // StreamReader reader = File.OpenText("configuration.toml");

            // Parse the table
            // TomlTable table = TOML.Parse(reader);

            



            // Console.WriteLine(table["title"]);  // Prints "TOML Example"

            // // You can check the type of the node via a property and access the exact type via As*-property
            // Console.WriteLine(table["owner"]["dob"].IsDateTime);  // Prints "True"

            // // You can also do both with C# 7 syntax
            // if (table["owner"]["dob"] is TomlDate date)
            //     Console.WriteLine(date.OnlyDate); // Some types contain additional properties related to formatting

            // // You can also iterate through all nodes inside an array or a table
            // foreach (TomlNode node in table["database"]["ports"])
            //     Console.WriteLine(node);

        }
    }
}
