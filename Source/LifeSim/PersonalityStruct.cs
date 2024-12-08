namespace DiscordBotRewrite.LifeSim
{
    //This is just an intermediate struct passed when generating new characters.
    //AKA idk if the SQL library works well with structs.
    public struct PersonalityStruct
    {
        public float Extraversion { get; set; }
        public float Openness { get; set; }
        public float Conscientiousness { get; set; }
        public float Agreeableness { get; set; }
        public float Neuroticism { get; set; }

        public PersonalityStruct(float extraversion, float openness, float conscientiousness, float agreeableness, float neuroticism)
        {
            Extraversion = extraversion;
            Openness = openness;
            Conscientiousness = conscientiousness;
            Agreeableness = agreeableness;
            Neuroticism = neuroticism;
        }

        public static PersonalityStruct GenerateRandomPersonality()
        {
            return new PersonalityStruct(
                GenerateRandomNumber(-1, 1.0f),
                GenerateRandomNumber(-1, 1.0f),
                GenerateRandomNumber(-1, 1.0f),
                GenerateRandomNumber(-1, 1.0f),
                GenerateRandomNumber(-1, 1.0f)
            );
        }
    }
}