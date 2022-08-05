using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        List<string> logs;
        int reportCount;
        RayinAPI.OnLoginEvent? onLogin;
        RayinAPI.OnConnectEvent? onOdrConnect;
        RayinAPI.OnConnectEvent? onAckMatConnect;
        RayinAPI.OnConnectEvent? onQuoteConnect;
        RayinAPI.OnDisConnectEvent? onOdrDisConnect;
        RayinAPI.OnDisConnectEvent? onAckMatDisconnect;
        RayinAPI.OnDisConnectEvent? onQuoteDisconnect;
        RayinAPI.OnInstantAckEvent? onInstantAck;
        RayinAPI.OnNewAckEvent? onNewAck;
        RayinAPI.OnNewMatEvent? onNewMat;
        RayinAPI.OnNewQuoteEvent? onNewQuote;
        RayinAPI.OnErrorEvent? onOdrErr;
        RayinAPI.OnErrorEvent? onAckMatErr;
        RayinAPI.OnMsgAlertEvent? onMsgAlert;

        private void addLog(string message)
        {
            // lock (logs)
            // {
            //     logs.Add(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
            // }
        }
        private void OnLogin(string status)
        {
            if (status.IndexOf("<ret ") >= 0 &&
                Utility.getXMLValue(status, "status") != "OK")
            {
                addLog("登入失敗: " + Utility.getXMLValue(status, "msg"));
                return;
            }

            this.isLogined = true;
            addLog("登入成功");

            if (RayinAPI.ConnectToOrderServer() &&
                RayinAPI.ConnectToAckMatServer() &&
                RayinAPI.ConnectToQuoteServer()) addLog("嘗試連線伺服器成功");
            else addLog("嘗試連線伺服器失敗");
        }
        private void OnOdrConnect()
        {
            this.isOrderConnected = true;
            addLog("下單線路已連線");
        }
        private void OnOdrDisConnect()
        {
            this.isOrderConnected = false;
            addLog("下單線路已斷線");
        }
        private void OnOdrError(int errCode, string errMsg)
        {
            // TODO: should handle stock
            addLog("下單線路異常: " + errCode.ToString() + " " + errMsg);
        }
        private void OnAckMatConnect()
        {
            this.isAckMatConnected = true;
            // do not refill order report
            RayinAPI.Recover(this.reportCount.ToString());
            addLog("主動回報線路已連線");
        }
        private void OnAckMatDisConnect()
        {
            this.isAckMatConnected = false;
            addLog("主動回報線路已斷線");
        }
        private void OnAckMatError(int errCode, string errMsg)
        {
            addLog("主動回報線路異常: " + errCode.ToString() + " " + errMsg);
        }
        private void OnQuoteConnect()
        {
            this.isQuoteConnected = true;
            addLog("行情線路已連線");
        }
        private void OnQuoteDisConnect()
        {
            this.isQuoteConnected = false;
            addLog("行情線路已斷線");
        }
        private void OnNewQuote(string NewQuote)
        {
            string[] args = Regex.Split(NewQuote, @"\|\|", RegexOptions.IgnoreCase);
            // TODO: from here
            // addLog("NewQuote:[" + NewQuote + "]");
        }
        public void OnInstantAck(string ExecType, string ClOrdId, string BranchId,
                                 string Account, string OrderDate, string OrderTime,
                                 string OrderID, string Symbol, int ExCode,
                                 string Side, string OrdType, int PriceFlag, Single Price,
                                 int OrderQty, int BeforeQty, int AfterQty,
                                 string TimeInForce, string errCode, string errMsg)
        {
            // only handle new orders
            if (ExecType == "O")
            {
                if (errCode != string.Empty && errCode != "00000000")
                {
                    //TODO: from here
                    // restore orderAmount with OrderQty
                    // restore buyTimes or sellTimes with Side
                    addLog("InstantAck:" + OrderID + " " + errMsg);
                }
                else
                {
                    addLog("InstantAck:" + OrderID + " " + OrderDate + " " + OrderTime);
                }
            }
        }
        public void OnNewAck(string LineNo, string ClOrdId, string OrgClOrdId,
                             string BranchId, string Account, string OrderDate,
                             string OrderTime, string OrderID, string Symbol,
                             int ExCode, string Side, string OrdType, int PriceFlag,
                             Single Price, int OrderQty, string errCode, string errMsg,
                             [MarshalAs(UnmanagedType.AnsiBStr)] string ExecType,
                             int BeforeQty, int AfterQty, string TimeInForce, string UserData)
        {
            // only handle new orders
            if (ExecType == "O")
            {
                if (errCode != string.Empty && errCode != "00000000")
                {
                    //TODO: from here
                    // restore orderAmount with OrderQty
                    // restore buyTimes or sellTimes with Side
                    addLog("NewAck:" + OrderID + " " + errMsg);
                }
                else
                {
                    addLog("NewAck:" + OrderID + " " + OrderDate + " " + OrderTime);
                }
            }
        }
        public void OnNewMat(string LineNo, string BranchId, string Account,
                             string OrderID, string Symbol, int ExCode,
                             string Side, string OrdType, string MatTime,
                             Single MatPrice, int MatQty, string MatSeq)
        {
            var quantity = MatQty / 1000;
            //TODO: from here
            // update matchAmount with quantity
            addLog("NewMat:" + OrderID + " " + MatPrice.ToString() + " " + quantity.ToString());
        }
        private void OnMsgAlertEvent(int errCode, string errMsg)
        {
            addLog("MsgAlert:" + errCode.ToString() + " " + errMsg);
        }
        private void registerEvents()
        {
            onLogin = new RayinAPI.OnLoginEvent(OnLogin);
            RayinAPI.SetOnLoginEvent(onLogin);

            onOdrConnect = new RayinAPI.OnConnectEvent(OnOdrConnect);
            RayinAPI.SetOnOdrConnectEvent(onOdrConnect);

            onAckMatConnect = new RayinAPI.OnConnectEvent(OnAckMatConnect);
            RayinAPI.SetOnAckMatConnectEvent(onAckMatConnect);

            onQuoteConnect = new RayinAPI.OnConnectEvent(OnQuoteConnect);
            RayinAPI.SetOnQuoteConnectEvent(onQuoteConnect);

            onOdrDisConnect = new RayinAPI.OnDisConnectEvent(OnOdrDisConnect);
            RayinAPI.SetOnOdrDisConnectEvent(onOdrDisConnect);

            onAckMatDisconnect = new RayinAPI.OnDisConnectEvent(OnAckMatDisConnect);
            RayinAPI.SetOnAckMatDisConnectEvent(onAckMatDisconnect);

            onQuoteDisconnect = new RayinAPI.OnDisConnectEvent(OnQuoteDisConnect);
            RayinAPI.SetOnQuoteDisConnectEvent(onQuoteDisconnect);

            onInstantAck = new RayinAPI.OnInstantAckEvent(OnInstantAck);
            RayinAPI.SetOnInstantAckEvent(onInstantAck);

            onNewAck = new RayinAPI.OnNewAckEvent(OnNewAck);
            RayinAPI.SetNewAckEvent(onNewAck);

            onNewMat = new RayinAPI.OnNewMatEvent(OnNewMat);
            RayinAPI.SetNewMatEvent(onNewMat);

            onNewQuote = new RayinAPI.OnNewQuoteEvent(OnNewQuote);
            RayinAPI.SetNewQuoteEvent(onNewQuote);

            onOdrErr = new RayinAPI.OnErrorEvent(OnOdrError);
            RayinAPI.SetOnOdrErrorEvent(onOdrErr);

            onAckMatErr = new RayinAPI.OnErrorEvent(OnAckMatError);
            RayinAPI.SetOnAckMatErrorEvent(onAckMatErr);

            onMsgAlert = new RayinAPI.OnMsgAlertEvent(OnMsgAlertEvent);
            RayinAPI.SetMsgAlertEvent(onMsgAlert);
        }
        public EventHandler(dynamic config)
        {
            this.config = config;
            this.logs = new List<string>();
            this.reportCount = 0;
            registerEvents();
        }
        public void registerTargets(dynamic rules, ref Dictionary<string, Target> targets)
        {
            // 代號長度
            if (Convert.ToBoolean(rules.Exclude.IDLength.enabled))
            {
                var length = Convert.ToInt32(rules.Exclude.IDLength.length);
                List<string> removals = (from target in targets
                                         where target.Key.Count(c => !Char.IsWhiteSpace(c)) > length
                                         select target.Key).ToList();

                foreach (var removal in removals)
                {
                    targets.Remove(removal);
                }
            }
            // 當沖註記
            if (Convert.ToBoolean(rules.Exclude.NotDayTrade.enabled))
            {
                List<string> removals = (from target in targets
                                         where target.Value.dayTradeMark == ' '
                                         select target.Key).ToList();

                foreach (var removal in removals)
                {
                    targets.Remove(removal);
                }
            }
            // 全額交割 (透過變更交易)
            if (Convert.ToBoolean(rules.Exclude.FullCash.enabled))
            {
                List<string> removals = (from target in targets
                                         where target.Value.dealType != '0'
                                         select target.Key).ToList();

                foreach (var removal in removals)
                {
                    targets.Remove(removal);
                }
            }
            // 處置註記
            if (Convert.ToBoolean(rules.Exclude.Disposed.enabled))
            {
                List<string> removals = (from target in targets
                                         where target.Value.disposeMark != '0'
                                         select target.Key).ToList();

                foreach (var removal in removals)
                {
                    targets.Remove(removal);
                }
            }
            // // 交易量
            // if (Convert.ToBoolean(rules.Exclude.TradeAmount.enabled))
            // {
            //     var expect = Convert.ToInt32(rules.Exclude.TradeAmount.total);
            //     List<string> removals = (from target in targets
            //                              where target.Value.totalAmount < expect
            //                              select target.Key).ToList();

            //     foreach (var removal in removals)
            //     {
            //         targets.Remove(removal);
            //     }
            // }
            // // 收盤價
            // if (Convert.ToBoolean(rules.Exclude.TradePrice.enabled))
            // {
            //     var low = Convert.ToSingle(rules.Exclude.TradePrice.price.min);
            //     var high = Convert.ToSingle(rules.Exclude.TradePrice.price.max);
            //     List<string> removals = (from target in targets
            //                              where target.Value.ldcPrice < low || 
            //                                    target.Value.ldcPrice > high
            //                              select target.Key).ToList();

            //     foreach (var removal in removals)
            //     {
            //         targets.Remove(removal);
            //     }
            // }

            foreach (var target in targets)
            {
                var errCode = Marshal.StringToHGlobalAnsi("");
                var errMsg = Marshal.StringToHGlobalAnsi("");
                RayinAPI.AddQuote(1, target.Key, ref errCode, ref errMsg);
                var code = Marshal.PtrToStringAnsi(errCode);
                var msg = Marshal.PtrToStringAnsi(errMsg);
                if (code == "00" || code == " ")
                    Console.WriteLine("訂閱行情成功: " + target.Key);
                else Console.WriteLine("訂閱行情失敗: " + target.Key + " " + code + " " + msg);
            }
        }
        public bool login()
        {
            if (this.config == null) return false;
            isLogined = false;

            var start = DateTime.ParseExact(Convert.ToString(this.config.time.start), "HH:mm:ss", null);
            while (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) < 0)
                Thread.Sleep(100);

            RayinAPI.SetDebugMode(Convert.ToBoolean(this.config.debug));
            RayinAPI.SetRecvTimeout(Convert.ToInt32(this.config.timeout));
            RayinAPI.SetServer(
                    Convert.ToString(this.config.host), Convert.ToInt32(this.config.port));
            return RayinAPI.Login(
                    Convert.ToString(this.config.account), Convert.ToString(this.config.password));
        }
        public bool shouldLogout()
        {

            if (this.config == null) return false;
            var end = DateTime.ParseExact(Convert.ToString(this.config.time.end), "HH:mm:ss", null);
            if (DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0) return false;
            else return true;
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