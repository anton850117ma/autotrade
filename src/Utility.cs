using System;
using System.IO;

namespace AutoTrade
{
    public class Utility
    {

        public enum T30
        {
            TWSE,
            ROCO
        }

        public const string DEF_FUND_CODE = "0061  ";
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
        public static void addLogDebug(StreamWriter logger, string msg)
        {   
            
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(string.Format("{0} [{1,-8}]: {2}", 
                                     DateTime.Now.ToString("HH:mm:ss.ffffff"), "DEBUG", msg));
                    logger.Flush();
                }
            }
            
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [DEBUG]: " + msg);
        }
        public static void addLogInfo(StreamWriter logger, string msg)
        {
            
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                     DateTime.Now.ToString("HH:mm:ss.ffffff"), "INFO", msg));
                    // logger.Flush();
                }
            }
            // Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [INFO]: " + msg);
        }
        public static void addLogWarning(StreamWriter logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                     DateTime.Now.ToString("HH:mm:ss.ffffff"), "WARNING", msg));
                    // logger.Flush();
                }
            }
        }
        public static void addLogError(StreamWriter logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(string.Format("{0} [{1,-8}]: {2}",
                                     DateTime.Now.ToString("HH:mm:ss.ffffff"), "ERROR", msg));
                    logger.Flush();
                }
            }
        }
        public static void addLogCrtical(StreamWriter logger, string msg)
        {
            if (logger != null)
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
}
