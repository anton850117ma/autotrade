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
        public static bool nearlyEqual(float a, float b)
        {
            if (Math.Abs(a - b) < Single.Epsilon * 2) return true;
            else return false;
        }

        public static void addLogDebug(StreamWriter? logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [DEBUG]: " + msg);
                    logger.Flush();
                }
            }
        }
        public static void addLogInfo(StreamWriter? logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [INFO]: " + msg);
                    // logger.Flush();
                }
            }
        }
        public static void addLogWarning(StreamWriter? logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [WARNING]: " + msg);
                    // logger.Flush();
                }
            }
        }
        public static void addLogError(StreamWriter? logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [ERROR]: " + msg);
                    logger.Flush();
                }
            }
        }
        public static void addLogCrtical(StreamWriter? logger, string msg)
        {
            if (logger != null)
            {
                lock (logger)
                {
                    logger.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [CRITICAL]: " + msg);
                    logger.Flush();
                }
            }
        }
    }
}