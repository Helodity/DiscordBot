using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //The question to be answered
        [Column("question")]
        public string Question { get; set; }

        //When this poll ends
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        #region Constructors


        public Poll() {
            StartWatching();
        }

        public Poll(DiscordMessage message, string question, List<string> choices, DateTime endTime) {
            GuildId = (long)message.Channel.Guild.Id;
            ChannelId = (long)message.Channel.Id;
            MessageId = (long)message.Id;
            Question = question;
            EndTime = endTime;
            foreach(string choice in choices.Distinct()) {
                if(!Bot.Database.Table<PollChoice>().Any(x => x.PollId == MessageId && x.Name == choice))
                    Bot.Database.Insert(new PollChoice(MessageId, choice));
            }

            StartWatching();
        }
        #endregion

        #region Public
        public async Task<DiscordMessage> GetMessageAsync() {
            return await (await Bot.Client.GetChannelAsync((ulong)ChannelId)).GetMessageAsync((ulong)MessageId);
        }
        //Only 
        public void StartWatching() {
            new TimeBasedEvent(EndTime - DateTime.Now, async () => {
                while(Bot.Modules == null) {
                    await Task.Delay(100);
                }
                Bot.Modules.Poll.OnPollEnd(this);
            }).Start();
        }
        #endregion
    }
}