using DiscordBotRewrite.Global;

namespace DiscordBotRewrite.LifeSim
{
    public class LifeSimModule
    {
        public TimeBasedEvent UpdateEvent;
        private List<string> PotentialNames;
        public readonly List<SimulationLocation> Locations;

        public const int TICKS_PER_YEAR = 1440;
        #region Constructors
        public LifeSimModule()
        {
            //Setup database
            Bot.Database.CreateTable<SimulationCharacter>();
            Bot.Database.CreateTable<SimulationRelationship>();
            Bot.Database.CreateTable<GuildSimulationData>();

            //Start ticking
            UpdateEvent = new TimeBasedEvent(TimeSpan.FromMinutes(1), () => { TickSimulations(); UpdateEvent.Start(); });
            UpdateEvent.Start();

            LoadPotentialNames();

            Locations = LoadJson<List<SimulationLocation>>(SimulationLocation.JsonLocation);

            if (Locations.Count == 0)
            {
                SimulationLocation dummy =
                    new SimulationLocation(
                        "DummyLocation", SimulationInterests.Gaming | SimulationInterests.Animals, 0
                    );
                Locations.Add(dummy);
                SaveJson(Locations, SimulationLocation.JsonLocation);
            }
        }
        #endregion

        public GuildSimulationData GetSimulationData(ulong guildID)
        {
            GuildSimulationData simData = Bot.Database.Table<GuildSimulationData>().FirstOrDefault(x => x.GuildId == (long)guildID);
            if (simData == null)
            {
                simData = new GuildSimulationData((long)guildID);
                Bot.Database.Insert(simData);
            }
            return simData;
        }

        public SimulationRelationship GetRelationshipData(SimulationCharacter owner, SimulationCharacter target)
        {
            SimulationRelationship relationshipData = Bot.Database.Table<SimulationRelationship>().FirstOrDefault(x => x.OwnerID == owner.Id && x.TargetID == target.Id);
            if (relationshipData == null)
            {
                relationshipData = new SimulationRelationship(owner.Id, target.Id);
                Bot.Database.Insert(relationshipData);
            }
            return relationshipData;
        }
        public SimulationLocation? GetSimulationLocation(string locationName)
        {
            foreach (SimulationLocation loc in Locations)
            {
                if (loc.Name == locationName)
                {
                    return loc;
                }
            }
            return null;
        }

        public string GetRandomName()
        {
            return PotentialNames[GenerateRandomNumber(0, PotentialNames.Count - 1)];
        }

        private void LoadPotentialNames()
        {
            PotentialNames = LoadJson<List<string>>("Json/LifeSim/names.json");

            if (PotentialNames.Count == 0)
            {
                PotentialNames.Add("Names need to be added in the config!");
                SaveJson(PotentialNames, "Json/LifeSim/names.json");
            }

        }

        private void TickSimulations()
        {
            foreach (GuildSimulationData data in Bot.Database.Table<GuildSimulationData>().ToList())
            {
                if (!data.SimulationRunning)
                {
                    continue;
                }

                TickSimulation(data);

            }
        }

        private void TickSimulation(GuildSimulationData data)
        {
            //See if someone new moves in. 
            TryForCharacterMoveIn(data);
            //TODO: Limit actions based on age
            DoCharacterAging(data);

            List<SimulationCharacter> charList = data.GetAllCharacters();

            //Moving around n stuff
            //TODO add visiting friends.
            for (int i = 0; i < charList.Count(); i++)
            {
                SimulationCharacter c = charList[i];
                c.Current_Location_Duration += 1;
                if (c.Current_Location == "None")
                {
                    //Go somewhere!
                    int goOutsideInverseOdds = 45 + (int)(10 * c.Personality_Extraversion);

                    if (GenerateRandomNumber(1, goOutsideInverseOdds) > 1)
                    {
                        Bot.Database.Update(c);
                        continue;
                    }

                    float maxAppeal = 0;
                    int topLocationIndex = -1;

                    for (int loc = 0; loc < Locations.Count(); loc++)
                    {
                        SimulationLocation location = Locations[loc];
                        float appeal = 0;
                        //Matching interests
                        int totalAppeals = 0;
                        int matchingAppeals = 0;
                        foreach (Enum value in Enum.GetValues(typeof(SimulationInterests)))
                        {
                            if (location.Appeals.HasFlag(value))
                            {
                                totalAppeals++;
                                if (c.Interests.HasFlag(value))
                                {
                                    matchingAppeals++;
                                }
                            }
                        }
                        appeal += 15 * (matchingAppeals * 2 / (totalAppeals + 1));
                        //Matching intensity
                        appeal += (1 - Math.Abs(c.Personality_Extraversion - location.Intensity)) * 20;

                        //Random chance
                        appeal += (3 + c.Personality_Openness * 2) * 7 * GenerateRandomNumber(-1, 1f);

                        if (appeal > maxAppeal || topLocationIndex == -1)
                        {
                            maxAppeal = appeal;
                            topLocationIndex = loc;
                        }
                    }
                    SimulationLocation newLocation = Locations[topLocationIndex];
                    c.Current_Location = newLocation.Name;
                    c.Current_Location_Duration = 0;
                }
                else
                {
                    int goInsideInverseOdds = 120 - ((int)(40 * c.Personality_Extraversion) + c.Current_Location_Duration);
                    if (goInsideInverseOdds < 10)
                    {
                        goInsideInverseOdds = 10;
                    }

                    if (GenerateRandomNumber(1, goInsideInverseOdds) > 1)
                    {
                        Bot.Database.Update(c);
                        continue;
                    }

                    c.Current_Location = "None";
                    c.Current_Location_Duration = 0;

                }
                Bot.Database.Update(c);
            }

            for (int i = 0; i < Locations.Count(); i++)
            {
                SimulationLocation location = Locations[i];
                List<SimulationCharacter> charsAtLocation =
                Bot.Database.Table<SimulationCharacter>().Where(x => x.Current_Location == location.Name).ToList();

                if (charsAtLocation.Count() < 2)
                {
                    continue;
                }

                for (int j = 0; j < charsAtLocation.Count(); j++)
                {
                    for (int k = j; k < charsAtLocation.Count(); k++)
                    {
                        DoCharacterInteraction(charsAtLocation[j], charsAtLocation[k]);
                    }
                }

            }
        }

        private void DoCharacterInteraction(SimulationCharacter c1, SimulationCharacter c2)
        {
            if (c1.Id == c2.Id)
            {
                return;
            }

            float appeal = 0;
            //Matching / Conflicting interests
            foreach (Enum value in Enum.GetValues(c1.Interests.GetType()))
            {
                if (c1.Interests.HasFlag(value) && c2.Interests.HasFlag(value))
                {
                    appeal += 10;
                }
                if (!c1.Interests.HasFlag(value) && c2.Interests.HasFlag(value))
                {
                    appeal -= 5;
                }
                if (c1.Interests.HasFlag(value) && !c2.Interests.HasFlag(value))
                {
                    appeal -= 5;
                }
            }
            appeal += (c1.Personality_Agreeableness + c2.Personality_Agreeableness) * 10;
            appeal -= (c1.Personality_Neuroticism + c1.Personality_Neuroticism) * 5;

            //Very slight positive lean.
            appeal += GenerateRandomNumber(-20, 25);

            SimulationRelationship relationship1 = GetRelationshipData(c1, c2);
            float r1Appeal = appeal;
            r1Appeal += relationship1.Friendship * 0.1f;
            if (r1Appeal < 0)
            {
                r1Appeal *= 1 + (c1.Personality_Neuroticism * 0.8f);
            }
            relationship1.Friendship += r1Appeal / 100;
            Bot.Database.Update(relationship1);

            SimulationRelationship relationship2 = GetRelationshipData(c2, c1);
            float r2Appeal = appeal;
            r2Appeal += relationship2.Friendship * 0.1f;
            if (r2Appeal < 0)
            {
                r2Appeal *= 1 + (c2.Personality_Neuroticism * 0.8f);
            }
            relationship2.Friendship += r2Appeal / 100;
            Bot.Database.Update(relationship2);
        }

        private void TryForCharacterMoveIn(GuildSimulationData data)
        {
            //A new character will move in every tick with a 1/x chance.
            int newCharacterInverseOdds = 50 * data.GetAllCharacters().Count() + 1;

            if (GenerateRandomNumber(1, newCharacterInverseOdds) > 1)
            {
                return;
            }

            //Someone new is moving in!!!
            SimulationCharacter newChar = GenerateRandomCharacter(data.GuildId);
            Bot.Database.Insert(newChar);
        }

        private void DoCharacterAging(GuildSimulationData data)
        {
            List<SimulationCharacter> charList = data.GetAllCharacters();

            //Manage Aging
            for (int i = 0; i < charList.Count(); i++)
            {
                SimulationCharacter c = charList[i];
                c.AgeTicks += 1;

                if (TicksToYears(c.AgeTicks) > c.HealthyLifespanYears)
                {
                    //This one is old, they got a chance of dying each tick!
                    //Potential TODO: Replace chance of dying with chance of getting health issue
                    int deathInverseOdds = (TICKS_PER_YEAR * 15) - TICKS_PER_YEAR * (TicksToYears(c.AgeTicks) - c.HealthyLifespanYears) + 1;
                    if (deathInverseOdds < 10)
                    {
                        deathInverseOdds = 10;
                    }

                    if (deathInverseOdds < 1 || GenerateRandomNumber(1, deathInverseOdds) == 1)
                    {
                        //Uh oh he dieded
                        charList.Remove(c);
                        i--;
                        Bot.Database.Table<SimulationCharacter>().Delete(x => x.Id == c.Id);
                        continue;
                    }
                }
            }
            Bot.Database.UpdateAll(charList);
        }
        public SimulationCharacter GenerateRandomCharacter(long guildID)
        {
            string firstName = Bot.Modules.LifeSim.GetRandomName();
            string lastName = Bot.Modules.LifeSim.GetRandomName();

            int ageTicks = YearsToTicks(GenerateRandomNumber(25, 45));
            int healthyLifespan = GenerateRandomNumber(65, 85);
            PersonalityStruct personality = PersonalityStruct.GenerateRandomPersonality();
            SimulationInterests interests = SimulationInterests.None;

            for (int i = 1; i < (int)SimulationInterests.SIZE; i *= 2)
            {
                if (GenerateRandomNumber(0, 1.0f) < 0.25f)
                {
                    interests |= (SimulationInterests)i;
                }
            }

            return new SimulationCharacter(guildID, firstName, lastName, ageTicks, healthyLifespan, personality, interests);
        }

        public static int TicksToYears(int ticks)
        {
            return ticks / TICKS_PER_YEAR;
        }
        public static int YearsToTicks(int years)
        {
            return years * TICKS_PER_YEAR;
        }

    }
}