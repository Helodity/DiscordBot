﻿namespace DiscordBotRewrite.Modules;
public class QuoteModule {

    public Dictionary<ulong, GuildQuoteData> QuoteData;

    public QuoteModule(DiscordClient client) {
        QuoteData = LoadJson<Dictionary<ulong, GuildQuoteData>>(GuildQuoteData.JsonLocation);
        client.MessageReactionAdded += TryQuote;
    }

    public GuildQuoteData GetQuoteData(ulong id) {
        //Create new data if it doesn't already exist
        if(!QuoteData.TryGetValue(id, out GuildQuoteData userData)) {
            userData = new GuildQuoteData(id);
            QuoteData.Add(id, userData);
        }
        return userData;
    }
    public void SaveQuoteData(GuildQuoteData data) {
        QuoteData.AddOrUpdate(data.GuildId, data);
        SaveJson(QuoteData, GuildQuoteData.JsonLocation);
    }

    async Task TryQuote(DiscordClient client, MessageReactionAddEventArgs args) {
        GuildQuoteData data = GetQuoteData(args.Guild.Id);

        //This isn't enabled in the server, ignore the reaction
        if(!data.Enabled) return;

        //Already quoted, no need to continue
        if(data.Quotes.Where(x => x.OriginalMessage == args.Message.Id).Count() > 0) return;

        //For some reason not all data is given from event args, so we get it manually here
        DiscordMessage proper_message = await args.Channel.GetMessageAsync(args.Message.Id);

        //Grab each reaction and count up the amount that are the server's quote emoji
        var potentialReactions = await proper_message.GetReactionsAsync(await args.Guild.GetEmojiAsync(data.QuoteEmojiId));

        //Ignore the author's reaction because really you're gonna upvote your own post
        var countedReactions = potentialReactions.Where(x => x.Id != proper_message.Author.Id);
        int quoteReactions = countedReactions.Count();

        DiscordAttachment attachment = null;
        if(proper_message.Attachments.Count > 0)
            attachment = proper_message.Attachments.FirstOrDefault(x => x.IsImage());

        //Did we get enough emojis to create a quote?
        if(quoteReactions >= data.EmojiAmountToQuote) {
            DiscordUser author = proper_message.Author; //This isn't needed but makes the embed creation look cleaner
            //Quote it!
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.LightGray)
                .WithAuthor($"{author.Username}#{author.Discriminator}", iconUrl: string.IsNullOrEmpty(author.AvatarHash) ? author.DefaultAvatarUrl : author.AvatarUrl);

            if(attachment != null) {
                embed.WithImageUrl(attachment.Url);
            }
            DiscordChannel channel = await client.GetChannelAsync(data.QuoteChannelId);
            var msgBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordLinkButtonComponent(proper_message.JumpLink.ToString(), "Context"));

            ulong quoteId = (await client.SendMessageAsync(channel, msgBuilder)).Id;

            //Save the quote to avoid repeating the same quote

            data.Quotes.Add(new Quote(quoteId, proper_message.Id));
            SaveQuoteData(data);
        }
    }
}
