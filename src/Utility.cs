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
        
        // var cfg_string = File.OpenText(@"cfg\\settings.ini").ReadToEnd();
        // var cfg_toml = Toml.ToModel(cfg_string);
        // string strExeFilePath = Assembly.GetExecutingAssembly().Location;
        // string strWorkPath = Path.GetDirectoryName(strExeFilePath);
        // var path = Path.Combine(Directory.GetCurrentDirectory(), "\\fileName.txt");

        // Console.WriteLine(strWorkPath);
        // string strSettingsXmlFilePath = Path.Combine(strWorkPath, "Settings.xml");
        // var path = Path.Combine(Application.StartupPath, "\\fileName.txt");
        // string contents = File.ReadAllText(path);


        // foreach (string file in Directory.EnumerateFiles("Release", "*.xml"))
        // {
        //     string contents = File.ReadAllText(file);
        // }
        // var login_args = (TomlTable)cfg_toml["Login"];
        // Console.WriteLine(login_args["host"]);
        // Console.WriteLine(login_args["port"]);
        // Console.WriteLine(login_args["account"]);
        // Console.WriteLine(login_args["start"]);

        // var rules = (TomlTable)cfg_toml["Rule"];
        // var rule1 = (TomlTable)rules["ExcludeNotDayTradeTarget"];
        // Console.WriteLine(rule1["enabled"]);

        //Console.WriteLine(model["global"]);
        // // Prints "1"
        // Console.WriteLine(((TomlTable)model["my_table"]!)["key"]);
        // // Prints 4, 5, 6
        // Console.WriteLine(string.Join(", ", (TomlArray)((TomlTable)model["my_table"]!)["list"]));

    }
}