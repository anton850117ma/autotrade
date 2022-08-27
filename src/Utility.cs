using System;
using System.IO;

namespace AutoTrade
{
    public class Utility
    {
        public enum Verbose
        {
            Trace,
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }
        public enum T30
        {
            TWSE,
            ROCO
        }

        public static Verbose verbose;
        public const int DEF_SHARE_LEN = 4;
        public const int DEF_PRICE_FACTOR = 10000;

        //XMLText = XML電文完整內容  XMLName = XML欄位值
        public static string getXMLValue(string XMLText, string XMLName)
        {
            int pos = XMLText.ToLower().IndexOf(XMLName.ToLower() + "='");
            if (pos > 0)
            {
                string result = XMLText.Remove(0, pos);
                result = result.Remove(0, result.IndexOf("'") + 1);
                result = result.Substring(0, result.IndexOf("'"));
                return result;
            }
            else
            {
                return ""; //無此欄位則回傳空白
            }
        }
        public static string addPading(string symbol)
        {
            return symbol.PadRight(6, ' ');
        }
        public static bool nearlyEqual(float a, float b)
        {
            if (Math.Abs(a - b) < Single.Epsilon * 2) return true;
            else return false;
        }
        public static int compareNumbers(string a, string b)
        {
            if (a.Length > b.Length) return 1;
            else if (a.Length < b.Length) return -1;
            else return a.CompareTo(b);
        }
        public static void addLog(StreamWriter logger, Verbose level, string msg)
        {
            if (level < verbose) return;
            else
            {
                switch (level)
                {
                    case Verbose.Trace:
                        addLogTrace(logger, msg); break;
                    case Verbose.Debug:
                        addLogDebug(logger, msg); break;
                    case Verbose.Info:
                        addLogInfo(logger, msg); break;
                    case Verbose.Warning:
                        addLogWarning(logger, msg); break;
                    case Verbose.Error:
                        addLogError(logger, msg); break;
                    case Verbose.Critical:
                        addLogCritical(logger, msg); break;
                }
            }
        }
        public static void addLogTrace(StreamWriter logger, string msg)
        {
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "TRACE", msg));
                // logger.Flush();
            }
        }
        public static void addLogDebug(StreamWriter logger, string msg)
        {   
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}", 
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "DEBUG", msg));
                logger.Flush();
            }
        }
        public static void addLogInfo(StreamWriter logger, string msg)
        {
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "INFO", msg));
                // logger.Flush();
            }
        }
        public static void addLogWarning(StreamWriter logger, string msg)
        {
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "WARNING", msg));
                // logger.Flush();
            }
        }
        public static void addLogError(StreamWriter logger, string msg)
        {
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "ERROR", msg));
                logger.Flush();
            }
        }
        public static void addLogCritical(StreamWriter logger, string msg)
        {
            lock (logger)
            {
                logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                 DateTime.Now.ToString("HH:mm:ss.ffffff"), "CRITICAL", msg));
                logger.Flush();
            }
        }
    }
}
