namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("quote", "its like truth or dare")]
class QuoteCommands : ApplicationCommandModule {

    [SlashCommand("set_channel", "Sets this channel to the server's quote channel")]
    public async Task SetQuoteChannel(InteractionContext ctx) {
        //Ensure we picked a text channel
        if(ctx.Channel.Type != ChannelType.Text) {
            await ctx.CreateResponseAsync("Invalid channel!");
            return;
        }

        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        data.QuoteChannelId = ctx.Channel.Id;
        Bot.Modules.Quote.SetQuoteData(data);
        await ctx.CreateResponseAsync($"Set this server's quote channel to {ctx.Channel.Mention}!");
    }
    [SlashCommand("set_emoji", "Sets this server's quote emoji")]
    public async Task SetQuoteEmoji(InteractionContext ctx) {
        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        await ctx.CreateResponseAsync("React to this message with the emoji to use!");

        //Get the user's emoji they want
        var interactivity = ctx.Client.GetInteractivity();
        var reaction = await interactivity.WaitForReactionAsync(await ctx.GetOriginalResponseAsync(), ctx.User, TimeSpan.FromMinutes(1));

        //Ensure they sent an emoji
        if(reaction.TimedOut) {
            await ctx.EditResponseAsync($"No response: quote emoji remains as {DiscordEmoji.FromGuildEmote(ctx.Client, data.QuoteEmojiId)}");
            return;
        }
        await ctx.EditResponseAsync($"Set the server's quote emoji to {reaction.Result.Emoji}");

        data.QuoteEmojiId = reaction.Result.Emoji.Id;
        Bot.Modules.Quote.SetQuoteData(data);
    }
    [SlashCommand("set_emoji_amount", "Sets how many reactions are needed to quote a message")]
    public async Task SetQuoteEmojiAmount(InteractionContext ctx, [Option("amount", "how many")] long amount) {
        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        data.EmojiAmountToQuote = (ushort)amount;
        Bot.Modules.Quote.SetQuoteData(data);
        await ctx.CreateResponseAsync($"Set emoji amount to {amount}!");
    }
    [SlashCommand("toggle", "Enable or disable the quote system")]
    public async Task Toggle(InteractionContext ctx) {
        var data = Bot.Modules.Quote.GetQuoteData(ctx.Guild.Id);
        data.Enabled = !data.Enabled;
        Bot.Modules.Quote.SetQuoteData(data);
        await ctx.CreateResponseAsync($"{(data.Enabled ? "Enabled" : "Disabled")} auto quoting!");
    }
}

