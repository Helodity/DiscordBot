﻿using DiscordBotRewrite.Global;
using DiscordBotRewrite.Global.Extensions;
using DiscordBotRewrite.Poll.Enums;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

        public async Task<DiscordUser> GetUserAsync() {
            return await Bot.Client.GetUserAsync((ulong)AskerId);
        }

        public void StartWatching()
        {
            new TimeBasedEvent(EndTime - DateTime.Now, async () =>
            {
                while (Bot.Modules == null)
                {
                    await Task.Delay(100);
                }
                OnEnd();
            }).Start();
        }

        public async virtual void OnEnd()
        {
            Bot.Client.Logger.LogCritical("Tried to run OnEnd() on base class poll!");
        }
        public async virtual Task OnVote(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            Bot.Client.Logger.LogCritical("Tried to run OnVote() on base class poll!");
        }
        public virtual DiscordMessageBuilder GetActiveMessageBuilder()
        {
            int voteCount = Bot.Database.Table<Vote>().Count(x => x.PollId == MessageId);
            string voteString = voteCount == 1 ? $"{voteCount} member has voted." : $"{voteCount} members have voted.";
            return new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder
                {
                    Description = $"Poll ends {EndTime.ToTimestamp()}!\n{Question.ToBold()}\n{voteString}",
                    Color = Bot.Style.DefaultColor
                });
        }
        #endregion
    }
}