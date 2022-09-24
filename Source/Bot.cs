using System;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotRewrite.Commands;
using DiscordBotRewrite.Global;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using SQLite;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite {
    public static class Bot {
        #region Debug Specifics
#if DEBUG
        static readonly bool Debugging = true;
        static readonly ulong? TargetServer = 941558436561305630; //This is my private testing server, if you want to debug the bot you'll have to manually change this as any release won't use this code
#else
        static readonly bool Debugging = false;
        static readonly ulong? TargetServer = null;
#endif
        #endregion

        #region Properties
        public static DiscordClient Client { get; private set; } //will need to change for sharding, deal with when that becomes important
        public static SlashCommandsExtension SlashExtension { get; private set; }
        public static Config Config { get; private set; }
        public static SQLiteConnection Database { get; private set; }
        public static ModuleContainer Modules { get; private set; }
        public static Style Style { get; private set; }
        #endregion

        #region Public
        public static async Task Start() {
            if(!TryLoadConfig()) return;
            await InitClient();
            Database = new SQLiteConnection("SavedData.db3");
            Modules = new ModuleContainer(Client);
            Style = new Style();
            await InitCommands();
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
        #endregion

        #region Private
        private static bool TryLoadConfig() {
            Config = LoadJson<Config>(Config.JsonLocation);

            if(string.IsNullOrWhiteSpace(Config.Token)) {
                Console.WriteLine("No token is set! Set one in Json/Config.json");
                SaveJson(Config, Config.JsonLocation);
                Console.ReadLine();
                return false;
            }
            return true;
        }
        private static Task InitClient() {
            var config = new DiscordConfiguration {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Debugging ? LogLevel.Debug : LogLevel.Information
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.UseInteractivity(new InteractivityConfiguration() {
                Timeout = TimeSpan.FromMinutes(2)
            });
            return Task.CompletedTask;
        }
        private static Task InitCommands() {
            SlashExtension = Client.UseSlashCommands();

            SlashExtension.RegisterCommands<UnsortedCommands>(TargetServer);
            SlashExtension.RegisterCommands<PixelCommands>(TargetServer);
            SlashExtension.RegisterCommands<QuestionCommands>(TargetServer);
            SlashExtension.RegisterCommands<QuoteCommands>(TargetServer);
            SlashExtension.RegisterCommands<PollCommands>(TargetServer);
            SlashExtension.RegisterCommands<EconomyCommands>(TargetServer);
            if(Config.UseVoice) SlashExtension.RegisterCommands<VoiceCommands>(TargetServer);

            return Task.CompletedTask;
        }
        private static async Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
            Assembly thisAssem = typeof(Bot).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();

            await Client.UpdateStatusAsync(new DiscordActivity() {
                ActivityType = ActivityType.Playing,
                Name = Debugging ? $"Version {thisAssemName.Version}-Debug" : $"Version {thisAssemName.Version}"
            }, UserStatus.Online);
            Client.Logger.Log(LogLevel.Debug, "Bot has started!");
        }
        #endregion
    }
}