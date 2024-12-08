using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiscordBotRewrite.LifeSim
{
    public struct SimulationLocation
    {
        public const string JsonLocation = "Json/LifeSim/Locations.json";

        [JsonProperty("Name")]
        public string Name;

        [JsonProperty("Appeals")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SimulationInterests Appeals;

        [JsonProperty("Intensity")]
        public float Intensity;

        public SimulationLocation(string name, SimulationInterests appeals, float intensity)
        {
            Name = name;
            Appeals = appeals;
            Intensity = intensity;
        }

    }
}