using System;
using System.Threading;

namespace AutoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program begins!");

            var eventHandler = new EventHandler(new DataHandler());

            if (!eventHandler.updateCapitalOnly())
            {
                Console.WriteLine("Waiting to login...");
                // TODO: make login also check logout time
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
            }
            
            eventHandler.storeRecords();

            Console.WriteLine("Program ends! Type any keys to close.");
            Console.ReadKey(true);
        }
    }
}
