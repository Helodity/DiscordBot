namespace DiscordBotRewrite.Modules;
public class Poll {
    [JsonProperty("message_id")]
    public readonly ulong MessageId;

    [JsonProperty("end_time")]
    public DateTime EndTime;

    //Choice name, votes
    [JsonProperty("votes")]
    public Dictionary<string, uint> Votes;

    public Poll(ulong messageId, List<string> choices, DateTime endTime) {
        MessageId = messageId;
        EndTime = endTime;
        foreach(string choice in choices) {
            Votes.Add(choice, 0);
        }
    }
}
