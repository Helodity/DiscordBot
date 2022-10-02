using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("guild_quote_data")]
    public class GuildQuoteData {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Unique, Column("guild_id")]
        public long GuildId { get; set; }

        //Does this server have quoting enabled?
        [Column("enabled")]
        public bool Enabled { get; set; }

        //Which channel to send quotes?
        [Column("quote_channel")]
        public long ChannelId { get; set; }

        //What emoji do we look for when quoting
        [Column("emoji_id")]
        public long EmojiId { get; set; }
        [Column("emoji_name")]
        public string EmojiName { get; set; }

        //How many of these emojis need to be added to quote a message
        [Column("emoji_amount")]
        public short EmojiAmount { get; set; }
        public GuildQuoteData() {}
        public GuildQuoteData(long id) {
            GuildId = id;
            Enabled = true;
            ChannelId = 0;
            EmojiId = 0;
            EmojiName = "";
            EmojiAmount = 1;
        }
    }
}