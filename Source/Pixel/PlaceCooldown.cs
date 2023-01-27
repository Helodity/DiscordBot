using SQLite;

namespace DiscordBotRewrite.Pixel
{
    [Table("place_cooldown")]
    public class PlaceCooldown
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public long UserID { get; set; }
        [Column("guild_id")]
        public long GuildID { get; set; }
        [Column("expiration_time")]
        public DateTime EndTime { get; set; }

        public PlaceCooldown() { }

        public PlaceCooldown(long guildID, long userID)
        {
            UserID = userID;
            GuildID = guildID;
            EndTime = DateTime.Now;
        }
    }
}