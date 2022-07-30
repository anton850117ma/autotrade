using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace AutoTrade
{
    public class TargetT30
    {
        string? name;
        int? category;
        double? upperPrice;
        double? closePrice;
        double? lowerPrice;
        string? tradeType;
        bool? isDisposed;
        bool? isMonitored;
        bool? isLimited;
        bool? isTradable;
    }
    public class DataHandler
    {
        Dictionary<string, TargetT30> TargetT30Map;

        public DataHandler(string data_path)
        {
            var settings_path = Path.Combine(Directory.GetCurrentDirectory(), data_path, "settings.ini");
            var settings_toml = Toml.ToModel(File.ReadAllText(settings_path));

        }
    }
}