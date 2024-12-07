using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DiscordBotRewrite.LifeSim;

namespace DiscordBotRewrite.General
{
    class UnsortedCommands : ApplicationCommandModule
    {
        #region Ping
        [SlashCommand("ping", "Check if the bot is on")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = "Pong!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

    }
}