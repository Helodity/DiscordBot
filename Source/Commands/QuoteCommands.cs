namespace DiscordBotRewrite.Commands;

class QuoteCommands : ApplicationCommandModule {

    [SlashCommand("set_quote_channel", "Sets this channel to the server's quote channel")]
    public async Task SetQuoteChannel(InteractionContext ctx) {
        //Ensure we picked a text channel
        if(ctx.Channel.Type != ChannelType.Text) {
            await BotUtils.CreateBasicResponse(ctx, "Invalid channel!");
            return;
        }

        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        data.QuoteChannelId = ctx.Channel.Id;
        Bot.Modules.Quote.SetQuoteData(data);
        await BotUtils.CreateBasicResponse(ctx, $"Set this server's quote channel to {ctx.Channel.Mention}!");
    }
    [SlashCommand("set_quote_emoji", "Sets this server's quote emoji")]
    public async Task SetQuoteEmoji(InteractionContext ctx) {
        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        await BotUtils.CreateBasicResponse(ctx, "React to this message with the emoji to use!");

        //Get the user's emoji they want
        var interactivity = ctx.Client.GetInteractivity();
        var reaction = await interactivity.WaitForReactionAsync(await ctx.GetOriginalResponseAsync(), ctx.User, TimeSpan.FromMinutes(1));

        //Ensure they sent an emoji
        if(reaction.TimedOut) {
            await BotUtils.EditBasicResponse(ctx, $"No response: quote emoji remains as {DiscordEmoji.FromGuildEmote(ctx.Client, data.QuoteEmojiId)}");
            return;
        }
        await BotUtils.EditBasicResponse(ctx, $"Set the server's quote emoji to {reaction.Result.Emoji}");

        data.QuoteEmojiId = reaction.Result.Emoji.Id;
        Bot.Modules.Quote.SetQuoteData(data);
    }
    [SlashCommand("set_quote_emoji_amount", "Sets how many reactions are needed to quote a message")]
    public async Task SetQuoteEmojiAmount(InteractionContext ctx, [Option("amount", "how many")] long amount) {
        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        data.EmojiAmountToQuote = (ushort)amount;
        Bot.Modules.Quote.SetQuoteData(data);
        await BotUtils.CreateBasicResponse(ctx, $"Set this server's quote channel to {ctx.Channel.Mention}!");
    }
}

