using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace TitleLibrary
{
    internal class Configuration
    {
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "TitleLibrary.json");

        // 玩家称号信息
        public Dictionary<string, PlayerTitleInfo> PlayerTitleInfos { get; set; }
        public string ChatFormat { get; set; }
        public Dictionary<string, TitleLibrary> TitlesLibrary { get; set; } // 修改为字典

        public Configuration()
        {
            ChatFormat = "{0}{1}{2}:{5}{3}{4}"; // 默认格式
            TitlesLibrary = new Dictionary<string, TitleLibrary>();
            PlayerTitleInfos = new Dictionary<string, PlayerTitleInfo>();
        }

        public void Write()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var newConfig = new Configuration();
                newConfig.Write(); // 写入初始配置文件
                TShock.Log.ConsoleError("[TitleLibrary] 未找到配置文件，已新建预设");
                return newConfig;
            }
            else
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }

        public class TitleLibrary
        {
            public List<string> PrePrefix { get; set; }
            public List<string> Prefix { get; set; }
            public List<string> Suffix { get; set; }
            public List<string> SufSuffix { get; set; }

            public TitleLibrary()
            {
                PrePrefix = new List<string>();
                Prefix = new List<string>();
                Suffix = new List<string>();
                SufSuffix = new List<string>();
            }
        }

        public class PlayerTitleInfo
        {
            public string PrePrefix { get; set; } = "";
            public string Prefix { get; set; } = "";
            public string Suffix { get; set; } = "";
            public string SufSuffix { get; set; } = "";
        }
    }
}