using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Text;
using Terraria.ID;
using System.Configuration;
using static TitleLibrary.Configuration;

namespace TitleLibrary
{
    [ApiVersion(2, 1)]
    public class TitleLibrary : TerrariaPlugin
    {
        public override string Author => "35117";
        public override string Description => "TitleLibrary";
        public override string Name => "TitleLibrary";
        public override Version Version => new Version(1, 0, 0);
        static Random random = new Random();
        internal static Configuration Config = new();

        public TitleLibrary(Main game) : base(game)
        {
        }

        #region 初始化及卸载
        public override void Initialize()
        {
            LoadConfig();
            GeneralHooks.ReloadEvent += ReloadEvent;
            Commands.ChatCommands.Add(new Command(permissions: new List<string> { "TitleLibrary.GiveTitle", }, GiveTitle, "givetitle", "gt"));
            Commands.ChatCommands.Add(new Command(permissions: new List<string> { "TitleLibrary.ChangeTitle", }, ChangeTitle, "changetitle", "ct"));
            ServerApi.Hooks.ServerChat.Register(this, OnPlayerChat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //移除所有由本插件添加的所有指令
                TShockAPI.Hooks.GeneralHooks.ReloadEvent -= ReloadEvent;
                var asm = Assembly.GetExecutingAssembly();
                Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
                ServerApi.Hooks.ServerChat.Deregister(this, OnPlayerChat);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region 重读及加载配置
        private static void ReloadEvent(ReloadEventArgs e)
        {
            Config = Configuration.Read();
            e.Player?.SendSuccessMessage("[TitleLibrary] 重新加载配置完毕。");
            Config.Write();
        }
        private static void LoadConfig()
        {
            Config = Configuration.Read();
            Config.Write();
        }
        #endregion

        #region 玩家聊天事件
        private void OnPlayerChat(ServerChatEventArgs e)
        {
            try
            {
                if (ChatDetect(e))
                {
                    string NewChatContent = e.Text;
                    string preprefix = ""; // 根据配置设置preprefix
                    string prefix = ""; // 根据配置设置prefix
                    string PlayerName = TShock.Players[e.Who].Name; // 使用 Name 获取玩家名称
                    string suffix = ""; // 根据配置设置suffix
                    string sufsuffix = ""; // 根据配置设置sufsuffix

                    if (Config.PlayerTitleInfos.ContainsKey(PlayerName))
                    {
                        var titleInfo = Config.PlayerTitleInfos[PlayerName];
                        preprefix = string.Join(" ", titleInfo.PrePrefix);
                        prefix = string.Join(" ", titleInfo.Prefix);
                        suffix = string.Join(" ", titleInfo.Suffix);
                        sufsuffix = string.Join(" ", titleInfo.SufSuffix);
                    }

                    // 构建新的消息内容
                    string NewMessage = string.Format(Config.ChatFormat, preprefix, prefix, PlayerName, suffix, sufsuffix, NewChatContent);

                    // 发送新的消息
                    SendChat(e, NewChatContent, preprefix, prefix, PlayerName, suffix, sufsuffix);
                }
            }
            catch (FormatException ex)
            {
                TShock.Log.ConsoleError($"格式化聊天信息时发生错误: {ex.Message}");
                e.Handled = true;
            }
        }
        #endregion

        #region 发送消息
        private void SendChat(ServerChatEventArgs args, string ChatContent, string preprefix, string prefix, string PlayerName, string suffix, string sufsuffix)
        {
            if (ChatDetect(args))
            {
                // 使用配置中的格式字符串构建新的消息
                string format = Config.ChatFormat.Replace("{preprefix}", "{0}").Replace("{prefix}", "{1}").Replace("{PlayerName}", "{2}")
                    .Replace("{suffix}", "{3}").Replace("{sufsuffix}", "{4}").Replace("{ChatContent}", "{5}");
                string NewMessage = string.Format(format, preprefix, prefix, PlayerName, suffix, sufsuffix, ChatContent);

                // 发送新的消息给所有玩家
                TSPlayer.All.SendMessage(NewMessage, TShock.Players[args.Who].Group.R, TShock.Players[args.Who].Group.G, TShock.Players[args.Who].Group.B);

                // 将消息打印到控制台
                TSPlayer.Server.SendMessage(NewMessage, TShock.Players[args.Who].Group.R, TShock.Players[args.Who].Group.G, TShock.Players[args.Who].Group.B);

                // 标记为已处理，防止消息被再次发送
                args.Handled = true;
            }
        }
        #endregion

        #region 聊天检测
        private static bool ChatDetect(ServerChatEventArgs args)
        {
            // 检查消息是否以命令符号开头
            bool flag = (args.Text.Substring(0, 1) != TShock.Config.Settings.CommandSpecifier && args.Text.Substring(0, 1) != TShock.Config.Settings.CommandSilentSpecifier) || args.Text.Length == 1;
            if (flag)
            {
                TSPlayer player = TShock.Players[args.Who];
                bool flag2 = player.Group.Name != TShock.Config.Settings.DefaultGuestGroupName;
                if (flag2)
                {
                    bool flag3 = args.Text.Length > 500;
                    if (flag3)
                    {
                        player.SendErrorMessage("您发送的信息过长！");
                        args.Handled = true;
                        return false;
                    }
                    bool flag4 = !player.HasPermission(Permissions.canchat);
                    if (flag4)
                    {
                        player.SendErrorMessage("您没有聊天所需的权限\"tshock.canchat\"");
                        args.Handled = true;
                        return false;
                    }
                    bool mute = player.mute;
                    if (mute)
                    {
                        player.SendErrorMessage("您正被禁言中！");
                        args.Handled = true;
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 给予称号指令
        private static void GiveTitle(CommandArgs args)
        {
            string[] parameters = args.Parameters.ToArray();
            if (parameters.Length < 3)
            {
                args.Player.SendErrorMessage("使用例: givetitle <name> <prepre/pre/suf/suf> <title>");
                return;
            }

            string targetPlayerName = parameters[0];
            string type = parameters[1].ToLower();
            string title = string.Join(" ", parameters, 2, parameters.Length - 2);

            // 确保配置已加载
            LoadConfig();

            // 如果PlayerTitleInfos中不存在该玩家的条目，则初始化它
            if (!Config.PlayerTitleInfos.ContainsKey(targetPlayerName))
            {
                Config.PlayerTitleInfos[targetPlayerName] = new Configuration.PlayerTitleInfo();
            }

            // 获取或创建TitlesLibrary中的玩家条目
            if (!Config.TitlesLibrary.ContainsKey(targetPlayerName))
            {
                Config.TitlesLibrary[targetPlayerName] = new Configuration.TitleLibrary
                {
                    PrePrefix = new List<string>(),
                    Prefix = new List<string>(),
                    Suffix = new List<string>(),
                    SufSuffix = new List<string>()
                };
            }

            var titleEntry = Config.TitlesLibrary[targetPlayerName];
            switch (type)
            {
                case "prepre":
                    titleEntry.PrePrefix.Add(title);
                    break;
                case "pre":
                    titleEntry.Prefix.Add(title);
                    break;
                case "suf":
                    titleEntry.Suffix.Add(title);
                    break;
                case "sufsuf":
                    titleEntry.SufSuffix.Add(title);
                    break;
                default:
                    args.Player.SendErrorMessage("错误的输入. 请使用 'prepre', 'pre', 'suf', 或 'sufsuf'.");
                    return;
            }

            // 保存配置
            Config.Write();
            args.Player.SendSuccessMessage($"称号 '{title}' 被添加至 '{targetPlayerName}' 的 {type} 中");
        }
        #endregion

        #region 切换称号指令
        private static void ChangeTitle(CommandArgs args)
        {
            string[] parameters = args.Parameters.ToArray();
            if (parameters.Length < 1)
            {
                args.Player.SendErrorMessage("使用例: changetitle <prepre/pre/suf/suf> <num/list>");
                return;
            }

            string type = parameters[0].ToLower();
            int num = 0;
            bool listTitles = false;

            // 如果第二个参数是 "list"，则设置 listTitles 为 true
            if (parameters.Length > 1 && parameters[1].ToLower() == "list")
            {
                listTitles = true;
            }
            else if (parameters.Length > 1 && !int.TryParse(parameters[1], out num) || num < 0)
            {
                args.Player.SendErrorMessage("错误的数字参数.");
                return;
            }

            LoadConfig(); // 确保配置已加载

            if (!Config.TitlesLibrary.ContainsKey(args.Player.Name))
            {
                args.Player.SendErrorMessage($"'{args.Player.Name}' 没有称号.");
                return;
            }

            var titleEntry = Config.TitlesLibrary[args.Player.Name];
            if (listTitles)
            {
                // 显示preprefix列表
                if (type == "prepre" && titleEntry.PrePrefix.Count > 0)
                {
                    ShowTitles(args.Player, titleEntry.PrePrefix, "前前缀");
                }
                // 显示prefix列表
                else if (type == "pre" && titleEntry.Prefix.Count > 0)
                {
                    ShowTitles(args.Player, titleEntry.Prefix, "前缀");
                }
                // 显示suffix列表
                else if (type == "suf" && titleEntry.Suffix.Count > 0)
                {
                    ShowTitles(args.Player, titleEntry.Suffix, "后缀");
                }
                // 显示sufsuffix列表
                else if (type == "sufsuf" && titleEntry.SufSuffix.Count > 0)
                {
                    ShowTitles(args.Player, titleEntry.SufSuffix, "后后缀");
                }
                else
                {
                    args.Player.SendErrorMessage($"没有可用的 '{type}'.");
                }
            }
            else if (num == 0)
            {
                // 将玩家的 PlayerTitleInfos 对应的prefix或suffix设置为默认的空字符串
                if (type == "prepre")
                {
                    Config.PlayerTitleInfos[args.Player.Name].PrePrefix = "";
                }
                else if (type == "pre")
                {
                    Config.PlayerTitleInfos[args.Player.Name].Prefix = "";
                }
                else if (type == "suf")
                {
                    Config.PlayerTitleInfos[args.Player.Name].Suffix = "";
                }
                else if (type == "sufsuf")
                {
                    Config.PlayerTitleInfos[args.Player.Name].SufSuffix = "";
                }
                args.Player.SendSuccessMessage($"你的 {type} 已被清空.");
            }
            else
            {
                // 应用prefix或suffix
                string title = "";
                switch (type)
                {
                    case "prepre":
                        if (titleEntry.PrePrefix.Count >= num)
                        {
                            title = titleEntry.PrePrefix[num - 1];
                            Config.PlayerTitleInfos[args.Player.Name].PrePrefix = title;
                        }
                        break;
                    case "pre":
                        if (titleEntry.Prefix.Count >= num)
                        {
                            title = titleEntry.Prefix[num - 1];
                            Config.PlayerTitleInfos[args.Player.Name].Prefix = title;
                        }
                        break;
                    case "suf":
                        if (titleEntry.Suffix.Count >= num)
                        {
                            title = titleEntry.Suffix[num - 1];
                            Config.PlayerTitleInfos[args.Player.Name].Suffix = title;
                        }
                        break;
                    case "sufsuf":
                        if (titleEntry.SufSuffix.Count >= num)
                        {
                            title = titleEntry.SufSuffix[num - 1];
                            Config.PlayerTitleInfos[args.Player.Name].SufSuffix = title;
                        }
                        break;
                    default:
                        args.Player.SendErrorMessage("错误的种类. 请使用 'prepre', 'pre', 'suf', 或 'sufsuf'.");
                        return;
                }

                if (string.IsNullOrEmpty(title))
                {
                    args.Player.SendErrorMessage($"错误的序号.'{type}' 没有对应序号的称号.");
                    return;
                }

                Config.Write(); // 保存配置
                args.Player.SendSuccessMessage($"你的 {type} 已被更改为 '{title}'.");
            }
        }
        #endregion

        #region 展示称号
        private static void ShowTitles(TSPlayer player, List<string> titles, string titleType)
        {
            if (titles.Count == 0)
            {
                player.SendMessage($"你没有 {titleType} .",255,255,255);
                return;
            }

            player.SendInfoMessage($"{titleType}列表:");
            for (int i = 0; i < titles.Count; i++)
            {
                player.SendMessage($"{i + 1}: {titles[i]}",255,255,255);
            }
        }
        #endregion
    }
}