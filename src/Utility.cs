using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTrade
{
    public class Utility
    {
        //XMLText = XML電文完整內容  XMLName = XML欄位值
        public static string GetXMLValue(string XMLText, string XMLName) 
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
    }
}