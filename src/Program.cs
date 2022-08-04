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
            var dataHandler = new DataHandler(settings, records);

            if (dataHandler.config == null) return;
            var eventHandler = new EventHandler(dataHandler.config.Login);

            if (!eventHandler.login()) return;
            // while(!eventHandler.isLogined);

            eventHandler.registerTargets(dataHandler.config.Rules, ref dataHandler.targetMap);

            // if (!eventHandler.logout()) return;

            // dataHandler.storeRecords();
        }
    }
}
