namespace DiscordBotRewrite.Modules;
public class GuildPollData {
    //Reference to the Json file's relative path
    public const string JsonLocation = "Json/Polls.json";

    //The guild this data is for
    [JsonProperty("guild_id")]
    public readonly ulong Id;

    //Which channel to send polls?
    [JsonProperty("poll_channel")]
    public ulong PollChannelId;

    //List of currently running polls
    [JsonProperty("polls")]
    public List<Poll> ActivePolls;

    public GuildPollData(ulong id) {
        Id = id;
        PollChannelId = 0;
        ActivePolls = new();
    }
}
