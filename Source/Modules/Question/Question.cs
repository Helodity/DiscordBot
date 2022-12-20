using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiscordBotRewrite.Modules {
    public readonly struct Question {
        #region Properties
        #region Constants
        public const string TruthJsonLocation = "Json/Questions/TruthQuestions.json";
        public const string ParanoiaJsonLocation = "Json/Questions/ParanoiaQuestions.json";
        [Flags]
        public enum DepthGroup {
            All = 0,
            [ChoiceName("G")]
            G = 1 << 0,
            [ChoiceName("PG")]
            PG = 1 << 1,
            [ChoiceName("PG13")]
            PG13 = 1 << 2,
            [ChoiceName("R")]
            R = 1 << 3
        }
        #endregion

        [JsonProperty("Text")]
        public readonly string Text;

        [JsonProperty("Groups")]
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly DepthGroup Groups;
        #endregion

        #region Constructors
        public Question(string text, DepthGroup depth) {
            Text = text;
            Groups = depth;
        }
        #endregion
    }
}