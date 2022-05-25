namespace DiscordBotRewrite.Modules;
public class Poll {
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

    [JsonConstructor]
    public Poll(ulong guildId, ulong messageId, string question, List<string> choices, DateTime endTime) {
        GuildId = guildId;
        MessageId = messageId;
        Question = question;
        EndTime = endTime;
        Choices = choices;
        Votes = new Dictionary<ulong, Vote>();

        new TimeBasedEvent(endTime, () => { EndPoll(); }, false).Start();
    }


    public Poll(DiscordMessage message, string question, List<string> choices, DateTime endTime) {
        GuildId = message.Channel.Guild.Id;
        MessageId = message.Id;
        Question = question;
        EndTime = endTime;
        Choices = choices;
        Votes = new Dictionary<ulong, Vote>();

        new TimeBasedEvent(endTime, () => { EndPoll(); }, false).Start();
    }

    void EndPoll() {
        Bot.Modules.Poll.OnPollEnd(this);
    }


}
