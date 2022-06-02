using System;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotRewrite.Commands;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite {
    public class Bot {
        #region Debug Specifics
#if DEBUG
        public bool Debugging = true;
        public ulong? TargetServer = 941558436561305630; //This is my private testing server, if you want to debug the bot you'll have to manually change this as any release won't use this code
#else
    public bool Debugging = false;
    public ulong? TargetServer = null;
#endif
        #endregion

        #region Properties
        public static DiscordClient Client { get; private set; } //will need to change for sharding, deal with when that becomes important
        public static SlashCommandsExtension SlashExtension { get; private set; }
        public static Config Config { get; private set; }
        public static ModuleContainer Modules { get; private set; }
        #endregion

        #region Public
        public async Task Start() {
            if(!TryLoadConfig()) return;
            await InitClient();
            Modules = new ModuleContainer(Client);
            await InitCommands();
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
        #endregion

        #region Private
        private bool TryLoadConfig() {
            Config = LoadJson<Config>(Config.JsonLocation);

            if(string.IsNullOrWhiteSpace(Config.Token)) {
                Console.WriteLine("No token is set! Set one in Json/Config.json");
                SaveJson(Config, Config.JsonLocation);
                Console.ReadLine();
                return false;
            }
            return true;
        }
        private Task InitClient() {
            var config = new DiscordConfiguration {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Debugging ? LogLevel.Debug : LogLevel.Information
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.UseInteractivity(new InteractivityConfiguration() {
                Timeout = TimeSpan.FromSeconds(30)
            });
            return Task.CompletedTask;
        }
        private Task InitCommands() {
            SlashExtension = Client.UseSlashCommands();

            SlashExtension.RegisterCommands<UnsortedCommands>(TargetServer);
            SlashExtension.RegisterCommands<PixelCommands>(TargetServer);
            SlashExtension.RegisterCommands<QuestionCommands>(TargetServer);
            SlashExtension.RegisterCommands<QuoteCommands>(TargetServer);
            SlashExtension.RegisterCommands<PollCommands>(TargetServer);
            if(Config.UseVoice) SlashExtension.RegisterCommands<VoiceCommands>(TargetServer);

            return Task.CompletedTask;
        }
        private async Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
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