using DiscordBotRewrite.Global;
using DiscordBotRewrite.Global.Extensions;
using SkiaSharp;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.LifeSim
{

    public static class LifeSimulation
    {
        public static TimeBasedEvent UpdateEvent;

        static List<string> PotentialNames;

        public static void Init()
        {   
            Bot.Database.CreateTable<SimulationCharacter>();
            Bot.Database.CreateTable<SimulationRelationship>();
            LoadPotentialNames();

            UpdateEvent = new TimeBasedEvent(TimeSpan.FromMinutes(1), () => { TickSimulation(); UpdateEvent.Start(); });
            UpdateEvent.Start();
        }

        private static void LoadPotentialNames(){
            PotentialNames = LoadJson<List<string>>("Json/LifeSim/names.json");

            if(PotentialNames.Count == 0)
            {
                PotentialNames.Add("Names need to be added in the config!");
                SaveJson(PotentialNames, "Json/LifeSim/names.json");
            }

        }

        private static void TickSimulation(){
            //TODO

        }
    }
}
