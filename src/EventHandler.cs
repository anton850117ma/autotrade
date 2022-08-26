using System;
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
        public bool isLogouted;
    }
    public class EventHandler : Status
    {
        DataHandler dataHandler;
        int newAckCount, newMatCount;
        RayinAPI.OnLoginEvent onLogin;
        RayinAPI.OnConnectEvent onOdrConnect;
        RayinAPI.OnConnectEvent onAckMatConnect;
        RayinAPI.OnConnectEvent onQuoteConnect;
        RayinAPI.OnDisConnectEvent onOdrDisConnect;
        RayinAPI.OnDisConnectEvent onAckMatDisconnect;
        RayinAPI.OnDisConnectEvent onQuoteDisconnect;
        RayinAPI.OnInstantAckEvent onInstantAck;
        RayinAPI.OnNewAckEvent onNewAck;
        RayinAPI.OnNewMatEvent onNewMat;
        RayinAPI.OnNewQuoteEvent onNewQuote;
        RayinAPI.OnErrorEvent onOdrErr;
        RayinAPI.OnErrorEvent onAckMatErr;
        RayinAPI.OnMsgAlertEvent onMsgAlert;

        public bool checkConditions(dynamic rule, string symbol)
        {
            if (this.dataHandler.targetMap == null) return false;
            if (!this.dataHandler.targetMap.ContainsKey(symbol)) return false;

            var target = this.dataHandler.targetMap[symbol];

            // 代號長度
            if (Convert.ToBoolean(rule.IDLength.enabled))
            {
                int length = Convert.ToInt32(rule.IDLength.length);
                if (symbol.Count(c => !char.IsWhiteSpace(c)) > length) return false;
                bool fund = Convert.ToBoolean(rule.IDLength.fund);
                if (fund && symbol.CompareTo(Utility.DEF_FUND_CODE) <= 0) return false;
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
                if (Utility.compareNumbers(target.capital, amount) >= 0) return false;
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
                float low = Convert.ToSingle(rule.TradePrice.price.min);
                float high = Convert.ToSingle(rule.TradePrice.price.max);
                if (target.ldcPrice < low || target.ldcPrice > high) return false;
            }

            return true;
        }
        public bool createBuyOrder(dynamic rule, string symbol, Target target, string tag)
        {
            if (this.dataHandler.config == null) return false;
            string subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            string account = Convert.ToString(this.dataHandler.config.Login.account);
            string timeinforce = Convert.ToString(rule.timeinforce);
            int quantity = Convert.ToInt32(Convert.ToInt32(rule.cost) / (target.bullPrice * 1000));

            var clOrdId = new IntPtr();
            var errCode = new IntPtr();
            var errMsg = new IntPtr();
            RayinAPI.NewGetClOrdId(ref clOrdId);
            RayinAPI.NewOrder(subcomp, account, symbol, 1, "B", "0", 0, 4, quantity,
                              timeinforce, "", ref clOrdId, ref errCode, ref errMsg);

            var retVal = Marshal.PtrToStringAnsi(errCode);
            if (retVal != null && (retVal.Length == 0 || retVal == "00"))
            {
                //target.stockData.orderAmount += quantity;
                Interlocked.Increment(ref target.stockData.buyTimes);
                Utility.addLogInfo(this.dataHandler.logger, 
                    string.Format("{0} | 條件:{1,-10} 序號:{2,-9} 股票:{3,-6} 數量:{4,-3}", 
                                  "新單買進成功", tag, clOrdId, symbol, quantity.ToString()));
                return true;
            }
            else
            {
                Utility.addLogWarning(this.dataHandler.logger,
                     string.Format("{0} | 條件:{1,-10} 序號:{2,-12} 股票:{3,-6} 數量:{4,-3} 原因:{5}",
                                   "新單買進失敗", tag, clOrdId, symbol, quantity.ToString(), errMsg));
                return false;
            }
        }
        public bool ruleBuyNowPrice(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return false;

            if (Convert.ToBoolean(rule.enabled))
            {
                DateTime begin = DateTime.ParseExact(
                                Convert.ToString(rule.time.begin), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(begin.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int times = Convert.ToInt32(rule.stock.times);
                    var symbol = Utility.addPading(values[1]);
                    var target = this.dataHandler.targetMap[symbol];

                    if (times > Interlocked.CompareExchange(ref target.stockData.buyTimes, 0, 0))
                    {
                        float close = target.ldcPrice * Convert.ToSingle(rule.price.close.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">":
                                if (now > close)
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                            case ">=":
                                if (now > close || Utility.nearlyEqual(now, close))
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                            case "==":
                                if (Utility.nearlyEqual(now, close))
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                            case "<=":
                                if (now < close || Utility.nearlyEqual(now, close))
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                            case "<":
                                if (now < close)
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                            case "!=":
                                if (!Utility.nearlyEqual(now, close))
                                    return this.createBuyOrder(rule.order, symbol, target, "NowPrice");
                                break;
                        }
                    }
                }
            }
            return false;
        }
        public bool createSellOrder(dynamic rule, string symbol, Target target, string tag)
        {
            if (this.dataHandler.config == null) return false;
            string subcomp = Convert.ToString(this.dataHandler.config.Login.subcomp);
            string account = Convert.ToString(this.dataHandler.config.Login.account);
            string timeinforce = Convert.ToString(rule.timeinforce);

            var quantity = target.stockData.matchAmount;
            int cost = Convert.ToInt32(rule.cost);
            if (cost >= 0)
                quantity = Convert.ToInt32(cost / (target.bullPrice * 1000));

            var clOrdId = new IntPtr();
            var errCode = new IntPtr();
            var errMsg = new IntPtr();
            RayinAPI.NewGetClOrdId(ref clOrdId);
            RayinAPI.NewOrder(subcomp, account, symbol, 1, "S", "0", 0, 4, quantity,
                              timeinforce, "", ref clOrdId, ref errCode, ref errMsg);

            var retVal = Marshal.PtrToStringAnsi(errCode);
            if (retVal != null && (retVal.Length == 0 || retVal == "00"))
            {
                Interlocked.Add(ref target.stockData.matchAmount, -quantity);
                //Interlocked.Increment(ref target.stockData.sellTimes);
                Utility.addLogInfo(this.dataHandler.logger,
                    string.Format("{0} | 條件:{1,-10} 序號:{2,-12} 股票:{3,-6} 數量:{4,-3}",
                                  "新單賣出成功", tag, clOrdId, symbol, quantity.ToString()));
                return true;
            }
            else
            {
                Utility.addLogWarning(this.dataHandler.logger,
                    string.Format("{0} | 條件:{1,-10} 序號:{2,-12} 股票:{3,-6} 數量:{4,-3} 原因:{5}",
                                  "新單賣出失敗", tag, clOrdId, symbol, quantity.ToString(), errMsg));
                return false;
            }
        }
        public bool ruleSellNowPrice1(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return false;

            if (Convert.ToBoolean(rule.enabled))
            {
                DateTime begin = DateTime.ParseExact(
                                Convert.ToString(rule.time.begin), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(begin.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int stockAmount = Convert.ToInt32(rule.stock.amount);
                    var symbol = Utility.addPading(values[1]);
                    var target = this.dataHandler.targetMap[symbol];
                    if (stockAmount <= Interlocked.CompareExchange(ref target.stockData.matchAmount, 0, 0))
                    {
                        float max = target.maxPrice * Convert.ToSingle(rule.price.max.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">":
                                if (now > max)
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                            case ">=":
                                if (now > max || Utility.nearlyEqual(now, max))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                            case "==":
                                if (Utility.nearlyEqual(now, max))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                            case "<=":
                                if (now < max || Utility.nearlyEqual(now, max))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                            case "<":
                                if (now < max)
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                            case "!=":
                                if (!Utility.nearlyEqual(now, max))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice1");
                                break;
                        }
                    }
                }
            }
            return false;
        }
        public bool ruleSellNowPrice2(dynamic rule, string[] values)
        {
            if (this.dataHandler.targetMap == null) return false;

            if (Convert.ToBoolean(rule.enabled))
            {
                DateTime begin = DateTime.ParseExact(
                                Convert.ToString(rule.time.begin), "HH:mm:ss", null);
                DateTime end = DateTime.ParseExact(
                                Convert.ToString(rule.time.end), "HH:mm:ss", null);
                if (DateTime.Now.TimeOfDay.CompareTo(begin.TimeOfDay) >= 0 &&
                    DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0)
                {
                    int stockAmount = Convert.ToInt32(rule.stock.amount);
                    var symbol = Utility.addPading(values[1]);
                    var target = this.dataHandler.targetMap[symbol];
                    if (stockAmount <= Interlocked.CompareExchange(ref target.stockData.matchAmount, 0, 0))
                    {
                        float bull = target.bullPrice * Convert.ToSingle(rule.price.bull.factor);
                        var now = target.nowPrice;
                        string compare = Convert.ToString(rule.price.now.compare);
                        switch (compare)
                        {
                            case ">":
                                if (now > bull)
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                            case ">=":
                                if (now > bull || Utility.nearlyEqual(now, bull))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                            case "==":
                                if (Utility.nearlyEqual(now, bull))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                            case "<=":
                                if (now < bull || Utility.nearlyEqual(now, bull))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                            case "<":
                                if (now < bull)
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                            case "!=":
                                if (!Utility.nearlyEqual(now, bull))
                                    return this.createSellOrder(rule.order, symbol, target, "NowPrice2");
                                break;
                        }
                    }
                }
            }
            return false;
        }
        public bool updateTarget(string[] values)
        {
            if (this.dataHandler.targetMap == null) return false;
            var symbol = Utility.addPading(values[1]);
            if (!this.dataHandler.targetMap.ContainsKey(symbol))
            {
                Utility.addLogWarning(this.dataHandler.logger, 
                    string.Format("{0} | 報價股票:{1,-6}", "更新標的錯誤", symbol));
                return false;
            }

            this.dataHandler.targetMap[symbol].updateFromQuoteEx(
                values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[11]);
            return true;
        }
        public void OnLogin(string status)
        {
            // Console.WriteLine("recv login response");

            if (status.IndexOf("<ret ") >= 0 &&
                Utility.getXMLValue(status, "status") != "OK")
            {
                Utility.addLogCrtical(this.dataHandler.logger,
                                      string.Format("{0} | 原因:{1}", "登入失敗", Utility.getXMLValue(status, "msg")));
                return;
            }

            this.isLogined = true;
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "登入成功"));

            if (RayinAPI.ConnectToOrderServer())
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "請求連線下單線路成功"));
            else
            {
                Utility.addLogCrtical(this.dataHandler.logger, string.Format("{0}", "請求連線下單線路失敗"));
                return;
            }

            if (RayinAPI.ConnectToAckMatServer())
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "請求連線回報線路成功"));
            else
            {
                Utility.addLogCrtical(this.dataHandler.logger, string.Format("{0}", "請求連線回報線路失敗"));
                return;
            }

            RayinAPI.ConnectToQuoteServer();
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "請求連線報價線路成功"));
            /*else
            {
                Utility.addLogCrtical(this.dataHandler.logger, "請求連線報價線路失敗");
                return;
            }*/
        }
        public void OnOdrConnect()
        {
            this.isOrderConnected = true;
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "下單線路已連線"));
        }
        public void OnOdrDisConnect()
        {
            this.isOrderConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, string.Format("{0}", "下單線路非正常斷線"));
            else 
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "下單線路已斷線"));
        }
        public void OnOdrError(int errCode, string errMsg)
        {
            Utility.addLogError(this.dataHandler.logger, 
                string.Format("{0} | 原因:{1}", "下單線路錯誤", errMsg));
        }
        public void OnAckMatConnect()
        {
            this.isAckMatConnected = true;
            //RayinAPI.Recover((this.newAckCount + this.newMatCount).ToString());
            RayinAPI.Recover("EXCEL");
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "回報線路已連線"));
        }
        public void OnAckMatDisConnect()
        {
            this.isAckMatConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, string.Format("{0}", "回報線路非正常斷線"));
            else 
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "回報線路已斷線"));
        }
        public void OnAckMatError(int errCode, string errMsg)
        {
            Utility.addLogError(this.dataHandler.logger, 
                string.Format("{0} | 原因:{1}", "回報線路錯誤", errMsg));
        }
        public void OnQuoteConnect()
        {
            this.isQuoteConnected = true;
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "報價線路已連線"));
            this.registerTargets();
        }
        public void OnQuoteDisConnect()
        {
            this.isQuoteConnected = false;
            if (this.isLogined)
                Utility.addLogCrtical(this.dataHandler.logger, string.Format("{0}", "報價線路非正常斷線"));
            else 
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "報價線路已斷線"));
        }
        public void OnNewQuote(string NewQuote)
        {
            if (this.dataHandler.config == null) return;
            var rules = this.dataHandler.config.Rules;

            string[] values = Regex.Split(NewQuote, @"\|\|", RegexOptions.IgnoreCase);
            var symbol = Utility.addPading(values[1]);

            if (!this.updateTarget(values)) return;
            if (!this.isLogouted)
            {
                if (!this.isOrderConnected) return;

                bool result = false;

                var buyRule = rules.Buy.NowPrice;
                if (!result) result = this.ruleBuyNowPrice(buyRule, values);

                var sellRule1 = rules.Sell.NowPrice1;
                if (!result) result = this.ruleSellNowPrice1(sellRule1, values);

                var sellRule2 = rules.Sell.NowPrice2;
                if (!result) result = this.ruleSellNowPrice2(sellRule2, values);

                if (result)
                    Utility.addLogInfo(this.dataHandler.logger,
                        string.Format("{0} | 股票:{1,-6} 參考價:{2,-6} " +
                                      "漲停價:{3,-7} 最高價:{4,-7} 成交價:{5,-7} 累計成交量:{6,-7} 名稱:{7}",
                                      "新報價      ", symbol, values[3], values[4], values[7], values[9], 
                                      values[11], values[2]));
            } 
        }
        public void OnInstantAck(string ExecType, string ClOrdId, string BranchId,
                                 string Account, string OrderDate, string OrderTime,
                                 string OrderID, string Symbol, int ExCode,
                                 string Side, string OrdType, int PriceFlag, float Price,
                                 int OrderQty, int BeforeQty, int AfterQty,
                                 string TimeInForce, string errCode, string errMsg)
        {
            // only handle new orders
            if (ExecType == "O")
            {
                var symbol = Utility.addPading(Symbol);
                if (errCode != string.Empty && errCode != "000000" && errCode != "00000000")
                {
                    if (this.dataHandler.targetMap == null) return;

                    
                    if (Side == "B")
                    {
                        //this.dataHandler.targetMap[symbol].stockData.orderAmount -= OrderQty;
                        Interlocked.Decrement(ref this.dataHandler.targetMap[symbol].stockData.buyTimes);
                    }
                    else
                    {
                        Interlocked.Add(ref this.dataHandler.targetMap[symbol].stockData.matchAmount, OrderQty);
                        //Interlocked.Decrement(ref this.dataHandler.targetMap[symbol].stockData.sellTimes);
                    }
                    
                    Utility.addLogWarning(this.dataHandler.logger,
                        string.Format("{0} | 編號:{1,-5} 序號:{2,-12} 股票:{3,-6} 原因:{4}", 
                                      "及時回報失敗", OrderID, ClOrdId, symbol, errMsg));
                }
                else
                {
                    Utility.addLogInfo(this.dataHandler.logger,
                        string.Format("{0} | 編號:{1,-5} 序號:{2,-12} 股票:{3,-6} 買賣:{4,-1} 數量:{5}",
                                      "及時回報成功", OrderID, ClOrdId, symbol, Side, OrderQty.ToString()));
                }
            }
        }
        public void OnNewAck(string LineNo, string ClOrdId, string OrgClOrdId,
                             string BranchId, string Account, string OrderDate,
                             string OrderTime, string OrderID, string Symbol,
                             int ExCode, string Side, string OrdType, int PriceFlag,
                             float Price, int OrderQty, string errCode, string errMsg,
                             [MarshalAs(UnmanagedType.AnsiBStr)] string ExecType,
                             int BeforeQty, int AfterQty, string TimeInForce, string UserData)
        {
            // only handle new orders
            this.newAckCount++;
            if (ExecType == "O")
            {
                var symbol = Utility.addPading(Symbol);
                if (errCode != string.Empty && errCode != "000000" && errCode != "00000000")
                {
                    if (this.dataHandler.targetMap == null) return;

                    /*
                    if (Side == "B")
                    {
                        this.dataHandler.targetMap[symbol].stockData.orderAmount -= OrderQty;
                        this.dataHandler.targetMap[symbol].stockData.buyTimes -= 1;
                    }
                    else
                    {
                        this.dataHandler.targetMap[symbol].stockData.orderAmount += OrderQty;
                        this.dataHandler.targetMap[symbol].stockData.sellTimes -= 1;
                    }
                    */ 
                    Utility.addLogWarning(this.dataHandler.logger,
                        string.Format("{0} | 編號:{1,-5} 序號:{2,-12} 股票:{3,-6} 原因:{4}",
                                      "委託回報失敗", OrderID, ClOrdId, symbol, errMsg));
                }
                else
                {
                    Utility.addLogInfo(this.dataHandler.logger,
                        string.Format("{0} | 編號:{1,-5} 序號:{2,-12} 股票:{3,-6} 買賣:{4,-1} 數量:{5,-3}",
                                      "委託回報成功", OrderID, ClOrdId, symbol, Side, OrderQty.ToString()));
                }
            }
        }
        public void OnNewMat(string LineNo, string BranchId, string Account,
                             string OrderID, string Symbol, int ExCode,
                             string Side, string OrdType, string MatTime,
                             float MatPrice, int MatQty, string MatSeq)
        {
            this.newMatCount++;
            if (this.dataHandler.targetMap == null) return;
            var quantity = MatQty / 1000;
            var symbol = Utility.addPading(Symbol);
            int result;

            if (Side == "B")
            {
                result = Interlocked.Add(ref this.dataHandler.targetMap[symbol].stockData.matchAmount, quantity);
            }
            else
            {
                result = Interlocked.Add(ref this.dataHandler.targetMap[symbol].stockData.matchAmount, -quantity);
            }
            Utility.addLogInfo(this.dataHandler.logger,
                string.Format("{0} | 編號:{1,-5} 股票:{2,-6} 買賣:{3,-1} 數量:{4,-3} 價格:{5,-7} 庫存:{6,-3}",
                              "成交回報    ", OrderID, symbol, Side, quantity.ToString(), MatPrice.ToString(), result));
        }
        public void OnMsgAlertEvent(int errCode, string errMsg)
        {
            Utility.addLogWarning(this.dataHandler.logger, string.Format("{0} | 原因:{1}", "訊息提醒    ", errMsg));
        }
        public void registerEvents()
        {
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "開始註冊事件"));

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

            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "完成註冊事件"));
        }
        public EventHandler(DataHandler handler)
        {
            this.dataHandler = handler;
            this.newAckCount = 0;
            this.newMatCount = 0;
            this.isLogined = false;
            this.isOrderConnected = false;
            this.isAckMatConnected = false;
            this.isQuoteConnected = false;
            this.isLogouted = false;
            registerEvents();
        }
        public void registerTargets()
        {
            if (this.dataHandler.config == null) return;
            if (this.dataHandler.targetMap == null) return;
            var targets = this.dataHandler.targetMap;

            int success = 0, failed = 0, skipped = 0;
            var rules = this.dataHandler.config.Rules;

            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "開始訂閱報價"));

            //int counter = 0;

            foreach (var target in targets)
            {

                if (!this.checkConditions(rules.Exclude, target.Key))
                {
                    skipped++;
                    // Utility.addLogInfo(this.dataHandler.logger, string.Format("{0} | 股票:{1,-6}", "訂閱跳過", target.Key));
                    continue;
                }

                //if (++counter <= 1000) continue;

                var errCode = Marshal.StringToHGlobalAnsi("");
                var errMsg = Marshal.StringToHGlobalAnsi("");
                RayinAPI.AddQuote(1, target.Key, ref errCode, ref errMsg);
                var code = Marshal.PtrToStringAnsi(errCode);
                var msg = Marshal.PtrToStringAnsi(errMsg);
                if (code == "00" || code == " ")
                {
                    success++;
                    target.Value.registered = true;
                    Utility.addLogInfo(this.dataHandler.logger, 
                                       string.Format("{0} | 股票:{1,-6}", "訂閱成功    ", target.Key));
                }
                else
                {
                    failed++;
                    Utility.addLogWarning(this.dataHandler.logger,
                                          string.Format("{0} | 股票:{1,-6} 原因:{2}", 
                                          "訂閱失敗    ", target.Key, msg));
                }

            }
            Utility.addLogDebug(this.dataHandler.logger,
                                string.Format("{0} | 成功:{1} 失敗:{2} 跳過:{3}", 
                                "完成報價訂閱", success.ToString(), failed.ToString(), skipped.ToString()));
        }
        public void updateTotals()
        {
            if (this.dataHandler.config == null) return;
            if (this.dataHandler.targetMap == null) return;
            var targets = this.dataHandler.targetMap;
            this.isLogouted = true;

            //Utility.addLogInfo(this.dataHandler.logger, string.Format("{0}", "開始更新各股成交量"));

            foreach (var target in targets)
            {
                var errCode = Marshal.StringToHGlobalAnsi("");
                var errMsg = Marshal.StringToHGlobalAnsi("");
                if (target.Value.registered)
                {
                    RayinAPI.DelQuote(1, target.Key, ref errCode, ref errMsg);
                    var code = Marshal.PtrToStringAnsi(errCode);
                    if (code == "00" || code == " ") target.Value.registered = false;
                    continue;
                }
                else
                {
                    RayinAPI.AddQuote(1, target.Key, ref errCode, ref errMsg);
                    Thread.Sleep(1);
                    RayinAPI.DelQuote(1, target.Key, ref errCode, ref errMsg);
                }
            }

            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "完成更新各股成交量"));
        }
        public bool updateCapitalOnly()
        {
            return this.dataHandler.updateCapitalOnly();
        }
        public bool login()
        {
            if (this.isLogined) return true;
            var config = this.dataHandler.config;
            if (config == null) return false;
            if (this.dataHandler.updateCapitalOnly())
            {
                this.dataHandler.updateCapitals();
                return true;
            }
            
            DateTime begin = DateTime.ParseExact(Convert.ToString(config.Login.time.begin), "HH:mm:ss", null);
            Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "等待登入時刻"));
            while (DateTime.Now.TimeOfDay.CompareTo(begin.TimeOfDay) < 0)
            {
                Thread.Sleep(100);
            }

            RayinAPI.SetDebugMode(Convert.ToBoolean(config.Login.debug));
            RayinAPI.SetServer(
                    Convert.ToString(config.Login.host), Convert.ToInt32(config.Login.port));

            var result = RayinAPI.Login(
                            Convert.ToString(config.Login.username),
                            Convert.ToString(config.Login.password));
            if (result)
            {
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "請求登入成功"));
            }
            else
            {
                Utility.addLogError(this.dataHandler.logger, string.Format("{0}", "請求登入失敗"));
            }
            return result;
        }
        public bool shouldLogout()
        {
            var config = this.dataHandler.config;
            if (config == null) return false;
            if (this.dataHandler.updateCapitalOnly()) return true;
            DateTime end = DateTime.ParseExact(Convert.ToString(config.Login.time.end), "HH:mm:ss", null);
            if (DateTime.Now.TimeOfDay.CompareTo(end.TimeOfDay) <= 0) return false;
            else return true;
        }
        public void logout()
        {
            this.isLogined = false;
            var result = RayinAPI.Logout();

            if (result)
            {
                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "登出成功"));
            }
            else
            {
                Utility.addLogError(this.dataHandler.logger, string.Format("{0}", "登出失敗"));

                RayinAPI.DisconnecrQuoteServer();
                this.isQuoteConnected = false;
                RayinAPI.DisconnecrOrderServer();
                this.isOrderConnected = false;
                RayinAPI.DisconnecrAckMatServer();
                this.isAckMatConnected = false;

                Utility.addLogDebug(this.dataHandler.logger, string.Format("{0}", "主動斷線成功"));
            }
        }
        public void storeRecords()
        {
            this.dataHandler.storeRecords();
        }
    }
}