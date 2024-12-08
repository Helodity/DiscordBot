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

        public string GetQuantizedString()
        {

            List<SimulationRelationship> relationships = Bot.Database.Table<SimulationRelationship>().Where(x => x.OwnerID == OwnerID).ToList();
            int BestFriendID = -1;
            float BestRelationship = 0;
            for (int i = 0; i < relationships.Count(); i++)
            {
                if (relationships[i].Friendship > Math.Max(50, BestRelationship))
                {
                    BestFriendID = relationships[i].TargetID;
                    BestRelationship = relationships[i].Friendship;
                }
            }
            if (BestFriendID == TargetID)
            {
                return "Best Friends";
            }

            switch (Friendship)
            {
                case <= -25:
                    return "Hatred";
                case >= -25 and <= -2:
                    return "Dislike";
                case >= -2 and <= 2:
                    return "Acquaintance";
                case > 2 and <= 25:
                    return "Casual Friends";
                case > 25 and <= 50:
                    return "Close Friends";
                case > 50:
                    return "Intimate Friends";
                default:
                    return "Acquaintance";
            }
        }
    }
}