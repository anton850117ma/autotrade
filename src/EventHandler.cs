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

        public bool checkConditions(dynamic rule, string symbol)
        {
            if (this.dataHandler.targetMap == null) return false;
            if (!this.dataHandler.targetMap.ContainsKey(symbol)) return false;

            var target = this.dataHandler.targetMap[symbol];

            // 代號長度
            if (Convert.ToBoolean(rule.IDLength.enabled))
            {
                int length = Convert.ToInt32(rule.IDLength.length);
                if (symbol.Length > length) return false;
            }

            // 當沖註記
            if (Convert.ToBoolean(rule.NotDayTrade.enabled))
            {
                if (target.dayTradeMark == ' ') return false;
            }

            // 全額交割 (透過變更交易)
            if (Convert.ToBoolean(rule.FullCash.enabled))
            {
                if (target.dealType != '0') return false;
            }
            // 處置註記
            if (Convert.ToBoolean(rule.Disposed.enabled))
            {
                if (target.dealType != '0') return false;
            }

            // 資本額
            if (Convert.ToBoolean(rule.Capital.enabled))
            {
                string amount = Convert.ToString(rule.Capital.amount);
                if (target.capital.CompareTo(amount) >= 0) return false;
            }

            // 交易量
            if (Convert.ToBoolean(rule.TradeAmount.enabled))
            {
                int expect = Convert.ToInt32(rule.TradeAmount.total);
                if (target.totalAmount < expect) return false;
            }

            // 收盤價
            if (Convert.ToBoolean(rule.TradePrice.enabled))
            {
                Single low = Convert.ToSingle(rule.TradePrice.price.min);
                Single high = Convert.ToSingle(rule.TradePrice.price.max);
                if (target.ldcPrice < low || target.ldcPrice > high) return false;
            }

            return true;
        }
        public void createBuyOrder(dynamic rule, string symbol, ref Target target)
        {
            if (this.dataHandler.config == null) return;
            string subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            string account = Convert.ToString(this.dataHandler.config.Login.account);
            string timeinforce = Convert.ToString(rule.timeinforce);
            int quantity = Convert.ToInt32(rule.cost) / (target.bullPrice * 1000);

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
                target.stockData.sellTimes += 1;
                Utility.addLogInfo(this.dataHandler.logger,
                                   "新單買入成功 " + symbol + " " + quantity.ToString());
            }
            else
            {
                Utility.addLogError(this.dataHandler.logger,
                                    "新單買入失敗 " + symbol + " " + quantity.ToString());
            }
        }
        public void ruleBuyNowPrice(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return;

            if (Convert.ToBoolean(rule.enabled))
            {
                DateTime start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount == target.stockData.orderAmount)
                    {
                        Single close = target.ldcPrice * Convert.ToSingle(rule.price.close.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
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
            string subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            string account = Convert.ToString(this.dataHandler.config.Login.account);
            string timeinforce = Convert.ToString(rule.timeinforce);

            var quantity = target.stockData.orderAmount;
            var cost = Convert.ToInt32(rule.cost);
            if (cost != -1)
                quantity = cost / (target.bullPrice * 1000);

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
                Utility.addLogInfo(this.dataHandler.logger,
                                   "新單賣出成功 " + symbol + " " + quantity.ToString());
            }
            else
            {
                Utility.addLogError(this.dataHandler.logger,
                                    "新單賣出失敗 " + symbol + " " + quantity.ToString());
            }
        }
        public void ruleSellNowPrice1(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return;

            if (Convert.ToBoolean(rule.enabled))
            {
                DateTime start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount <= target.stockData.orderAmount)
                    {
                        Single max = target.maxPrice * Convert.ToSingle(rule.price.max.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
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
                DateTime start = DateTime.ParseExact(
                                Convert.ToString(rule.time.start), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int stockAmount = Convert.ToInt32(rule.stock.amount);
                    var target = this.dataHandler.targetMap[values[1]];
                    if (stockAmount <= target.stockData.orderAmount)
                    {
                        Single bull = target.bullPrice * Convert.ToSingle(rule.price.bull.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
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
                Utility.addLogWarning(this.dataHandler.logger,
                                      "股票 " + values[1] + " 不在標的表中");
                return false;
            }

            this.dataHandler.targetMap[values[1]].updateFromQuoteEx(
                values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[11]);
            return true;
        }
        public void OnLogin(string status)
        {
            Console.WriteLine("recv login response");

            if (status.IndexOf("<ret ") >= 0 &&
                Utility.getXMLValue(status, "status") != "OK")
            {
                Utility.addLogCrtical(this.dataHandler.logger,
                                      "登入失敗 " + Utility.getXMLValue(status, "msg"));
                return;
            }

            this.isLogined = true;
            Utility.addLogInfo(this.dataHandler.logger, "登入成功");

            if (RayinAPI.ConnectToOrderServer())
                Utility.addLogInfo(this.dataHandler.logger, "請求連線下單線路成功");
            else
            {
                Utility.addLogCrtical(this.dataHandler.logger, "請求連線下單線路失敗");
                return;
            }

            if (RayinAPI.ConnectToAckMatServer())
                Utility.addLogInfo(this.dataHandler.logger, "請求連線回報線路成功");
            else
            {
                Utility.addLogCrtical(this.dataHandler.logger, "請求連線回報線路失敗");
                return;
            }

            if (RayinAPI.ConnectToQuoteServer())
                Utility.addLogInfo(this.dataHandler.logger, "請求連線行情線路成功");
            else
            {
                Utility.addLogCrtical(this.dataHandler.logger, "請求連線行情線路失敗");
                return;
            }
        }
        public void OnOdrConnect()
        {
            this.isOrderConnected = true;
            Utility.addLogInfo(this.dataHandler.logger, "下單線路已連線");
        }
        public void OnOdrDisConnect()
        {
            this.isOrderConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, "下單線路意外斷線");
            else Utility.addLogInfo(this.dataHandler.logger, "下單線路已斷線");
        }
        public void OnOdrError(int errCode, string errMsg)
        {
            Utility.addLogCrtical(this.dataHandler.logger,
                                  "下單線路異常 " + errMsg);
        }
        public void OnAckMatConnect()
        {
            this.isAckMatConnected = true;
            //TODO: handle reportCount
            // RayinAPI.Recover(this.reportCount.ToString());
            RayinAPI.Recover("EXCEL");
            Utility.addLogInfo(this.dataHandler.logger, "委託回報線路已連線");
        }
        public void OnAckMatDisConnect()
        {
            this.isAckMatConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, "主動回報線路意外斷線");
            else Utility.addLogInfo(this.dataHandler.logger, "委託回報線路已斷線");
        }
        public void OnAckMatError(int errCode, string errMsg)
        {
            Utility.addLogCrtical(this.dataHandler.logger,
                                  "委託回報線路異常 " + errMsg);
        }
        public void OnQuoteConnect()
        {
            this.isQuoteConnected = true;
            Utility.addLogInfo(this.dataHandler.logger, "行情線路已連線");
            this.registerTargets();
        }
        public void OnQuoteDisConnect()
        {
            this.isQuoteConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, "行情線路意外斷線");
            else Utility.addLogInfo(this.dataHandler.logger, "行情線路已斷線");
        }
        public void OnNewQuote(string NewQuote)
        {
            if (this.dataHandler.config == null) return;
            var rules = this.dataHandler.config.Rules;

            string[] values = Regex.Split(NewQuote, @"\|\|", RegexOptions.IgnoreCase);

            if (!this.updateTarget(values)) return;
            if (!this.isOrderConnected) return;
            if (!this.checkConditions(rules.Exclude, values[1])) return;

            var buyRule = rules.Buy.NowPrice;
            this.ruleBuyNowPrice(buyRule, values);

            var sellRule1 = rules.Sell.NowPrice1;
            this.ruleSellNowPrice1(sellRule1, values);

            var sellRule2 = rules.Sell.NowPrice2;
            this.ruleSellNowPrice2(sellRule2, values);

            Utility.addLogInfo(this.dataHandler.logger,
                               "新行情 " + values[1] + " " + values[3] + " " +
                               values[9] + " " + values[11]);
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
                    Utility.addLogError(this.dataHandler.logger,
                                       "及時回報失敗 :" + OrderID + " " + errMsg);
                }
                else
                {
                    Utility.addLogInfo(this.dataHandler.logger,
                                       "及時回報成功 " + OrderID + " " + OrderDate + " " + OrderTime);
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
                    Utility.addLogError(this.dataHandler.logger,
                                        "委託回報錯誤 " + OrderID + " " + errMsg);
                }
                else
                {
                    Utility.addLogInfo(this.dataHandler.logger,
                                       "委託回報成功 " + OrderID + " " + OrderDate + " " + OrderTime);
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
            Utility.addLogInfo(this.dataHandler.logger,
                               "成交回報 " + OrderID + " " + MatPrice.ToString() + " " + quantity.ToString());
        }
        public void OnMsgAlertEvent(int errCode, string errMsg)
        {
            Utility.addLogWarning(this.dataHandler.logger, "訊息提醒 " + errMsg);
        }
        public void registerEvents()
        {
            Utility.addLogDebug(this.dataHandler.logger, "開始註冊事件...");

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

            Utility.addLogDebug(this.dataHandler.logger, "完成註冊事件...");
        }
        public EventHandler(DataHandler handler)
        {
            this.dataHandler = handler;
            this.logs = new List<string>();
            this.reportCount = 0;
            this.isLogined = false;
            this.isOrderConnected = false;
            this.isQuoteConnected = false;
            registerEvents();
        }
        public void registerTargets()
        {
            if (this.dataHandler.config == null) return;
            if (this.dataHandler.targetMap == null) return;
            var targets = this.dataHandler.targetMap;

            int success = 0, failed = 0;

            Utility.addLogInfo(this.dataHandler.logger, "開始訂閱行情...");

            foreach (var target in targets)
            {
                var errCode = Marshal.StringToHGlobalAnsi("");
                var errMsg = Marshal.StringToHGlobalAnsi("");
                RayinAPI.AddQuote(1, target.Key, ref errCode, ref errMsg);
                var code = Marshal.PtrToStringAnsi(errCode);
                var msg = Marshal.PtrToStringAnsi(errMsg);
                if (code == "00" || code == " ")
                {
                    success++;
                    Utility.addLogDebug(this.dataHandler.logger, "成功訂閱 " + target.Key);
                }
                else
                {
                    failed++;
                    Utility.addLogDebug(this.dataHandler.logger,
                                         "失敗訂閱 " + target.Key + " " + code + " " + msg);
                }

            }
            Utility.addLogInfo(this.dataHandler.logger, "完成行情訂閱" +
                               " 成功: " + success.ToString() +
                               " 失敗: " + failed.ToString());

        }
        public bool login()
        {
            if (this.isLogined) return true;
            var config = this.dataHandler.config;
            if (config == null) return false;

            DateTime start = DateTime.ParseExact(Convert.ToString(config.Login.time.start), "HH:mm:ss", null);
            Utility.addLogInfo(this.dataHandler.logger, "等待登入時刻...");
            while (DateTime.Now.TimeOfDay.CompareTo(start.TimeOfDay) < 0)
            {
                Thread.Sleep(100);
            }

            RayinAPI.SetDebugMode(Convert.ToBoolean(config.Login.debug));
            RayinAPI.SetRecvTimeout(Convert.ToInt32(config.Login.timeout));
            RayinAPI.SetServer(
                    Convert.ToString(config.Login.host), Convert.ToInt32(config.Login.port));

            var result = RayinAPI.Login(
                            Convert.ToString(config.Login.username),
                            Convert.ToString(config.Login.password));
            if (result)
            {
                Utility.addLogInfo(this.dataHandler.logger, "發出登入請求成功");
            }
            else
            {
                Utility.addLogInfo(this.dataHandler.logger, "發出登入請求失敗");
            }
            return result;
        }
        public bool shouldLogout()
        {
            var config = this.dataHandler.config;
            if (config == null) return false;
            DateTime end = DateTime.ParseExact(Convert.ToString(config.Login.time.end), "HH:mm:ss", null);
            if (DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0) return false;
            else return true;
        }
        public bool logout()
        {
            var result = RayinAPI.Logout();
            if (result)
            {
                this.isLogined = false;
                Utility.addLogInfo(this.dataHandler.logger, "請求登出成功");
            }
            else
            {
                Utility.addLogInfo(this.dataHandler.logger, "請求登出失敗");
            }
            return result;
        }
        public void storeRecords()
        {
            Utility.addLogInfo(this.dataHandler.logger, "等待完全登出...");
            while (this.isLogined || this.isQuoteConnected ||
                   this.isOrderConnected || this.isAckMatConnected)
            {
                Thread.Sleep(1000);
            }
            this.dataHandler.storeRecords();
        }
    }
}