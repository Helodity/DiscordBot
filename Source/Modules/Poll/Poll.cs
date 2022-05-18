namespace DiscordBotRewrite.Modules;
public class Poll {
    [JsonProperty("guild_id")]
    public readonly ulong GuildId;

    [JsonProperty("message_id")]
    public readonly ulong MessageId;

    [JsonProperty("end_time")]
    public DateTime EndTime;

    //Choice name, votes
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
