using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;
using Terraria;
using TerrariaApi;
using static Terraria.Player;

namespace TitleLibrary
{
    internal class Configuration
    {
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "TitleLibrary.json");

        [JsonProperty("玩家目前称号信息")]
        public Dictionary<string, PlayerTitleInfo> PlayerTitleInfos { get; set; } = new Dictionary<string, PlayerTitleInfo>();

        [JsonProperty("聊天格式")]
        public string ChatFormat { get; set; } = "{6}{0}{1}{2}:{5}{3}{4}"; // 默认格式

        [JsonProperty("额外内容")]
        public string ExtraContent { get; set; } = "";

        [JsonProperty("称号存储库")]
        public Dictionary<string, TitleLibrary> TitlesLibrary { get; set; } = new Dictionary<string, TitleLibrary>();

        [JsonProperty("自定义占位符内容-数据库")]
        public Dictionary<string, DatabasePlaceholder> DatabasePlaceholders { get; set; } = new Dictionary<string, DatabasePlaceholder>();
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
                newConfig.Initialize();
                newConfig.Write();
                TShock.Log.ConsoleError("[TitleLibrary] 未找配置文件，已新建预设");
                return newConfig;
            }
            else
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        public class DatabasePlaceholder
        {
            public string TableName { get; set; } = "";
            public string ColumnName { get; set; } = "";
            public string PlayerColumn { get; set; } = "";
        }

        public class TitleLibrary
        {
            public List<string> PrePrefix { get; set; } = new List<string>();
            public List<string> Prefix { get; set; } = new List<string>();
            public List<string> Suffix { get; set; } = new List<string>();
            public List<string> SufSuffix { get; set; } = new List<string>();
        }

        public class PlayerTitleInfo
        {
            public string PrePrefix { get; set; } = "";
            public string Prefix { get; set; } = "";
            public string Suffix { get; set; } = "";
            public string SufSuffix { get; set; } = "";
        }
        public void Initialize()
        {
            ChatFormat = "{6}{0}{1}{2}:{5}{3}{4}"; // 默认格式
            ExtraContent = "";
            PlayerTitleInfos = new Dictionary<string, PlayerTitleInfo>
            {
                {"",new PlayerTitleInfo() }
            };
            TitlesLibrary = new Dictionary<string, TitleLibrary>
            {
                {"",new TitleLibrary() } 
            };
            DatabasePlaceholders = new Dictionary<string, DatabasePlaceholder>
            {
                { "death.count", new DatabasePlaceholder { TableName = "Death", ColumnName = "Count", PlayerColumn = "Name" } },
                { "eco.money", new DatabasePlaceholder { TableName = "Economics", ColumnName = "Currency", PlayerColumn = "UserName" } },
                { "online.duration", new DatabasePlaceholder { TableName = "OnlineDuration", ColumnName = "duration", PlayerColumn = "username" } },
                { "eco.level", new DatabasePlaceholder { TableName = "RPG", ColumnName = "Level", PlayerColumn = "UserName" } },
                { "zhipm.time", new DatabasePlaceholder { TableName = "Zhipm_PlayerExtra", ColumnName = "time", PlayerColumn = "Name" } },
                { "zhipm.killnpcnum", new DatabasePlaceholder { TableName = "Zhipm_PlayerExtra", ColumnName = "killNPCnum", PlayerColumn = "Name" } },
                { "zhipm.point", new DatabasePlaceholder { TableName = "Zhipm_PlayerExtra", ColumnName = "point", PlayerColumn = "Name" } },
                { "zhipm.deathcount", new DatabasePlaceholder { TableName = "Zhipm_PlayerExtra", ColumnName = "deathcount", PlayerColumn = "Name" } },
            };
        }
    }
}