using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace AutoTrade
{
    public class Stock
    {
        //public int orderAmount;
        public int matchAmount;
        public int buyTimes;
        public int sellTimes;
        public Stock()
        {
            //this.orderAmount = 0;
            this.matchAmount = 0;
            this.buyTimes = 0;
            this.sellTimes = 0;
        }
        public Stock(dynamic stock)
        {
            this.matchAmount = Convert.ToInt32(stock);
            if (this.matchAmount > 0)
            {
                this.buyTimes = 1;
                this.sellTimes = 0;
            }
            else if (this.matchAmount < 0)
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
        public class Record
        {
            public string symbol;
            public string capital;
            public int total;
            public int stock;

            public Record(string symbol, Target target)
            {
                this.symbol = symbol;
                this.capital = target.capital;
                this.total = target.totalAmount;
                this.stock = target.stockData.matchAmount;
            }
        }

        public Utility.T30 marketType;
        public float openPrice;
        public float maxPrice;
        public float minPrice;
        public float nowPrice;
        public int totalAmount;
        public Stock stockData;
        public float bullPrice;
        public float ldcPrice;
        public float bearPrice;
        public char dealType;
        public char disposeMark;
        public char monitorMark;
        public char limitMark;
        public char dayTradeMark;
        public string capital;
        public bool registered;

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
            this.capital = "0";
            this.stockData = new Stock();
            this.registered = false;
        }
        public Target(dynamic target)
        {
            this.capital = Convert.ToString(target.capital);
            this.totalAmount = Convert.ToInt32(target.total);
            this.stockData = new Stock(target.stock);

            /*
            this.bullPrice = 0;
            this.ldcPrice = 0;
            this.bearPrice = 0;
            this.dealType = values[41];
            this.disposeMark = values[42];
            this.monitorMark = values[43];
            this.limitMark = values[44];
            this.dayTradeMark = values[87];
            this.nowPrice = this.ldcPrice;
            */
        }
        public void updateFromRecord(dynamic target)
        {
            this.capital = Convert.ToString(target.capital);
            this.totalAmount = Convert.ToInt32(target.total);
            this.stockData = new Stock(target.stock);
        }
        public void updateFromSymbol(dynamic target)
        {
            this.capital = Convert.ToString(target.capital);
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
        public Dictionary<string, Target> targetMap;
        public dynamic config;
        public StreamWriter logger;

        public bool initConfig()
        {
            string strPath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return false;

            string[] files = Directory.GetFiles(strWorkPath, "Settings.json", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                throw new FileNotFoundException("Cannot find a \"Settings.json\" to load!");
            }
            else if (files.Length > 1)
            {
                throw new FileLoadException("Cannot load from Multiple \"Settings.json\" files!");
            }

            this.config = JsonConvert.DeserializeObject(File.ReadAllText(files[0]));
            return true;
        }
        public bool updateCapitalOnly()
        {
            if (this.config == null) return false;
            return Convert.ToBoolean(this.config.Urls.Info.update);
        }
        public void initTargetMap()
        {
            if (this.config == null) return;
            this.targetMap = new Dictionary<string, Target>();

            Utility.addLogDebug(this.logger, string.Format("{0}", "開始初始化標的表"));
            TimeSpan time = TimeSpan.Parse(Convert.ToString(this.config.Login.time.download));
            Utility.addLogDebug(this.logger, string.Format("{0}", "等待盤前檔準備好"));

            var date = DateTime.Today.ToString("yyyyMMdd");
            if (!this.updateCapitalOnly())
            {
                while (DateTime.Now.TimeOfDay.CompareTo(time) < 0)
                {
                    Thread.Sleep(100);
                }
            }
            else if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                date = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
            }

            var client = new HttpClient();

            HttpResponseMessage responseT30S =
                client.GetAsync(Convert.ToString(this.config.Urls.T30.TSE) + date).Result;

            if (!responseT30S.IsSuccessStatusCode)
            {
                Utility.addLogWarning(this.logger, string.Format("{0}", "無法取得上市盤前檔"));
                return;
            }

            var contentT30S = responseT30S.Content.ReadAsStreamAsync().Result;

            // Encoding.GetEncoding("big5")); //950
            var T30S = new StreamReader(contentT30S);
            Utility.addLogDebug(this.logger, string.Format("{0}", "正在處理上市盤前檔"));
            while (!T30S.EndOfStream)
            {
                var values = T30S.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    if (values.Substring(0, 6).Count(c => !char.IsWhiteSpace(c)) == 4)
                        this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.TWSE));
                }
            }

            HttpResponseMessage responseT30O =
                client.GetAsync(Convert.ToString(this.config.Urls.T30.OTC) + date).Result;

            if (!responseT30O.IsSuccessStatusCode)
            {
                Utility.addLogWarning(this.logger, string.Format("{0}", "無法取得上櫃盤前檔"));
                return;
            }

            var contentT30O = responseT30O.Content.ReadAsStreamAsync().Result;

            var T30O = new StreamReader(contentT30O);
            Utility.addLogDebug(this.logger, string.Format("{0}", "正在處理上櫃盤前檔"));
            while (!T30O.EndOfStream)
            {
                var values = T30O.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    if (values.Substring(0, 6).Count(c => !char.IsWhiteSpace(c)) == 4)
                        this.targetMap.Add(values.Substring(0, 6), new Target(values, Utility.T30.ROCO));
                }
            }
            Utility.addLogDebug(this.logger, string.Format("{0}", "完成初始化標的表"));
        }

        public void fillTargetMap()
        {
            if (this.config == null) return;
            Utility.addLogDebug(this.logger, string.Format("{0}", "嘗試讀取記錄檔"));

            string tempPath = Convert.ToString(this.config.Paths.Records);
            string strPath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return;

            var recordPath = Path.Combine(strWorkPath, tempPath);
            if (!File.Exists(recordPath))
            {
                Utility.addLogDebug(this.logger, string.Format("{0}", "記錄檔不存在"));
                return;
            }

            Utility.addLogDebug(this.logger, string.Format("{0}", "開始更新標的表"));

            dynamic text = JsonConvert.DeserializeObject(File.ReadAllText(recordPath));
            if (text == null) return;
            if (this.targetMap == null) return;
            if (this.targetMap.Count == 0)
            {
                Utility.addLogWarning(this.logger, string.Format("{0}", "標的表無法更新"));
                return;
            }

            foreach (var target in text)
            {
                // TODO: UPDATE TABLE WITH RECORDS FORCELY
                var key = Convert.ToString(target.symbol);
                if (this.targetMap.ContainsKey(key))
                {
                    this.targetMap[key].updateFromRecord(target);
                }
                else
                {
                    this.targetMap.Add(key, new Target(target));
                    Utility.addLogWarning(this.logger, string.Format("{0} [股票:{1,-6}]", "盤前檔中不存在的紀錄", key));
                }
            }
            Utility.addLogDebug(this.logger, string.Format("{0}", "完成更新標的表"));
        }
        public void initLogger()
        {
            if (this.config == null) return;
            string strPath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return;

            string logDir = Convert.ToString(this.config.Paths.LogDir);
            string pathDir = Path.Combine(strWorkPath, logDir);
            Directory.CreateDirectory(pathDir);

            var logPath = Path.Combine(pathDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            FileStream file = File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            this.logger = new StreamWriter(file, Encoding.GetEncoding("big5"));
        }
        public void updateCapitals()
        {
            if (this.config == null) return;
            if (this.targetMap == null) return;

            string url = Convert.ToString(this.config.Urls.Info.url);
            bool fund = Convert.ToBoolean(this.config.Rules.Exclude.IDLength.fund);
            var client = new HttpClient();
            var parameters = new Dictionary<string, string> {
                { "step", "1" }, { "run", "" }, {"firstin", "1"}, {"co_id", "0"} };

            foreach (var target in this.targetMap)
            {
                if (target.Key.Count(c => !Char.IsWhiteSpace(c)) != 4) continue;
                if (fund && target.Key.CompareTo(Utility.DEF_FUND_CODE) <= 0) continue;

                parameters["co_id"] = target.Key;
                bool getData = false;
                int counter = 0;

                while (true)
                {
                    Thread.Sleep(3000);
                    using (var encodedContent = new FormUrlEncodedContent(parameters))
                    {
                        if (encodedContent == null) continue;
                        try
                        {
                            using (var response = client.PostAsync(url, encodedContent).Result)
                            {
                                if (response == null || !response.IsSuccessStatusCode) continue;
                                using (var stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                                {
                                    if (stream == null) continue;

                                    counter++;
                                    while (!stream.EndOfStream)
                                    {
                                        var line = stream.ReadLine();
                                        if (line != null && line.Contains("實收資本額"))
                                        {
                                            for (var i = 0; i < 5; i++)
                                                line = stream.ReadLine();
                                            if (line == null) break;
                                            target.Value.capital =
                                                line.Replace("<td>", "").Replace("</td>", "").Replace(" ", "").Replace(",", "");
                                            getData = true;
                                            Utility.addLogDebug(this.logger,
                                                string.Format("{0} | 股票:{1,-6} 資本額:{2,-15}", 
                                                              "更新資本額成功", target.Key, target.Value.capital));
                                            break;
                                        }
                                        // else if (line != null && line.Contains("Overrun")) break;
                                    }
                                    if (getData || counter == 3) break;
                                }
                            }
                        }
                        catch (AggregateException err)
                        {
                            foreach (var errInner in err.InnerExceptions)
                            {
                                // Console.WriteLine(errInner.ToString());
                            }
                        }
                    }
                }
            }
        }
        public void storeRecords()
        {
            Utility.addLogDebug(this.logger, string.Format("{0}", "準備回寫紀錄"));
            string strPath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strPath);
            if (strWorkPath == null) return;
            if (this.config == null) return;

            var recordPath = Convert.ToString(this.config.Paths.Records);
            var path = Path.Combine(strWorkPath, recordPath);
            var records = new List<Target.Record>();

            if (this.targetMap == null) return;

            foreach (var target in this.targetMap)
            {
                if (target.Value.stockData.matchAmount != 0)
                    Utility.addLogInfo(this.logger, string.Format("{0} | 股票:{1,-6} 庫存:{2,-3}", 
                                       "今日總結", target.Key, target.Value.stockData.matchAmount));
                records.Add(new Target.Record(target.Key, target.Value));
            }

            string text = JsonConvert.SerializeObject(records, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);

            Utility.addLogDebug(this.logger, string.Format("{0}", "完成回寫紀錄"));

            if (this.logger != null) this.logger.Close();
        }
        public DataHandler()
        {
            if (!this.initConfig()) return;
            this.initLogger();
            this.initTargetMap();
            this.fillTargetMap();
            //this.updateCapitals();
        }
    }
}
