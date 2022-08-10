using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace AutoTrade
{
    public class Stock
    {
        public int orderAmount;
        public int matchAmount;
        public int buyTimes;
        public int sellTimes;
        public Stock()
        {
            this.orderAmount = 0;
            this.matchAmount = 0;
            this.buyTimes = 0;
            this.sellTimes = 0;
        }
        public Stock(dynamic stock)
        {
            this.orderAmount = Convert.ToInt32(stock.amount);
            this.matchAmount = this.orderAmount;
            if (this.orderAmount > 0)
            {
                this.buyTimes = 1;
                this.sellTimes = 0;
            }
            else if (this.orderAmount < 0)
            {
                this.buyTimes = 0;
                this.sellTimes = 1;
            }
            else
            {
                this.buyTimes = 0;
                this.sellTimes = 0;
            }
        }
    }

    public class Target
    {
        public class price
        {
            public Single open;
            public Single max;
            public Single min;
            public price(Target target)
            {
                this.open = target.openPrice;
                this.max = target.maxPrice;
                this.min = target.minPrice;
            }
        }
        public class stock
        {
            public int amount;
            public stock(Target target)
            {
                this.amount = target.stockData.matchAmount;
            }
        }
        public class Record
        {
            public string code;
            public price price;
            public int total;
            public stock stock;

            public Record(string code, Target target)
            {
                this.code = code;
                this.price = new price(target);
                this.total = target.totalAmount;
                this.stock = new stock(target);
            }
        }

        public Utility.T30 marketType;
        public Single openPrice;
        public Single maxPrice;
        public Single minPrice;
        public Single nowPrice;
        public int totalAmount;
        public Stock stockData;
        public Single bullPrice;
        public Single ldcPrice;
        public Single bearPrice;
        public char dealType;
        public char disposeMark;
        public char monitorMark;
        public char limitMark;
        public char dayTradeMark;

        public Target(string values, Utility.T30 type)
        {
            this.marketType = type;
            this.openPrice = 0;
            this.maxPrice = 0;
            this.minPrice = 0;
            this.totalAmount = 0;
            this.bullPrice = Convert.ToSingle(values.Substring(6, 9)) / Utility.DEF_PRICE_FACTOR;
            this.ldcPrice = Convert.ToSingle(values.Substring(15, 9)) / Utility.DEF_PRICE_FACTOR;
            this.bearPrice = Convert.ToSingle(values.Substring(24, 9)) / Utility.DEF_PRICE_FACTOR;
            this.dealType = values[41];
            this.disposeMark = values[42];
            this.monitorMark = values[43];
            this.limitMark = values[44];
            this.nowPrice = this.ldcPrice;
            if (type == Utility.T30.TWSE)
                this.dayTradeMark = values[86];
            else this.dayTradeMark = values[87];
            this.stockData = new Stock();
        }
        public void updateFromRecord(dynamic target)
        {
            this.openPrice = Convert.ToSingle(target.price.open);
            this.maxPrice = Convert.ToSingle(target.price.max);
            this.minPrice = Convert.ToSingle(target.price.min);
            this.totalAmount = Convert.ToInt32(target.total);
            this.stockData = new Stock(target.stock);
        }

        public void updateFromQuote(string openPrice, string maxPrice, string minPrice,
                                    string nowPrice, string totalAmount)
        {
            this.openPrice = Convert.ToSingle(openPrice);
            this.maxPrice = Convert.ToSingle(maxPrice);
            this.minPrice = Convert.ToSingle(minPrice);
            this.nowPrice = Convert.ToSingle(nowPrice);
            this.totalAmount = Convert.ToInt32(totalAmount);
        }
        public void updateFromQuoteEx(string todayPrice, string bullPrice, string bearPrice,
                                      string openPrice, string maxPrice, string minPrice,
                                      string nowPrice, string totalAmount)
        {
            this.ldcPrice = Convert.ToSingle(todayPrice);
            this.bullPrice = Convert.ToSingle(bullPrice);
            this.bearPrice = Convert.ToSingle(bearPrice);
            this.openPrice = Convert.ToSingle(openPrice);
            this.maxPrice = Convert.ToSingle(maxPrice);
            this.minPrice = Convert.ToSingle(minPrice);
            this.nowPrice = Convert.ToSingle(nowPrice);
            this.totalAmount = Convert.ToInt32(totalAmount);
        }
    }

    public class DataHandler
    {
        public Dictionary<string, Target>? targetMap;
        public dynamic? config;
        public string recordPath;
        public StreamWriter? logger;

        public void initTargetMap()
        {
            if (this.config == null) return;
            
            Utility.addLogDebug(this.logger, "開始初始化標的表");
            var time = TimeSpan.Parse(Convert.ToString(this.config.Login.time.download));

            Utility.addLogDebug(this.logger, "等待盤前檔準備好...");
            while (DateTime.Now.TimeOfDay.CompareTo(time) < 0)
            {
                // Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Thread.Sleep(100);
            }
            var date = DateTime.Now.ToString("yyyyMMdd");
            this.targetMap = new Dictionary<string, Target>();
            // var date = "_20220801";

            var client = new HttpClient();

            var responseT30S = client.GetAsync(Convert.ToString(this.config.T30.TSE) + date);
            var contentT30S = responseT30S.Result.Content.ReadAsStreamAsync().Result;

            // var path = Path.Combine(Directory.GetCurrentDirectory(), @"data\\ASCT30S_20220801.txt");
            // var T30S = new StreamReader(path); //950
            // var T30S = new StreamReader(contentT30S, Encoding.GetEncoding("big5")); //950
            var T30S = new StreamReader(contentT30S);
            Utility.addLogDebug(this.logger, "正在處理上市盤前檔...");
            while (!T30S.EndOfStream)
            {
                var values = T30S.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.TWSE));
                }
            }

            var responseT30O = client.GetAsync(Convert.ToString(this.config.T30.OTC) + date);
            var contentT30O = responseT30O.Result.Content.ReadAsStreamAsync().Result;

            var T30O = new StreamReader(contentT30O);
            Utility.addLogDebug(this.logger, "正在處理上櫃盤前檔...");
            while (!T30O.EndOfStream)
            {
                var values = T30O.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.ROCO));
                }
            }
            Utility.addLogDebug(this.logger, "完成初始化標的表");
        }

        public void fillTargetMap()
        {
            Utility.addLogDebug(this.logger, "讀取記錄檔...");
            dynamic? text = JsonConvert.DeserializeObject(File.ReadAllText(this.recordPath));
            if (text == null) return;
            if (this.targetMap == null) return;

            foreach (var target in text)
            {
                var key = Convert.ToString(target.code);
                if (this.targetMap.ContainsKey(key))
                {
                    this.targetMap[key].updateFromRecord(target);
                }
            }
            Utility.addLogDebug(this.logger, "完成更新標的表");
        }
        public void initLogger()
        {
            string strPath = Assembly.GetExecutingAssembly().Location;
            string? strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return;
            if (this.config == null) return;

            var logDir = Convert.ToString(this.config.LogDir);
            var pathDir = Path.Combine(strWorkPath, logDir);
            Directory.CreateDirectory(pathDir);

            var logPath = Path.Combine(pathDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            FileStream file = File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            this.logger = new StreamWriter(file, Encoding.GetEncoding("big5"));
        }

        public void storeRecords()
        {
            Utility.addLogDebug(this.logger, "準備回寫紀錄...");
            string strPath = Assembly.GetExecutingAssembly().Location;
            string? strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return;

            // TODO: should modify when passing tests
            var path = Path.Combine(strWorkPath, "Records.json");
            var records = new List<Target.Record>();

            if (this.targetMap == null) return;

            foreach (var target in this.targetMap)
            {
                records.Add(new Target.Record(target.Key, target.Value));
            }

            string text = JsonConvert.SerializeObject(records, Formatting.Indented);
            File.WriteAllText(path, text);

            Utility.addLogDebug(this.logger, "完成回寫紀錄");

            if (this.logger != null) this.logger.Close();
        }
        public DataHandler(string path_settings, string path_records)
        {
            this.recordPath = path_records;
            this.config = JsonConvert.DeserializeObject(File.ReadAllText(path_settings));
            this.initLogger();
            this.initTargetMap();
            this.fillTargetMap();
        }
    }
}