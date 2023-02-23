using Newtonsoft.Json;

namespace DiscordBotRewrite {
    public readonly struct Config {
        #region Properties
        public const string JsonLocation = "Config.json";

        [JsonProperty("token")]
        public readonly string Token;

        [JsonProperty("use_voice")]
        public readonly bool UseVoice;

        [JsonProperty("text_logging")]
        public readonly bool TextLogging;

        [JsonProperty("debug_logging")]
        public readonly bool DebugLogging;
        #endregion
    }
}