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
        double? upperPrice;
        double? closePrice;
        double? lowerPrice;
        string? tradeType;
        bool? isDisposed;
        bool? isMonitored;
        bool? isLimited;
        bool? isTradable;

        public TargetT30(string name, string[] values)
        {
            this.name = name;
            this.category = values[(int)Utility.IndexT30.Category];
            this.upperPrice = Convert.ToDouble(values[(int)Utility.IndexT30.UpperPrice]);
            this.closePrice = Convert.ToDouble(values[(int)Utility.IndexT30.ClosePrice]);
            this.lowerPrice = Convert.ToDouble(values[(int)Utility.IndexT30.LowerPrice]);
            this.tradeType = values[(int)Utility.IndexT30.TradeType];

            if (values[(int)Utility.IndexT30.IsDisposed] != Utility.DEF_NOREMAL_STRING)
                this.isDisposed = true;
            else this.isDisposed = false;

            if (values[(int)Utility.IndexT30.IsMonitored] != Utility.DEF_NOREMAL_STRING)
                this.isMonitored = true;
            else this.isMonitored = false;

            if (values[(int)Utility.IndexT30.IsLimited] != Utility.DEF_NOREMAL_STRING)
                this.isLimited = true;
            else this.isLimited = false;

            if (values[values.Length - 2] != Utility.DEF_DISABLED_STRING)
                this.isTradable = true;
            else this.isTradable = false;

        }
    }
    public class DataHandler
    {
        Dictionary<string, TargetT30>? targetT30Map;

        public void initT30Map(string filename_T30)
        {

            var t30_path = Path.Combine(Directory.GetCurrentDirectory(), filename_T30);
            var t30_reader = new StreamReader(t30_path, Encoding.GetEncoding("big5")); //950
            targetT30Map = new Dictionary<string, TargetT30>();

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
                        var key = values[(int)Utility.IndexT30.Code];
                        if (key.Length == Utility.DEF_TARGET_CODE_LEN) {
                            targetT30Map.Add(key, new TargetT30(name, values));
                        }
                    }
                }
            }
        }

        public DataHandler(string filename_T30)
        {
            this.initT30Map(filename_T30);
            // var settings_toml = Toml.ToModel(File.ReadAllText(settings_path));

        }
    }
}