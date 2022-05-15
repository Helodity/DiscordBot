namespace DiscordBotRewrite;

public readonly struct GlobalConfig {
    public const string JsonLocation = "Json/Config.json";

    [JsonProperty("token")]
    public readonly string Token;
    [JsonProperty("bot_owner")]
    public readonly ulong OwnerId;
}