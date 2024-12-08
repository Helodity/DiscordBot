using SQLite;

namespace DiscordBotRewrite.LifeSim
{
    [Table("guild_simulation_data")]
    public class GuildSimulationData
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Unique, Column("guild_id")]
        public long GuildId { get; set; }
        //Is the simulation updating
        [Column("is_paused")]
        public bool SimulationRunning { get; set; }


        public GuildSimulationData()
        {
            SimulationRunning = true;
        }

        public GuildSimulationData(long guildId)
        {
            GuildId = guildId;
            SimulationRunning = true;
        }


        public List<SimulationCharacter> GetAllCharacters()
        {
            return Bot.Database.Table<SimulationCharacter>().Where(x => x.GuildID == GuildId).ToList();
        }

    }
}