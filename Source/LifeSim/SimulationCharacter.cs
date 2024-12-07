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

        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }

        [Column("age")]
        public int Age { get; set; }

        //Todo: add like, genes, personality, etc


        public SimulationCharacter() { }

        public SimulationCharacter(string firstName, string lastName, int age)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }
    }
}