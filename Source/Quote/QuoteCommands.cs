using DiscordBotRewrite.Global.Attributes;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Quote
{
    [SlashCommandGroup("quote", "auto quoting config")]
    class QuoteCommands : ApplicationCommandModule
    {

        #region Set Channel
        [SlashCommand("channel", "Set this channel to the server's quote channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteChannel(InteractionContext ctx)
        {
            //Ensure we picked a text channel
            if (ctx.Channel.Type != ChannelType.Text)
            {
                await ctx.CreateResponseAsync("Invalid channel!");
                return;
            }

            var data = Bot.Modules.Quote.GetQuoteData((long)ctx.Guild.Id);
            data.ChannelId = (long)ctx.Channel.Id;
            Bot.Database.Update(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Set this server's quote channel to {ctx.Channel.Mention}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Set Emoji
        [SlashCommand("emoji", "Set this server's quote emoji")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteEmoji(InteractionContext ctx)
        {
            var data = Bot.Modules.Quote.GetQuoteData((long)ctx.Guild.Id);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = "React to this message with the emoji to use!",
                Color = Bot.Style.DefaultColor
            });

            //Get the user's emoji they want
            var interactivity = ctx.Client.GetInteractivity();
            var reaction = await interactivity.WaitForReactionAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

            //Ensure they sent an emoji
            if (reaction.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"No response: quote emoji remains as {Bot.Modules.Quote.GetEmoji(ctx.Client, data)}",
                    Color = Bot.Style.WarningColor
                });
                return;
            }

            DiscordEmoji emoji;
            if (reaction.Result.Emoji.Id == 0)
                emoji = DiscordEmoji.FromUnicode(ctx.Client, reaction.Result.Emoji.Name);
            else
                emoji = Bot.Modules.Quote.GetEmojiFromGuild(ctx.Guild, reaction.Result.Emoji.Id);

            if (emoji == null)
            {
                await ctx.EditResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"This emoji is from a different server!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            await ctx.EditResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Set the server's quote emoji to {reaction.Result.Emoji}",
                Color = Bot.Style.SuccessColor
            });

            data.EmojiName = reaction.Result.Emoji.Name;
            data.EmojiId = (long)reaction.Result.Emoji.Id;
            Bot.Database.Update(data);
        }
        #endregion

        #region Set Emoji Amount
        [SlashCommand("emoji_amount", "Set how many reactions are needed to quote a message")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetQuoteEmojiAmount(InteractionContext ctx, [Option("amount", "how many")] long amount)
        {
            var data = Bot.Modules.Quote.GetQuoteData((long)ctx.Guild.Id);
            data.EmojiAmount = (short)amount;
            Bot.Database.Update(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Set emoji amount to {amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Toggle
        [SlashCommand("toggle", "Enable or disable the quote system")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Toggle(InteractionContext ctx)
        {
            var data = Bot.Modules.Quote.GetQuoteData((long)ctx.Guild.Id);
            data.Enabled = !data.Enabled;
            Bot.Database.Update(data);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"{(data.Enabled ? "Enabled" : "Disabled")} auto quoting!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion
    }
}