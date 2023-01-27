using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Global.Extensions
{
    public static class InteractionContextExtensions
    {
        public static async Task<DiscordMessage> EditResponseAsync(this InteractionContext ctx, string message)
        {
            return await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }
        public static async Task<DiscordMessage> EditResponseAsync(this InteractionContext ctx, DiscordEmbedBuilder embed)
        {
            return await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}