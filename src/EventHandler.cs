using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rayin;

namespace AutoTrade
{
    public class Status
    {
        public bool isLogined;
        public bool isOrderConnected;
        public bool isAckMatConnected;
        public bool isQuoteConnected;
    }
    public class EventHandler : Status
    {
        RayinAPI.OnLoginEvent? onLogin;
        RayinAPI.OnConnectEvent? onConnect;

        public EventHandler()
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            onLogin = new RayinAPI.OnLoginEvent(OnLoginStatus);
            RayinAPI.SetOnLoginEvent(onLogin);
        }

        private void OnLoginStatus(string status)
        {
            if (status.IndexOf("<ret ") >= 0 &&
                Utility.GetXMLValue(status, "status") != "OK")
            {
                Console.WriteLine("Error: " + Utility.GetXMLValue(status, "msg"));
                return;
            }

            isLogined = true;
            // Interlocked.Exchange(ref isLogined, true);

            Console.WriteLine("Status: Login OK");

            //下單連線 && 回報連線 && 行情連線
            if (RayinAPI.ConnectToOrderServer() &&
                RayinAPI.ConnectToAckMatServer() &&
                RayinAPI.ConnectToQuoteServer())
            {
                Console.WriteLine("Error: Connect failed");
            }

            Console.WriteLine("Status: Connect OK");
        }

        public bool Login(bool use_debug, int timeout,
                          string host, int port, string account,
                          string password)
        {
            isLogined = false;
            RayinAPI.SetDebugMode(use_debug); //是否產生除錯log
            RayinAPI.SetRecvTimeout(timeout);
            RayinAPI.SetServer(host, port);
            return RayinAPI.Login(account, password);
        }

        public bool Logout()
        {
            return RayinAPI.Logout();
        }


    }
}