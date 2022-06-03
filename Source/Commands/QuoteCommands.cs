using System;
using System.Threading.Tasks;
using DiscordBotRewrite.Attributes;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using static DiscordBotRewrite.Global.Style;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("quote", "auto quoting config")]
    class QuoteCommands : ApplicationCommandModule {

        #region Set Channel
        [SlashCommand("channel", "Set this channel to the server's quote channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteChannel(InteractionContext ctx) {
            //Ensure we picked a text channel
            if(ctx.Channel.Type != ChannelType.Text) {
                await ctx.CreateResponseAsync("Invalid channel!");
                return;
            }

            var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
            data.QuoteChannelId = ctx.Channel.Id;
            Bot.Modules.Quote.SaveQuoteData(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Set this server's quote channel to {ctx.Channel.Mention}!",
                Color = SuccessColor
            });
        }
        #endregion

        #region Set Emoji
        [SlashCommand("emoji", "Set this server's quote emoji")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteEmoji(InteractionContext ctx) {
            var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "React to this message with the emoji to use!",
                Color = DefaultColor
            });

            //Get the user's emoji they want
            var interactivity = ctx.Client.GetInteractivity();
            var reaction = await interactivity.WaitForReactionAsync(await ctx.GetOriginalResponseAsync(), ctx.User, TimeSpan.FromMinutes(1));

            //Ensure they sent an emoji
            if(reaction.TimedOut) {
                await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                    Description = $"No response: quote emoji remains as {DiscordEmoji.FromGuildEmote(ctx.Client, data.QuoteEmojiId)}",
                    Color = WarningColor
                });
                return;
            }

            if(!DiscordEmoji.TryFromGuildEmote(ctx.Client, reaction.Result.Emoji.Id, out DiscordEmoji emoji)) {
                await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                    Description = $"This emoji is from a different server!",
                    Color = ErrorColor
                });
                return;
            }

            await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                Description = $"Set the server's quote emoji to {reaction.Result.Emoji}",
                Color = SuccessColor
            });

            data.QuoteEmojiId = reaction.Result.Emoji.Id;
            Bot.Modules.Quote.SaveQuoteData(data);
        }
        #endregion

        #region Set Emoji Amount
        [SlashCommand("emoji_amount", "Set how many reactions are needed to quote a message")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteEmojiAmount(InteractionContext ctx, [Option("amount", "how many")] long amount) {
            var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
            data.EmojiAmountToQuote = (ushort)amount;
            Bot.Modules.Quote.SaveQuoteData(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Set emoji amount to {amount}!",
                Color = SuccessColor
            });
        }
        #endregion

        #region Toggle
        [SlashCommand("toggle", "Enable or disable the quote system")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Toggle(InteractionContext ctx) {
            var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
            data.Enabled = !data.Enabled;
            Bot.Modules.Quote.SaveQuoteData(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"{(data.Enabled ? "Enabled" : "Disabled")} auto quoting!",
                Color = SuccessColor
            });
        }
        #endregion
    }
}