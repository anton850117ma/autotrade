using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Tomlyn;
using Tomlyn.Model;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoTrade
{
    public class TargetT30
    {
        string? name;
        string? category;
        Single? upperPrice;
        Single? closePrice;
        Single? lowerPrice;
        string? tradeType;
        bool? isDisposed;
        bool? isMonitored;
        bool? isLimited;
        bool? isTradable;

        public TargetT30(string name, string[] values)
        {
            this.name = name;
            this.category = values[(int)Utility.IndexT30.Category];
            this.upperPrice = Convert.ToSingle(values[(int)Utility.IndexT30.UpperPrice]);
            this.closePrice = Convert.ToSingle(values[(int)Utility.IndexT30.ClosePrice]);
            this.lowerPrice = Convert.ToSingle(values[(int)Utility.IndexT30.LowerPrice]);
            this.tradeType = values[(int)Utility.IndexT30.TradeType];

            if (!values[(int)Utility.IndexT30.IsDisposed].Contains(Utility.DEF_NOREMAL_STR))
                this.isDisposed = true;
            else this.isDisposed = false;

            if (values[(int)Utility.IndexT30.IsMonitored] != Utility.DEF_NOREMAL_STR)
                this.isMonitored = true;
            else this.isMonitored = false;

            if (values[(int)Utility.IndexT30.IsLimited] != Utility.DEF_NOREMAL_STR)
                this.isLimited = true;
            else this.isLimited = false;

            if (values[values.Length - 2] != Utility.DEF_DISABLED_STR)
                this.isTradable = true;
            else this.isTradable = false;

        }
    }

    public class Target
    {
        Single? closePrice;
        Single? upperPrice;
        Single? lowerPrice;
        Single? openPrice;
        Single? maxPrice;
        Single? minPrice;
        Single? nowPrice;
        int? totalAmount;

        public void setTarget(Single today_price, Single upper_price, Single lower_price,
                              Single open_price, Single max_price, Single min_price,
                              Single now_price, int total_amount)
        {
            this.closePrice = today_price;
            this.upperPrice = upper_price;
            this.lowerPrice = lower_price;
            this.openPrice = open_price;
            this.maxPrice = max_price;
            this.minPrice = min_price;
            this.nowPrice = now_price;
            this.totalAmount = total_amount;
        }

        public Target(TomlTable table)
        {
            this.closePrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["close"]);
            this.upperPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["upper"]);
            this.lowerPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["lower"]);
            this.openPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["open"]);
            this.maxPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["max"]);
            this.minPrice = Convert.ToSingle(((TomlTable)table[Utility.DEF_PRICE_STR])["min"]);
            this.totalAmount = Convert.ToInt32(table[Utility.DEF_AMOUNT_STR]);
            this.nowPrice = this.closePrice;
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

        public void initT30Map(string filename_T30)
        {
            var t30_path = Path.Combine(Directory.GetCurrentDirectory(), filename_T30);
            var t30_reader = new StreamReader(t30_path, Encoding.GetEncoding("big5")); //950
            this.targetT30Map = new Dictionary<string, TargetT30>();

            while (!t30_reader.EndOfStream)
            {
                var first_line = t30_reader.ReadLine();
                if (string.IsNullOrEmpty(first_line) ||
                    string.IsNullOrWhiteSpace(first_line) ||
                    first_line.Contains("結　束"))
                {
                    continue;
                }
                else if (first_line.Contains("報表名稱"))
                {
                    for (var i = 0; i < 6; i++)
                        t30_reader.ReadLine();
                }
                else
                {
                    var second_line = t30_reader.ReadLine();
                    if (!string.IsNullOrEmpty(second_line))
                    {
                        var values = Regex.Split(first_line, @"\s{2,}");
                        var name = Regex.Split(second_line, @"\s{8,}")[1];
                        var code = values[(int)Utility.IndexT30.Code];
                        if (code.Length == Utility.DEF_TARGET_CODE_LEN)
                        {
                            this.targetT30Map.Add(code, new TargetT30(name, values));
                        }
                    }
                }
            }
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

        public DataHandler(string filename_T30, string filename_quotes, string filename_stocks)
        {
            this.initT30Map(filename_T30);
            this.initQuoteMap(filename_quotes);
            this.initStockMap(filename_stocks);

            // Console.WriteLine(this.targetT30Map.Count);
            // Console.WriteLine(this.targetQuoteMap.Count);
            // Console.WriteLine(this.targetStockMap.Count);
        }
    }
}