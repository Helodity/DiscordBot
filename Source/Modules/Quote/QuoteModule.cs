using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Modules {
    public class QuoteModule {
        #region Properties
        public Dictionary<ulong, GuildQuoteData> QuoteData;
        #endregion

        #region Constructors
        public QuoteModule(DiscordClient client) {
            QuoteData = LoadJson<Dictionary<ulong, GuildQuoteData>>(GuildQuoteData.JsonLocation);
            client.MessageReactionAdded += TryQuote;
        }
        #endregion

        #region Public
        public GuildQuoteData GetQuoteData(ulong id) {
            //Create new data if it doesn't already exist
            if(!QuoteData.TryGetValue(id, out GuildQuoteData userData)) {
                userData = new GuildQuoteData(id);
                QuoteData.Add(id, userData);
            }
            return userData;
        }
        public void SaveQuoteData(GuildQuoteData data) {
            QuoteData.AddOrUpdate(data.Id, data);
            SaveJson(QuoteData, GuildQuoteData.JsonLocation);
        }
        #endregion

        #region Events
        async Task TryQuote(DiscordClient client, MessageReactionAddEventArgs args) {
            //Abort if we're in dms
            if(args.Guild == null) return;

            GuildQuoteData data = GetQuoteData(args.Guild.Id);

            //This isn't enabled in the server, ignore the reaction
            if(!data.Enabled) return;

            //Already quoted, no need to continue
            if(data.Quotes.Where(x => x.OriginalMessage == args.Message.Id).Count() > 0) return;

            //For some reason not all data is given from event args, so we get it manually here
            DiscordMessage proper_message = await args.Channel.GetMessageAsync(args.Message.Id);

            //Grab each reaction and count up the amount that are the server's quote emoji
            var potentialReactions = await proper_message.GetReactionsAsync(await args.Guild.GetEmojiAsync(data.QuoteEmojiId));

            //Clean up the reactions to remove unwanted users (bots and the author)
            var countedReactions = potentialReactions.Where(x => x.Id != proper_message.Author.Id && !x.IsBot);
            int quoteReactions = countedReactions.Count();

            //Did we get enough emojis to create a quote?
            if(quoteReactions >= data.EmojiAmountToQuote) {
                DiscordUser author = proper_message.Author; //This isn't needed but makes the embed creation look cleaner 
                DiscordChannel channel = await client.GetChannelAsync(data.QuoteChannelId);
                DiscordAttachment attachment = attachment = proper_message.Attachments.FirstOrDefault(x => x.IsImage());

                var quoteEmbed = new DiscordEmbedBuilder()
                    .WithColor(Bot.Style.DefaultColor)
                    .WithDescription(proper_message.Content)
                    .WithTimestamp(proper_message.Timestamp)
                    .WithAuthor($"{author.Username}#{author.Discriminator}", iconUrl: string.IsNullOrEmpty(author.AvatarHash) ? author.DefaultAvatarUrl : author.AvatarUrl)
                    .WithImageUrl(attachment != null ? attachment.Url : "");

                var msgBuilder = new DiscordMessageBuilder()
                    .WithEmbed(quoteEmbed)
                    .AddComponents(new DiscordLinkButtonComponent(proper_message.JumpLink.ToString(), "Context"));

                ulong quoteId = (await client.SendMessageAsync(channel, msgBuilder)).Id;

                //Save the quote to avoid repeating the same quote
                data.Quotes.Add(new Quote(quoteId, proper_message.Id));
                SaveQuoteData(data);
            }
        }
        #endregion
    }
}