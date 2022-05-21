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

    //List of choices and their respective vote count
    [JsonProperty("votes")]
    public Dictionary<string, uint> Votes;

    public Poll(DiscordMessage message, List<string> choices, DateTime endTime) {
        GuildId = message.Channel.Guild.Id;
        MessageId = message.Id;
        EndTime = endTime;
        foreach(string choice in choices) {
            Votes.Add(choice, 0);
        }
    }
}
