using System;
using System.IO;
using System.Threading;

namespace AutoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program starts!");

            var settings = @"data//Settings.json";
            var records = @"data//Records.json";
            var eventHandler = new EventHandler(new DataHandler(settings, records));

            Console.WriteLine("Waiting to login...");
            while (!eventHandler.login())
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Trading...");
            while (!eventHandler.shouldLogout()) 
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Logout and waiting to store data...");
            eventHandler.logout();
            eventHandler.storeRecords();

            Console.WriteLine("Program ends!");
        }
    }
}
