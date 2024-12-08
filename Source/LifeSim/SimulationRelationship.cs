using SQLite;

namespace DiscordBotRewrite.LifeSim
{
    //This does not need a guildID, since each character contains a reference to the guild they are in.
    [Table("simulation_relationships")]
    public class SimulationRelationship
    {
        //Unique identifiter for SQL. 
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        //Who feels something towards someone
        [Column("owner_id")]
        public int OwnerID { get; set; }

        //Who someone feels something towards
        [Column("target_id")]
        public int TargetID { get; set; }

        [Column("friendship")]
        public float Friendship { get; set; }


        //Todo: add like, more relationship stuff
        public SimulationRelationship() { }

        public SimulationRelationship(int ownerID, int targetID)
        {
            OwnerID = ownerID;
            TargetID = targetID;
            Friendship = 0;
        }
    }
}