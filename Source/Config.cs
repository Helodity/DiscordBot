using Newtonsoft.Json;

namespace DiscordBotRewrite {
    public readonly struct Config {
        #region Properties
        public const string JsonLocation = "Config.json";

        [JsonProperty("token")]
        public readonly string Token;

        //Lavalink violates terms of service, so allow an option to disable it
        [JsonProperty("use_voice")]
        public readonly bool UseVoice;

        [JsonProperty("text_logging")]
        public readonly bool TextLogging;
        #endregion
    }
}