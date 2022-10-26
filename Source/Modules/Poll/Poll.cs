using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus.Entities;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("polls")]
    public class Poll {
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

        //The question being asked
        [Column("question")]
        public string Question { get; set; }

        //When this poll ends
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        //When this poll ends
        [Column("poll_type")]
        public PollType Type { get; set; }

        public enum PollType {
            MultipleChoice,
            ShortAnswer
        }

        #region Constructors


        public Poll() {}

        protected Poll(DiscordMessage message, string question, DateTime endTime) {
            GuildId = (long)message.Channel.Guild.Id;
            ChannelId = (long)message.Channel.Id;
            MessageId = (long)message.Id;
            Question = question;
            EndTime = endTime;
            Type = PollType.ShortAnswer;

            StartWatching();
        }
        #endregion

        #region Public
        public async Task<DiscordMessage> GetMessageAsync() {
            return await (await Bot.Client.GetChannelAsync((ulong)ChannelId)).GetMessageAsync((ulong)MessageId);
        }

        public void StartWatching() {
            new TimeBasedEvent(EndTime - DateTime.Now, async () => {
                while(Bot.Modules == null) {
                    await Task.Delay(100);
                }
                OnEnd();
            }).Start();
        }

        public async virtual void OnEnd() {
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == MessageId).ToList();

            string voteString = string.Empty;
            foreach(Vote v in votes) {
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(this);

            try {
                var message = await GetMessageAsync();
                var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll has ended {EndTime.ToTimestamp()}!\n {Question.ToBold()}",
                    Color = Bot.Style.DefaultColor
                });

                await message.ModifyAsync(builder);
            } catch {
                //We can't find the poll message, dont bother trying to edit it.
                return;
            }
        }

        public virtual DiscordMessageBuilder GetActiveMessageBuilder() {
            int voteCount = Bot.Database.Table<Vote>().Count(x => x.PollId == MessageId);
            return new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll ends {EndTime.ToTimestamp()}! \n{Question.ToBold()}\n{voteCount} members have voted.",
                    Color = Bot.Style.DefaultColor
                });
        }
        #endregion
    }
}