using DiscordBotRewrite.General;
using DiscordBotRewrite.Global;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using SQLite;


namespace DiscordBotRewrite
{
    public static class Bot
    {
        #region Debug Specifics
#if DEBUG
        private static readonly ulong? TargetServer = 1312649782216364103; //This is my private testing server, if you want to debug the bot you'll have to manually change this as any release won't use this code
#else
        private static readonly ulong? TargetServer = null;
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
        public static async Task Start()
        {
            if (!TryLoadConfig())
            {
                return;
            }

            await InitClient();
            Database = new SQLiteConnection("SavedData.db3");
            Modules = new ModuleContainer(Client);
            Style = new Style();
            await InitCommands();
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
        public static void LogException(Exception e, string source = "")
        {
            string text = $"\n" +
                $"{DateTime.Now} | {source}\n" +
                $"MESSAGE: {e.Message}\n" +
                $"STACK TRACE:{e.StackTrace}\n" +
                $"INNER EXCEPTION:{e.InnerException}\n" +
                $"TARGETSITE:{e.TargetSite}";
            Client.Logger.LogCritical(text);
            if (Config.TextLogging)
            {
                File.AppendAllText("log.txt", text);
            }
        }
        #endregion

        #region Private
        private static bool TryLoadConfig()
        {
            Config = LoadJson<Config>(Config.JsonLocation);

            if (string.IsNullOrWhiteSpace(Config.Token))
            {
                Console.WriteLine("No token is set! Set one in Json/Config.json");
                SaveJson(Config, Config.JsonLocation);
                Console.ReadLine();
                return false;
            }
            return true;
        }
        private static Task InitClient()
        {
            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.MessageContents | DiscordIntents.AllUnprivileged,
                AutoReconnect = true,
                MinimumLogLevel = Config.DebugLogging ? LogLevel.Debug : LogLevel.Information
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.ClientErrored += (sender, args) =>
            {
                LogException(args.Exception, "ClientError");
                return Task.CompletedTask;
            };
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
            return Task.CompletedTask;
        }
        private static Task InitCommands()
        {
            SlashExtension = Client.UseSlashCommands();

            SlashExtension.RegisterCommands<UnsortedCommands>(TargetServer);

            SlashExtension.SlashCommandErrored += (sender, args) =>
            {
                LogException(args.Exception, "SlashCommandError");
                return Task.CompletedTask;
            };

            return Task.CompletedTask;
        }
        private static async Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            await Client.UpdateStatusAsync(new DiscordActivity()
            {
                ActivityType = ActivityType.Playing,
                Name = Config.DebugLogging ? $"Version {VersionString}-Debug" : $"Version {VersionString}"
            }, UserStatus.Online);
            Client.Logger.Log(LogLevel.Debug, "Bot has started!");
        }
        #endregion
    }
}