using System;
using SQLite;
using static DiscordBotRewrite.Modules.PixelModule;

namespace DiscordBotRewrite.Modules {
    [Table("place_cooldown")]
    public class PlaceCooldown {
        [PrimaryKey, AutoIncrement, Unique, Column("id")]
        public long Id { get; set; }
        [Column("user_id")]
        public long UserID { get; set; }
        [Column("guild_id")]
        public long GuildID { get; set; }
        [Column("expiration_time")]
        public DateTime EndTime { get; set; }

        public PlaceCooldown() { }

        public PlaceCooldown(long guildID, long userID) {
            UserID = userID;
            GuildID = guildID;
            EndTime = DateTime.Now;
        }
    }
}