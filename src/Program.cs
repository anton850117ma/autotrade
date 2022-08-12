using System;
using System.Threading;

namespace AutoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program starts!");

            //TODO: make all files with the exe
            var settings = @"data//Settings.json";
            var eventHandler = new EventHandler(new DataHandler(settings));

            // Console.WriteLine("Waiting to login...");
            // while (!eventHandler.login())
            // {
            //     Thread.Sleep(1000);
            // }

            // Console.WriteLine("Trading...");
            // while (!eventHandler.shouldLogout()) 
            // {
            //     Thread.Sleep(1000);
            // }

            // Console.WriteLine("Logout and waiting to store data...");
            // eventHandler.logout();
            // eventHandler.storeRecords();

            Console.WriteLine("Program ends! Type any keys to close.");
            Console.ReadKey(true);
        }
    }
}
