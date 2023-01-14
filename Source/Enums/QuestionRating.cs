using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Extensions;
[Flags]
public enum QuestionRating {
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