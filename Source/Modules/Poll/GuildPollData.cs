using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public class GuildPollData : ModuleData {
        #region Properties
        //Reference to the Json file's relative path
        public const string JsonLocation = "Json/Polls.json";

        //Which channel to send polls?
        [JsonProperty("poll_channel")]
        public ulong PollChannelId;

        //List of currently running polls
        [JsonProperty("polls")]
        public List<Poll> ActivePolls;
        #endregion

        #region Constructor
        public GuildPollData(ulong id) : base(id) {
            PollChannelId = 0;
            ActivePolls = new List<Poll>();
        }
        #endregion

        #region Public
        public bool HasPollChannelSet() {
            return GetPollChannelAsync() != null;
        }

        async Task<DiscordChannel> GetPollChannelAsync() {
            try {
                return await Bot.Client.GetChannelAsync(PollChannelId);
            } catch(NotFoundException) {
                return null;
            }
        }
        #endregion
    }
}