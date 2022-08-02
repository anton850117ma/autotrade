using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using Tomlyn;
using Tomlyn.Model;
using System.Text.RegularExpressions;

namespace AutoTrade
{
    public class TargetT30
    {
        public Utility.T30 type;
        public Single? bullPrice;
        public Single? ldcPrice;
        public Single? bearPrice;
        public char? dealType;
        public char? disposeMark;
        public char? monitorMark;
        public char? limitMark;
        public char? dayTradeMark;

        public TargetT30(string values, Utility.T30 type)
        {
            this.type = type;
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
        }
    }

    public class Target
    {
        public Single? ldcPrice;
        public Single? openPrice;
        public Single? maxPrice;
        public Single? minPrice;
        public Single? nowPrice;
        public int? totalAmount;

        public void updateTarget(Single openPrice, Single maxPrice, Single minPrice,
                                 Single nowPrice, int totalAmount)
        {
            this.openPrice = openPrice;
            this.maxPrice = maxPrice;
            this.minPrice = minPrice;
            this.nowPrice = nowPrice;
            this.totalAmount = totalAmount;
        }

        public Target(TomlTable table)
        {
            this.ldcPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["close"]);
            this.openPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["open"]);
            this.maxPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["max"]);
            this.minPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["min"]);
            this.totalAmount = Convert.ToInt32(table[Utility.DEF_AMOUNT_STR]);
            // this.nowPrice = this.closePrice;
        }
    }

    public class Stock
    {
        int? amount;
        Single? matchPrice;
        string? orderType;
        Utility.StockStatus? status;

        public Stock(TomlTable table)
        {
            this.amount = Convert.ToInt32(table[Utility.DEF_AMOUNT_STR]);
            this.matchPrice = Convert.ToSingle(table[Utility.DEF_PRICE_STR]);
            this.orderType = Convert.ToString(table[Utility.DEF_TYPE_STR]);
        }
    }

    public class DataHandler
    {
        Dictionary<string, TargetT30>? targetT30Map;
        Dictionary<string, Target>? targetQuoteMap;
        Dictionary<string, Stock>? targetStockMap;

        public void initT30Map(DateTime time)
        {
            // while (DateTime.Now.TimeOfDay.CompareTo(time.TimeOfDay) < 0)
            // {
            //     // Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            //     // Console.Write("\rWaiting for T30 files...");
            //     Thread.Sleep(1000);
            // }
            // var date = DateTime.Now.ToString("_yyyyMMdd");
            this.targetT30Map = new Dictionary<string, TargetT30>();
            var date = "_20220801";

            var client = new HttpClient();

            var responseT30S = client.GetAsync(Utility.DEF_T30_URL + "S" + date);
            var contentT30S = responseT30S.Result.Content.ReadAsStreamAsync().Result;

            var T30S = new StreamReader(contentT30S, Encoding.GetEncoding("big5")); //950
            while (!T30S.EndOfStream)
            {
                var values = T30S.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetT30Map.Add(values.Substring(0, 6), new TargetT30(values, Utility.T30.TWSE));
                }
            }
            Console.WriteLine(this.targetT30Map.Count);

            var responseT30O = client.GetAsync(Utility.DEF_T30_URL + "O" + date);
            var contentT30O = responseT30O.Result.Content.ReadAsStreamAsync().Result;

            var T30O = new StreamReader(contentT30O, Encoding.GetEncoding("big5"));
            while (!T30O.EndOfStream)
            {
                var values = T30O.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    this.targetT30Map.Add(values.Substring(0, 6), new TargetT30(values, Utility.T30.TWSE));
                }
            }
            Console.WriteLine(this.targetT30Map.Count);
        }

        public void initQuoteMap(string filename_quotes)
        {
            var targets = Toml.ToModel(File.ReadAllText(filename_quotes));
            this.targetQuoteMap = new Dictionary<string, Target>();

            if (targets.Count > 0)
            {
                foreach (var target in targets)
                    this.targetQuoteMap.Add(target.Key, new Target((TomlTable)target.Value));
            }
        }

        public void initStockMap(string filename_stocks)
        {
            var stocks = Toml.ToModel(File.ReadAllText(filename_stocks));
            this.targetStockMap = new Dictionary<string, Stock>();

            if (stocks.Count > 0)
            {
                foreach (var target in stocks)
                    this.targetStockMap.Add(target.Key, new Stock((TomlTable)target.Value));
            }
        }

        public DataHandler(string filenameT30T, string filenameT30O,
                           string filename_quotes, string filename_stocks)
        {
            this.initT30Map(new DateTime(2022, 8, 3, 8, 30, 0));
            this.initQuoteMap(filename_quotes);
            this.initStockMap(filename_stocks);
        }
    }
}