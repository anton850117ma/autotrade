using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Tomlyn;
using Tomlyn.Model;
using System.Text.RegularExpressions;

namespace AutoTrade
{
    public class Stock
    {
        public Utility.StockStatus? status;
        public int? amount;
        public int? buyTimes;
        public int? sellTimes;
        public Stock() { }
        // public Stock(TomlTable table)
        // {
        //     this.amount = Convert.ToInt32(table[Utility.DEF_AMOUNT_STR]);
        //     if (this.amount > 0)
        //     {
        //         this.buyTimes = 1;
        //         this.sellTimes = 0;
        //     }
        //     else if (this.amount < 0)
        //     {
        //         this.buyTimes = 0;
        //         this.sellTimes = 1;
        //     }
        //     else
        //     {
        //         this.buyTimes = 0;
        //         this.sellTimes = 0;
        //     }
        // }

        public Stock(dynamic stock)
        {
            this.amount = Convert.ToInt32(stock.amount);
            if (this.amount > 0)
            {
                this.buyTimes = 1;
                this.sellTimes = 0;
            }
            else if (this.amount < 0)
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
        public Utility.T30 marketType;
        public Single? openPrice;
        public Single? maxPrice;
        public Single? minPrice;
        public Single? nowPrice;
        public int? totalAmount;
        public Stock? stockData;
        public Single? bullPrice;
        public Single? ldcPrice;
        public Single? bearPrice;
        public char? dealType;
        public char? disposeMark;
        public char? monitorMark;
        public char? limitMark;
        public char? dayTradeMark;

        public Target(string values, Utility.T30 type)
        {
            this.marketType = type;
            this.bullPrice = Convert.ToSingle(values.Substring(6, 9)) / Utility.DEF_PRICE_FACTOR;
            this.ldcPrice = Convert.ToSingle(values.Substring(15, 9)) / Utility.DEF_PRICE_FACTOR;
            this.bearPrice = Convert.ToSingle(values.Substring(24, 9)) / Utility.DEF_PRICE_FACTOR;
            this.dealType = values[41];
            this.disposeMark = values[42];
            this.monitorMark = values[43];
            this.limitMark = values[44];
            if (type == Utility.T30.TWSE)
                this.dayTradeMark = values[86];
            else this.dayTradeMark = values[87];
            this.stockData = new Stock();
        }
        // public void updateFromRecord(TomlTable table)
        // {
        //     this.openPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["open"]);
        //     this.maxPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["max"]);
        //     this.minPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["min"]);
        //     this.totalAmount = Convert.ToInt32(table[Utility.DEF_TOTAL_STR]);
        //     this.stockData = new Stock((TomlTable)table[Utility.DEF_STOCK_STR]);
        //     this.nowPrice = this.ldcPrice;
        // }
        public void updateFromRecord(dynamic target)
        {
            this.openPrice = Convert.ToSingle(target.price.open);
            this.maxPrice = Convert.ToSingle(target.price.max);
            this.minPrice = Convert.ToSingle(target.price.min);
            this.totalAmount = Convert.ToInt32(target.total);
            this.stockData = new Stock(target.stock);
            this.nowPrice = this.ldcPrice;
        }

        public void updateFromQuote(Single openPrice, Single maxPrice, Single minPrice,
                                    Single nowPrice, int totalAmount)
        {
            this.openPrice = openPrice;
            this.maxPrice = maxPrice;
            this.minPrice = minPrice;
            this.nowPrice = nowPrice;
            this.totalAmount = totalAmount;
        }
    }

    public class DataHandler
    {
        public Dictionary<string, Target>? targetMap;
        public dynamic? config;

        public void initTargetMap(TimeSpan time)
        {
            while (DateTime.Now.TimeOfDay.CompareTo(time) < 0)
            {
                // Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                // Console.Write("\rWaiting for T30 files...");
                Thread.Sleep(1000);
            }
            var date = DateTime.Now.ToString("_yyyyMMdd");
            this.targetMap = new Dictionary<string, Target>();
            // var date = "_20220801";

            var client = new HttpClient();

            var responseT30S = client.GetAsync(Utility.DEF_T30_URL + "S" + date);
            var contentT30S = responseT30S.Result.Content.ReadAsStreamAsync().Result;

            var T30S = new StreamReader(contentT30S, Encoding.GetEncoding("big5")); //950
            while (!T30S.EndOfStream)
            {
                var values = T30S.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.TWSE));
                }
            }

            var responseT30O = client.GetAsync(Utility.DEF_T30_URL + "O" + date);
            var contentT30O = responseT30O.Result.Content.ReadAsStreamAsync().Result;

            var T30O = new StreamReader(contentT30O, Encoding.GetEncoding("big5"));
            while (!T30O.EndOfStream)
            {
                var values = T30O.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.ROCO));
                }
            }
        }

        public void fillTargetMap(string path_records)
        {
            dynamic? text = JsonConvert.DeserializeObject(File.ReadAllText(path_records));
            if (text == null) return;
            if (this.targetMap == null) return;

            foreach (var target in text.targets)
            {
                var key = Convert.ToString(target.code);
                if (this.targetMap.ContainsKey(key))
                {
                    this.targetMap[key].updateFromRecord(target);
                }
            }

            // var records = Toml.ToModel(File.ReadAllText(path_records));

            // if (records.Count > 0 && this.targetMap != null)
            // {
            //     foreach (var record in records)
            //     {
            //         var key = record.Key.PadRight(6);
            //         if (this.targetMap.ContainsKey(key))
            //         {
            //             this.targetMap[key].updateFromRecord((TomlTable)record.Value);
            //         }
            //     }
            // }
        }
        public DataHandler(string path_settings, string path_records)
        {
            this.config = JsonConvert.DeserializeObject(File.ReadAllText(path_settings));
            if (this.config == null) return;
            this.initTargetMap(TimeSpan.Parse(Convert.ToString(this.config.Login.time.download)));
            this.fillTargetMap(path_records);
        }
    }
}