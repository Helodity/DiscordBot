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
        #region Constructors
        public QuoteModule(DiscordClient client) {
            Bot.Database.CreateTable<GuildQuoteData>();
            Bot.Database.CreateTable<Quote>();
            client.MessageReactionAdded += TryQuote;
        }
        #endregion

        #region Public
        public GuildQuoteData GetQuoteData(long id) {
            GuildQuoteData quoteData = Bot.Database.Table<GuildQuoteData>().FirstOrDefault(x => x.GuildId == id);
            if(quoteData == null) {
                quoteData = new GuildQuoteData(id);
                Bot.Database.Insert(quoteData);
            }
            return quoteData;
        }

        public DiscordEmoji GetEmoji(DiscordClient client, GuildQuoteData data) {
            if(data.EmojiId == 0) {
                return DiscordEmoji.FromUnicode(client, data.EmojiName);
            } else {
                return DiscordEmoji.FromGuildEmote(client, (ulong)data.EmojiId);
            }
        }
        public DiscordEmoji GetEmojiFromGuild(DiscordGuild guild, ulong id) {
            foreach(DiscordEmoji emoji in guild.Emojis.Values) {
                if(emoji.Id == id)
                    return emoji;
            }
            return null;
        }
        #endregion

        #region Events
        async Task TryQuote(DiscordClient client, MessageReactionAddEventArgs args) {
            //We're in dms, ignore the reaction
            if(args.Guild == null) return;

            GuildQuoteData data = GetQuoteData((long)args.Guild.Id);

            //This isn't enabled in the server, ignore the reaction
            if(!data.Enabled) return;

            //We don't have a channel set, ignore the reaction
            if(data.ChannelId == 0) return;

            //We don't have a set amount of emojis needed, ignore the reaction
            if(data.EmojiAmount <= 0) return;

            //Already quoted, ignore the reaction
            if(Bot.Database.Table<Quote>().ToList().Any(x => (ulong)x.OriginalMessage == args.Message.Id)) return;

            //For some reason not all data is given from event args, so we get it manually here
            DiscordMessage proper_message = await args.Channel.GetMessageAsync(args.Message.Id);

            //Grab each reaction and count up the amount that are the server's quote emoji
            var potentialReactions = await proper_message.GetReactionsAsync(GetEmoji(client, data));

            //Clean up the reactions to remove unwanted users (bots and the author)
            var countedReactions = potentialReactions.Where(x => x.Id != proper_message.Author.Id && !x.IsBot);
            int quoteReactions = countedReactions.Count();

            //Did we get enough emojis to create a quote?
            if(quoteReactions >= data.EmojiAmount) {
                DiscordUser author = proper_message.Author;
                DiscordChannel channel = await client.GetChannelAsync((ulong)data.ChannelId);
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

                Bot.Database.Insert(new Quote((long)quoteId, (long)proper_message.Id));
            }
        }
        #endregion
    }
}