using SQLite;

namespace DiscordBotRewrite.Poll
{
    [Table("guild_poll_data")]
    public class GuildPollData
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Unique, Column("guild_id")]
        public long GuildId { get; set; }
        //Which channel to send polls?
        [Column("poll_channel")]
        public long PollChannelId { get; set; }


        public GuildPollData()
        {
            PollChannelId = 0;
        }

        public GuildPollData(long guildId)
        {
            GuildId = guildId;
        }
    }
}