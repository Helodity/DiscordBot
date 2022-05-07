﻿namespace DiscordBotRewrite.Modules; 
public class QuoteModule {

    public Dictionary<ulong, QuoteData> QuoteData;

    public QuoteModule(DiscordClient client) {
        QuoteData = BotUtils.LoadJson<Dictionary<ulong, QuoteData>>(Modules.QuoteData.JsonLocation);
        client.MessageReactionAdded += TryQuote;
    }

    public QuoteData GetQuoteData(ulong id) {
        //Create new data if it doesn't already exist
        if(!QuoteData.TryGetValue(id, out QuoteData userData)) {
            userData = new QuoteData(id);
            QuoteData.Add(id, userData);
        }
        return userData;
    }
    public void SetQuoteData(QuoteData data) {
        QuoteData.AddOrUpdate(data.Id, data);
        BotUtils.SaveJson(QuoteData, Modules.QuoteData.JsonLocation);
    }

    async Task TryQuote(DiscordClient client, MessageReactionAddEventArgs args) {
        QuoteData data = GetQuoteData(args.Guild.Id);

        //This isn't enabled in the server, ignore the reaction
        if(!data.Enabled) return;

        //Already quoted, no need to continue
        if(data.QuotedMessages.Contains(args.Message.Id)) return;

        //For some reason not all data is given from event args, so we get it manually here
        DiscordMessage proper_message = await args.Channel.GetMessageAsync(args.Message.Id);

        //Grab each reaction and count up the amount that are the server's quote emoji
        var reactions = await proper_message.GetReactionsAsync(await args.Guild.GetEmojiAsync(data.QuoteEmojiId));
        int quoteReactions = reactions.Count;

        DiscordAttachment attachment= null;
        if(proper_message.Attachments.Count > 0)
            attachment = proper_message.Attachments.FirstOrDefault(x => x.IsImage());

        //Did we get enough emojis to create a quote?
        if(quoteReactions >= data.EmojiAmountToQuote) {
            DiscordUser author = proper_message.Author; //This isn't needed but makes the embed creation look cleaner
            //Quote it!
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.LightGray)
                .WithAuthor($"{author.Username}#{author.Discriminator}", iconUrl: string.IsNullOrEmpty(author.AvatarHash) ? author.DefaultAvatarUrl : author.AvatarUrl)
                .WithDescription(proper_message.Content + $"\n\n[Context]({proper_message.JumpLink})");

            if(attachment != null) {
                embed.WithImageUrl(attachment.Url);
            }

            DiscordChannel channel = await client.GetChannelAsync(data.QuoteChannelId);
            await client.SendMessageAsync(channel, embed);
            
            //Save the quote to avoid repeating the same quote
            data.QuotedMessages.Add(proper_message.Id);
            SetQuoteData(data);
        }
    }
}

public class QuoteData {
    //Reference to the Json file's relative path
    public const string JsonLocation = "Json/Quotes.json";

    //The guild this data is for
    [JsonProperty("guild_id")]
    public readonly ulong Id;

    //The guild this data is for
    [JsonProperty("enabled")]
    public bool Enabled;

    //Which channel to send quotes?
    [JsonProperty("quote_channel")]
    public ulong QuoteChannelId;

    //What emoji do we look for when quoting
    [JsonProperty("quote_emoji")]
    public ulong QuoteEmojiId;

    //How many of these emojis need to be added to quote a message
    [JsonProperty("emoji_amount_to_quote")]
    public ushort EmojiAmountToQuote;

    //List of already quoted messages
    [JsonProperty("quotes")]
    public List<ulong> QuotedMessages;

    public QuoteData(ulong id) {
        Id = id;
        Enabled = true;
        QuoteChannelId = 0;
        QuoteEmojiId = 0;
        EmojiAmountToQuote = 1;
        QuotedMessages = new();
    }
}
