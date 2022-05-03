﻿namespace DiscordBotRewrite;

public class Bot {
#if DEBUG
    public bool Debugging = true;
        public ulong? TargetServer = 941558436561305630;
#else
        public bool Debugging = false;
        public ulong? DebugServer = null;
#endif
    public static DiscordClient Client { get; private set; } //will need to change for sharding, deal with when that becomes important
    public static SlashCommandsExtension SlashExtension { get; private set; }
    public static GlobalConfig Config { get; private set; }
    public static ModuleContainer Modules { get; private set; }

    public async Task Start() {
        if(!TryLoadConfig()) return;
        await InitClient();
        await InitModules();
        await InitCommands();
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    #region private
    private bool TryLoadConfig() {
        Config = BotUtils.LoadJson<GlobalConfig>(GlobalConfig.JsonLocation);

        if(string.IsNullOrEmpty(Config.Token)) {
            Console.WriteLine("No token is set! Set one in Json/Config.json");
            BotUtils.SaveJson(Config, GlobalConfig.JsonLocation);
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
    private async Task InitModules() {
        Modules = new ModuleContainer();

        Modules.Quote = new QuoteModule(Client);

        Modules.Question = new QuestionModule();

        Modules.Pixel = new PixelModule();

        Modules.Voice = new VoiceModule(Client);
        await Modules.Voice.EnableLavalink();
    }
    private Task InitCommands() {
        SlashExtension = Client.UseSlashCommands();

        SlashExtension.RegisterCommands<UnsortedCommands>(TargetServer);

        SlashExtension.RegisterCommands<PixelCommands>(TargetServer);

        SlashExtension.RegisterCommands<QuestionCommands>(TargetServer);

        SlashExtension.RegisterCommands<VoiceCommands>(TargetServer);

        SlashExtension.RegisterCommands<QuoteCommands>(TargetServer);

        return Task.CompletedTask;
    }
    private async Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
        await Client.UpdateStatusAsync(new DiscordActivity() {
            ActivityType = ActivityType.Playing,
            Name = Debugging ? "with my code!": "with new Slash Commands!"
        }, UserStatus.Online);
        Client.Logger.Log(LogLevel.Debug, "Bot has started!");
    }
    #endregion
}

public class ModuleContainer {
    public VoiceModule Voice;

    public QuoteModule Quote;

    public PixelModule Pixel;

    public QuestionModule Question;
}

public readonly struct GlobalConfig {
    public const string JsonLocation = "Json/Config.json";

    [JsonProperty("token")]
    public readonly string Token;
    [JsonProperty("bot_owner")]
    public readonly ulong OwnerId;
}