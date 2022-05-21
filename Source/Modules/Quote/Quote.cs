namespace DiscordBotRewrite.Modules;
public class Quote {

    //The message that the bot sends
    [JsonProperty("quote_message")]
    public ulong QuoteMessage;

    //The message being quoted by the bot
    [JsonProperty("original_message")]
    public ulong OriginalMessage;

    public Quote(ulong quote, ulong original) {
        QuoteMessage = quote;
        OriginalMessage = original;
    }
}
