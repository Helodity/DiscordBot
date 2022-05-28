using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public class GuildQuoteData : ModuleData {
        //Reference to the Json file's relative path
        public const string JsonLocation = "Json/Quotes.json";

        //Does this server have quoting enabled?
        [JsonProperty("enabled")]
        public bool Enabled;

        //Which channel to send quotes?
        [JsonProperty("quote_channel")]
        public ulong QuoteChannelId;

        //What emoji do we look for when quoting
        [JsonProperty("quote_emoji")]
        public ulong QuoteEmojiId;

        //How many of these emojis need to be added to quote a message
        [JsonProperty("emoji_amount_to_quote")]
        public ushort EmojiAmountToQuote;

        //List of already quoted messages
        [JsonProperty("quotes")]
        public List<Quote> Quotes;

        public GuildQuoteData(ulong id) : base(id) {
            Enabled = true;
            QuoteChannelId = 0;
            QuoteEmojiId = 0;
            EmojiAmountToQuote = 1;
            Quotes = new List<Quote>();
        }
    }
}