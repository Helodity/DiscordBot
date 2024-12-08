using SQLite;

namespace DiscordBotRewrite.LifeSim
{
    //Info about a character's relationships is stored elsewhere
    [Table("simulation_characters")]
    public class SimulationCharacter
    {
        //Unique identifiter for SQL. 
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        //Which guild the character belongs to
        [Column("guild_id")]
        public long GuildID { get; set; }

        //General Info
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        [Column("age")]
        public int AgeTicks { get; set; }

        //Todo: add like, genes, personality, etc
        //Sim Info

        [Column("current_location")]
        public string Current_Location { get; set; }
        [Column("current_location_duration")]
        public int Current_Location_Duration { get; set; }

        //Personality
        //Each of these ranges from -1 to 1
        //Impacts odds of like hanging out in general
        [Column("personality_extraversion")]
        public float Personality_Extraversion { get; set; }
        //Increases randomness of visit locations
        [Column("personality_openness")]
        public float Personality_Openness { get; set; }
        //Uhhh this might be used for something
        [Column("personality_conscientiousness")]
        public float Personality_Conscientiousness { get; set; }
        //Impacts odds of positive events
        [Column("personality_agreeableness")]
        public float Personality_Agreeableness { get; set; }
        //Impacts scale of negative interactions
        [Column("personality_neuroticism")]
        public float Personality_Neuroticism { get; set; }

        [Column("interests")]
        public SimulationInterests Interests { get; set; }

        //Genes
        //How long until they begin to have a chance of dying. 
        [Column("healthy_lifespan")]
        public int HealthyLifespanYears { get; set; }


        public SimulationCharacter() { }

        public SimulationCharacter(long guildID, string firstName, string lastName, int age, int healthyLifespan, PersonalityStruct personality, SimulationInterests interests)
        {
            GuildID = guildID;
            FirstName = firstName;
            LastName = lastName;
            AgeTicks = age;
            HealthyLifespanYears = healthyLifespan;

            Personality_Agreeableness = personality.Agreeableness;
            Personality_Conscientiousness = personality.Conscientiousness;
            Personality_Extraversion = personality.Extraversion;
            Personality_Neuroticism = personality.Neuroticism;
            Personality_Openness = personality.Openness;

            Interests = interests;
            Current_Location = "None";
            Current_Location_Duration = 0;
        }

        //Todo: Constructor that takes 2 other characters in as input
    }
}