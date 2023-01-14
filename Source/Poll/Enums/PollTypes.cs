using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Poll.Enums {
    public enum PollType {
        [ChoiceName("Multiple Choice")] MultipleChoice,
        [ChoiceName("Short Answer")] ShortAnswer
    }
}
