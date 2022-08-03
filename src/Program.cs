using System;
using System.IO;
using System.Reflection;
// using System.Text.RegularExpressions;
// using System.Runtime.InteropServices;

namespace AutoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = @"data//Settings.json";
            var records = @"data//Records.json";
            var dataHandler = new DataHandler(settings, records);
            
            // var eventHandler = new EventHandler();
            // var login = handler.Login(true, 1000, 
            // "itstradeuat.pscnet.com.tw", 11002, "A100000261", "AA123456");
            // while(login);

        }
    }
}
