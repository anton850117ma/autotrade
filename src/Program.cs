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
            
            string targets = @"data//測試環境參考價_上市.txt";
            string quotes = @"data//Quotes.ini";
            string stocks = @"data//Stocks.ini";
            DataHandler dataHandler = new DataHandler(targets, quotes, stocks);

            // EventHandler handler = new EventHandler();
            // var login = handler.Login(true, 1000, "itstradeuat.pscnet.com.tw", 11002, "A100000261", "AA123456");
            // while(login);



        }
    }
}
