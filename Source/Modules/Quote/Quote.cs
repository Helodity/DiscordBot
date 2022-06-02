using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public readonly struct Quote {
        #region Properties
        //The message that the bot sends
        [JsonProperty("quote_message")]
        public readonly ulong QuoteMessage;

        //The message being quoted by the bot
        [JsonProperty("original_message")]
        public readonly ulong OriginalMessage;
        #endregion

        #region Constructors
        public Quote(ulong quote, ulong original) {
            QuoteMessage = quote;
            OriginalMessage = original;
        }
        #endregion
    }
}