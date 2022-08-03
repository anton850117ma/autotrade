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
        dynamic? config;
        RayinAPI.OnLoginEvent? onLogin;
        RayinAPI.OnConnectEvent? onConnect;

        public EventHandler(dynamic? config)
        {
            this.config = config;
            registerEvents();
        }

        public void registerEvents()
        {
            onLogin = new RayinAPI.OnLoginEvent(onLoginStatus);
            RayinAPI.SetOnLoginEvent(onLogin);
        }

        private void onLoginStatus(string status)
        {
            if (status.IndexOf("<ret ") >= 0 &&
                Utility.GetXMLValue(status, "status") != "OK")
            {
                Console.WriteLine("Error: " + Utility.GetXMLValue(status, "msg"));
                return;
            }

            this.isLogined = true;
            Console.WriteLine("Status: Login OK");

            if (RayinAPI.ConnectToOrderServer() &&
                RayinAPI.ConnectToAckMatServer() &&
                RayinAPI.ConnectToQuoteServer())
            {
                Console.WriteLine("Error: Connect failed");
            }

            Console.WriteLine("Status: Connect OK");
        }

        public bool login()
        {
            if (this.config == null) return false;
            isLogined = false;
            RayinAPI.SetDebugMode(Convert.ToBoolean(this.config.debug));
            RayinAPI.SetRecvTimeout(Convert.ToInt32(this.config.timeout));
            RayinAPI.SetServer(
                    Convert.ToString(this.config.host), Convert.ToInt32(this.config.port));
            return RayinAPI.Login(
                    Convert.ToString(this.config.account), Convert.ToString(this.config.password));
        }

        public bool logout()
        {
            var result = RayinAPI.Logout();
            if (result)
            {
                this.isLogined = false;
                Console.WriteLine("Status: Logout OK");
            }
            return result;
        }


    }
}