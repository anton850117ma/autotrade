using System;
using System.IO;
using System.Threading;
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
            var eventHandler = new EventHandler(new DataHandler(settings, records));

            while (!eventHandler.login())
            {
                Console.WriteLine("retry\n");
                Thread.Sleep(1000);
            }


            while (!eventHandler.shouldLogout()) 
            {
                Thread.Sleep(1000);
            }

            eventHandler.logout();
            eventHandler.storeRecords();
        }
    }
}
