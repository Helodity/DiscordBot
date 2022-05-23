namespace DiscordBotRewrite;

public readonly struct Config {
    public const string JsonLocation = "Config.json";

    [JsonProperty("token")]
    public readonly string Token;

    //Lavalink violates terms of service, so allow an option to disable it
    [JsonProperty("use_voice")]
    public readonly bool UseVoice;
}