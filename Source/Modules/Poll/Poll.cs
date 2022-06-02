using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotRewrite.Global;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public class Poll {
        #region Properites
        //The guild this poll is posted in
        [JsonProperty("guild_id")]
        public readonly ulong GuildId;

        //The message ID of this poll
        [JsonProperty("message_id")]
        public readonly ulong MessageId;

        //The question to be answered
        [JsonProperty("question")]
        public readonly string Question;

        //When this poll ends
        [JsonProperty("end_time")]
        public DateTime EndTime;

        //Potential choices
        [JsonProperty("choices")]
        public List<string> Choices;

        //List of votes and a corresponding user id
        [JsonProperty("votes")]
        public Dictionary<ulong, Vote> Votes;
        #endregion

        #region Constructors
        [JsonConstructor]
        public Poll(ulong guildId, ulong messageId, string question, List<string> choices, DateTime endTime) {
            GuildId = guildId;
            MessageId = messageId;
            Question = question;
            EndTime = endTime;
            Choices = choices;
            Votes = new Dictionary<ulong, Vote>();

            StartWatching();
        }
        public Poll(DiscordMessage message, string question, List<string> choices, DateTime endTime) {
            GuildId = message.Channel.Guild.Id;
            MessageId = message.Id;
            Question = question;
            EndTime = endTime;
            Choices = choices;
            Votes = new Dictionary<ulong, Vote>();

            StartWatching();
        }
        #endregion

        #region Private
        void StartWatching() {
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