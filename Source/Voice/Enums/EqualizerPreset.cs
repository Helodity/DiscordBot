using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Voice.Enums {
    public enum EqualizerPreset {
        [ChoiceName("Pure")] Pure,
        [ChoiceName("Base Boost")] BaseBoost,
        [ChoiceName("Super Base Boost")] SuperBaseBoost,
        [ChoiceName("Center Boost")] CenterBoost
    }
}
