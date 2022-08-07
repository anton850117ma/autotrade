using System;
using System.IO;
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

            eventHandler.login();
            // while(!eventHandler.isLogined);

            eventHandler.registerTargets();

            // if (!eventHandler.logout()) return;

            // dataHandler.storeRecords();
        }
    }
}
