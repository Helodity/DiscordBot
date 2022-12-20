using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Global {
    public static class CommandGuards {
        public static async Task<bool> PreventBotTargetAsync(InteractionContext ctx, DiscordUser user) {
            if(user.IsBot) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"I'm gonna let you in on a secret: bots really don't like being chosen. Pick an actual person next time!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public static async Task<bool> PreventSelfTargetAsync(InteractionContext ctx, DiscordUser user) {
            if(user == ctx.User) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't target yourself!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckForCooldown(InteractionContext ctx, string cooldownName) {
            Cooldown cooldown = Cooldown.GetCooldown((long)ctx.User.Id, cooldownName);
            if(!cooldown.IsOver) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can run this again {cooldown.EndTime.ToTimestamp()}!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckForProperBetAsync(InteractionContext ctx, long bet) {
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            if(bet < 0) {
                account.Pay(1);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Alright bitchass stop trying to game the system. I'm taking a dollar from you cuz of that.",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }

            if(account.Balance < bet) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can only bet what's in your pocket!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }
    }
}
