namespace DiscordBotRewrite.Modules;
public class GuildPollData : ModuleData {
    //Reference to the Json file's relative path
    public const string JsonLocation = "Json/Polls.json";

    //Which channel to send polls?
    [JsonProperty("poll_channel")]
    public ulong PollChannelId;

    //List of currently running polls
    [JsonProperty("polls")]
    public List<Poll> ActivePolls;

    public GuildPollData(ulong id) : base(id) {
        PollChannelId = 0;
        ActivePolls = new();
    }
}
