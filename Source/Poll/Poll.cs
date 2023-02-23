using DiscordBotRewrite.Global;
using DiscordBotRewrite.Global.Extensions;
using DiscordBotRewrite.Poll.Enums;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using SQLite;

namespace DiscordBotRewrite.Poll
{
    [Table("polls")]
    public class Poll
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        //The guild this poll is posted in
        [Column("guild_id")]
        public long GuildId { get; set; }

        //The channel this poll is posted in
        [Column("channel_id")]
        public long ChannelId { get; set; }

        //The message ID of this poll
        [Column("message_id")]
        public long MessageId { get; set; }

        //The user ID of the asker
        [Column("asker_id")]
        public long AskerId { get; set; }

        //The question being asked
        [Column("question")]
        public string Question { get; set; }

        //When this poll ends
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        //When this poll ends
        [Column("poll_type")]
        public PollType Type { get; set; }

        #region Constructors


        public Poll() { }

        protected Poll(DiscordMessage message, string question, DateTime endTime, DiscordUser asker)
        {
            GuildId = (long)message.Channel.Guild.Id;
            ChannelId = (long)message.Channel.Id;
            MessageId = (long)message.Id;
            AskerId = (long)asker.Id;
            Question = question;
            EndTime = endTime;
            Type = PollType.ShortAnswer;

            StartWatching();
        }
        #endregion

        #region Public
        public async Task<DiscordMessage> GetMessageAsync()
        {
            return await (await Bot.Client.GetChannelAsync((ulong)ChannelId)).GetMessageAsync((ulong)MessageId);
        }

        public async Task<DiscordUser> GetAskerAsync() {
            try {
                return await Bot.Client.GetUserAsync((ulong)AskerId);
            } catch{
                return null;
            }
        }

        public void StartWatching()
        {
            new TimeBasedEvent(EndTime - DateTime.Now, async () =>
            {
                while (Bot.Modules == null)
                {
                    await Task.Delay(100);
                }
                await OnEnd();
            }).Start();
        }

        public async virtual Task OnEnd()
        {
            Bot.Client.Logger.LogCritical("Tried to run OnEnd() on base class poll!");
        }
        public async virtual Task OnVote(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            Bot.Client.Logger.LogCritical("Tried to run OnVote() on base class poll!");
        }
        public async virtual Task<DiscordMessageBuilder> GetActiveMessageBuilder()
        {
            int voteCount = Bot.Database.Table<Vote>().Count(x => x.PollId == MessageId);
            string voteString = voteCount == 1 ? $"{voteCount} member has voted." : $"{voteCount} members have voted.";
            DiscordUser asker = await GetAskerAsync();
            string askerMention = asker == null ? "Someone" : $"{asker.Mention}";
            var endEarlyButton = new DiscordButtonComponent(ButtonStyle.Primary, "end_early", "End Poll");
            return new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder {
                    Description = $"{askerMention} asks: {Question.ToBold()}\nPoll ends {EndTime.ToTimestamp()}!\n{voteString}",
                    Color = Bot.Style.DefaultColor
                }).AddComponents(endEarlyButton);
        }
        #endregion
    }
}