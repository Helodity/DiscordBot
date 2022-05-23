namespace DiscordBotRewrite.Modules;
public class Poll {
    //The guild this poll is posted in
    [JsonProperty("guild_id")]
    public readonly ulong GuildId;

    //The message ID of this poll
    [JsonProperty("message_id")]
    public readonly ulong MessageId;

    //When this poll ends
    [JsonProperty("end_time")]
    public DateTime EndTime;

    //Potential choices
    [JsonProperty("choices")]
    public List<string> Choices;

    //List of votes and a corresponding user id
    [JsonProperty("votes")]
    public Dictionary<ulong, Vote> Votes;

    [JsonConstructor]
    public Poll(ulong guildId, ulong messageId, List<string> choices, DateTime endTime) {
        GuildId = guildId;
        MessageId = messageId;
        EndTime = endTime;
        Choices = choices;
        Votes = new Dictionary<ulong, Vote>();

        //Setup poll end event, this should automatically end any polls that ended while the bot is off
        int durationMs = (int)(EndTime - DateTime.Now).TotalMilliseconds;

        if(durationMs > 0) {
            Task.Delay(durationMs).ContinueWith(x => {
                Bot.Modules.Poll.OnPollEnd(this);
            });
        }
    }


    public Poll(DiscordMessage message, List<string> choices, DateTime endTime) {
        GuildId = message.Channel.Guild.Id;
        MessageId = message.Id;
        EndTime = endTime;
        Choices = choices;
        Votes = new Dictionary<ulong, Vote>();

        //Setup poll end event, this should automatically end any polls that ended while the bot is off
        int durationMs = (int)(EndTime - DateTime.Now).TotalMilliseconds;

        Task.Delay(durationMs).ContinueWith(x => {
            Bot.Modules.Poll.OnPollEnd(this);
        });
    }

}
