using DiscordBotRewrite.Global.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Global
{
    public static class CommandGuards
    {
        public static async Task<bool> PreventBotTargetAsync(InteractionContext ctx, DiscordUser user)
        {
            if (user.IsBot)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"I'm gonna let you in on a secret: bots really don't like being chosen. Pick an actual person next time!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public static async Task<bool> PreventSelfTargetAsync(InteractionContext ctx, DiscordUser user)
        {
            if (user == ctx.User)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"You can't target yourself!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckForCooldown(InteractionContext ctx, string cooldownName)
        {
            Cooldown cooldown = Cooldown.GetCooldown((long)ctx.User.Id, cooldownName);
            if (!cooldown.IsOver)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"You can run this again {cooldown.EndTime.ToTimestamp()}!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }
    }
}
