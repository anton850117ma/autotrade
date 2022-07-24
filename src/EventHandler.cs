using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rayin;

namespace AutoTrade
{
    public class EventHandler
    {

        RayinAPI.OnLoginEvent? onLogin;

        public EventHandler()
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            onLogin = new RayinAPI.OnLoginEvent(OnLoginStatus);
            RayinAPI.SetOnLoginEvent(onLogin);
        }

        void OnLoginStatus(string status)
        {
            if (status.IndexOf("<ret ") >= 0 &&
                Utility.GetXMLValue(status, "status") != "OK")
            {
                Console.WriteLine("Error: Login failed");
                return;
            }

            //下單連線 && 回報連線 && 行情連線
            if (RayinAPI.ConnectToOrderServer() &&
                RayinAPI.ConnectToAckMatServer() &&
                RayinAPI.ConnectToQuoteServer())
            {
                Console.WriteLine("Error: Connect failed");
            }

            Console.WriteLine("Status: Login success");
        }

        public void Login()
        {
            // RayinAPI.SetMultiThread(chk_enMultiThread.Checked);
            // RayinAPI.SetDebugMode(true); //是否產生除錯log
            // RayinAPI.SetRecvTimeout(int.Parse(tb_recv.Text));
            // RayinAPI.SetServer(tb_ip.Text, int.Parse(tb_port.Text));
            // RayinAPI.Login(tb_username.Text, tb_password.Text);
        }
    }
}