using DiscordBotRewrite.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiscordBotRewrite.Question
{
    public readonly struct Question
    {
        #region Properties
        #region Constants
        public const string TruthJsonLocation = "Json/Questions/TruthQuestions.json";
        public const string ParanoiaJsonLocation = "Json/Questions/ParanoiaQuestions.json";
        #endregion

        [JsonProperty("Text")]
        public readonly string Text;

        [JsonProperty("Groups")]
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly QuestionRating Groups;
        #endregion

        #region Constructors
        public Question(string text, QuestionRating depth)
        {
            Text = text;
            Groups = depth;
        }
        #endregion
    }
}