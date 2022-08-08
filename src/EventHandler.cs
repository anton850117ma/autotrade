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
        DataHandler dataHandler;
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

        public void createBuyOrder(dynamic rule, string symbol, ref Target target)
        {
            if (this.dataHandler.config == null) return;
            var subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            var account = Convert.ToString(this.dataHandler.config.Login.account);
            var timeinforce = Convert.ToString(rule.timeinforce);
            int quantity = Convert.ToInt32(rule.cost) / (target.bullPrice * 1000);

            // TODO: deal with price flag

            var clOrdId = new IntPtr();
            var errCode = new IntPtr();
            var errMsg = new IntPtr();
            RayinAPI.NewGetClOrdId(ref clOrdId);
            RayinAPI.NewOrder(subcomp, account, symbol, 1, "B", "0", 0, 4, quantity,
                              timeinforce, "", ref clOrdId, ref errCode, ref errMsg);

            var retVal = Marshal.PtrToStringAnsi(errCode);
            if (retVal != null && (retVal.Length == 0 || retVal == "00"))
            {
                target.stockData.orderAmount += quantity;
                target.stockData.buyTimes += 1;
            }
            var id = Marshal.PtrToStringAnsi(clOrdId);
            if (id != null)
                addLog("新單: " + id + " Buy " + symbol + " " + quantity.ToString());
        }
        public void ruleBuyNowPrice(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return;

            if (Convert.ToBoolean(rule.enabled))
            {
                var start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                var end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    var stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount == target.stockData.orderAmount)
                    {
                        var close = target.ldcPrice * Convert.ToSingle(rule.price.close.factor);
                        var now = target.nowPrice;
                        var compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">=":
                                if (now >= close)
                                    this.createBuyOrder(rule.order, values[1], ref target);
                                break;
                            case ">":
                                if (now > close)
                                    this.createBuyOrder(rule.order, values[1], ref target);
                                break;
                            case "=":
                                if (now == close)
                                    this.createBuyOrder(rule.order, values[1], ref target);
                                break;
                            case "<=":
                                if (now <= close)
                                    this.createBuyOrder(rule.order, values[1], ref target);
                                break;
                            case "<":
                                if (now < close)
                                    this.createBuyOrder(rule.order, values[1], ref target);
                                break;
                        }
                    }
                }
            }
        }
        public void createSellOrder(dynamic rule, string symbol, ref Target target)
        {
            if (this.dataHandler.config == null) return;
            var subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            var account = Convert.ToString(this.dataHandler.config.Login.account);
            var timeinforce = Convert.ToString(rule.timeinforce);

            var quantity = target.stockData.orderAmount;
            var cost = Convert.ToInt32(rule.cost);
            if (cost != -1)
                quantity = cost / (target.bullPrice * 1000);

            // TODO: deal with price flag

            var clOrdId = new IntPtr();
            var errCode = new IntPtr();
            var errMsg = new IntPtr();
            RayinAPI.NewGetClOrdId(ref clOrdId);
            RayinAPI.NewOrder(subcomp, account, symbol, 1, "S", "0", 0, 4, quantity,
                              timeinforce, "", ref clOrdId, ref errCode, ref errMsg);

            var retVal = Marshal.PtrToStringAnsi(errCode);
            if (retVal != null && (retVal.Length == 0 || retVal == "00"))
            {
                target.stockData.orderAmount -= quantity;
                target.stockData.sellTimes += 1;
            }
            var id = Marshal.PtrToStringAnsi(clOrdId);
            if (id != null)
                addLog("新單: " + id + " Sell " + symbol + " " + quantity.ToString());
        }
        public void ruleSellNowPrice1(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return;

            if (Convert.ToBoolean(rule.enabled))
            {
                var start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                var end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    var stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount <= target.stockData.orderAmount)
                    {
                        var max = target.maxPrice * Convert.ToSingle(rule.price.max.factor);
                        var now = target.nowPrice;
                        var compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">=":
                                if (now >= max)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case ">":
                                if (now > max)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "=":
                                if (now == max)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "<=":
                                if (now <= max)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "<":
                                if (now < max)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                        }
                    }
                }
            }
        }
        public void ruleSellNowPrice2(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return;

            if (Convert.ToBoolean(rule.enabled))
            {
                var start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                var end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    var stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount <= target.stockData.orderAmount)
                    {
                        var bull = target.bullPrice * Convert.ToSingle(rule.price.bull.factor);
                        var now = target.nowPrice;
                        var compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">=":
                                if (now >= bull)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case ">":
                                if (now > bull)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "=":
                                if (now == bull)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "<=":
                                if (now <= bull)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                            case "<":
                                if (now < bull)
                                    this.createSellOrder(rule.order, values[1], ref target);
                                break;
                        }
                    }
                }
            }
        }
        public bool updateTarget(string[] values)
        {
            if (this.dataHandler.targetMap == null) return false;
            if (!this.dataHandler.targetMap.ContainsKey(values[1]))
            {
                addLog("Quote: " + values[1] + " not found!");
                return false;
            }

            this.dataHandler.targetMap[values[1]].updateFromQuoteEx(
                values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[11]);
            return true;
        }
        public void addLog(string message)
        {
            Console.WriteLine(message + "\n");
            // lock (logs)
            // {
            //     logs.Add(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
            // }
        }
        public void OnLogin(string status)
        {
            addLog("onlogin");
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
        public void OnOdrConnect()
        {
            this.isOrderConnected = true;
            addLog("下單線路已連線");
        }
        public void OnOdrDisConnect()
        {
            this.isOrderConnected = false;
            addLog("下單線路已斷線");
        }
        public void OnOdrError(int errCode, string errMsg)
        {
            addLog("下單線路異常: " + errCode.ToString() + " " + errMsg);
        }
        public void OnAckMatConnect()
        {
            this.isAckMatConnected = true;
            RayinAPI.Recover(this.reportCount.ToString());
            addLog("主動回報線路已連線");
        }
        public void OnAckMatDisConnect()
        {
            this.isAckMatConnected = false;
            addLog("主動回報線路已斷線");
        }
        public void OnAckMatError(int errCode, string errMsg)
        {
            addLog("主動回報線路異常: " + errCode.ToString() + " " + errMsg);
        }
        public void OnQuoteConnect()
        {
            this.isQuoteConnected = true;
            addLog("行情線路已連線");
            this.registerTargets();
        }
        public void OnQuoteDisConnect()
        {
            this.isQuoteConnected = false;
            addLog("行情線路已斷線");
        }
        public void OnNewQuote(string NewQuote)
        {
            if (this.dataHandler.config == null) return;
            var rules = this.dataHandler.config.Rules;

            string[] values = Regex.Split(NewQuote, @"\|\|", RegexOptions.IgnoreCase);

            addLog("新行情: " + values[1] + " " + values[3] + " " + values[9] + " " + values[11] + "\n");
            if (!this.updateTarget(values)) return;

            var buyRule = rules.Buy.NowPrice;
            this.ruleBuyNowPrice(buyRule, values);

            var sellRule1 = rules.Sell.NowPrice1;
            this.ruleSellNowPrice1(sellRule1, values);

            var sellRule2 = rules.Sell.NowPrice2;
            this.ruleSellNowPrice2(sellRule2, values);

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
                    if (this.dataHandler.targetMap == null) return;

                    if (Side == "B")
                    {
                        this.dataHandler.targetMap[Symbol].stockData.orderAmount -= OrderQty;
                        this.dataHandler.targetMap[Symbol].stockData.buyTimes -= 1;
                    }
                    else
                    {
                        this.dataHandler.targetMap[Symbol].stockData.orderAmount += OrderQty;
                        this.dataHandler.targetMap[Symbol].stockData.sellTimes -= 1;
                    }
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
                    if (this.dataHandler.targetMap == null) return;

                    if (Side == "B")
                    {
                        this.dataHandler.targetMap[Symbol].stockData.orderAmount -= OrderQty;
                        this.dataHandler.targetMap[Symbol].stockData.buyTimes -= 1;
                    }
                    else
                    {
                        this.dataHandler.targetMap[Symbol].stockData.orderAmount += OrderQty;
                        this.dataHandler.targetMap[Symbol].stockData.sellTimes -= 1;
                    }
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
            if (this.dataHandler.targetMap == null) return;
            var quantity = MatQty / 1000;

            if (Side == "B")
            {
                this.dataHandler.targetMap[Symbol].stockData.matchAmount += quantity;
            }
            else
            {
                this.dataHandler.targetMap[Symbol].stockData.matchAmount -= quantity;
            }
            addLog("NewMat:" + OrderID + " " + MatPrice.ToString() + " " + quantity.ToString());
        }
        public void OnMsgAlertEvent(int errCode, string errMsg)
        {
            addLog("MsgAlert:" + errCode.ToString() + " " + errMsg);
        }
        public void registerEvents()
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
        public EventHandler(DataHandler handler)
        {
            this.dataHandler = handler;
            this.logs = new List<string>();
            this.reportCount = 0;
            registerEvents();
        }
        public void registerTargets()
        {
            if (this.dataHandler.config == null) return;
            if (this.dataHandler.targetMap == null) return;
            var rules = this.dataHandler.config.Rules;
            var targets = this.dataHandler.targetMap;

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
            var config = this.dataHandler.config;
            if (config == null) return false;
            isLogined = false;

            var start = DateTime.ParseExact(Convert.ToString(config.Login.time.start), "HH:mm:ss", null);
            while (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) < 0) 
            {
                Console.Write("Waiting for login time...\n");
                Thread.Sleep(1000);
            }

            RayinAPI.SetDebugMode(Convert.ToBoolean(config.Login.debug));
            RayinAPI.SetRecvTimeout(Convert.ToInt32(config.Login.timeout));
            RayinAPI.SetServer(
                    Convert.ToString(config.Login.host), Convert.ToInt32(config.Login.port));
            return RayinAPI.Login(
                    Convert.ToString(config.Login.username), Convert.ToString(config.Login.password));
        }
        public bool shouldLogout()
        {
            var config = this.dataHandler.config;
            if (config == null) return false;
            var end = DateTime.ParseExact(Convert.ToString(config.Login.time.end), "HH:mm:ss", null);
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
        public void storeRecords(){
            this.dataHandler.storeRecords();
        }
    }
}