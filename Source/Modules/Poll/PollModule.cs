namespace DiscordBotRewrite.Modules;
public class PollModule {
    public Dictionary<ulong, GuildPollData> PollData;

    public PollModule(DiscordClient client) {
        PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
        //TODO
        //client.InteractionCreated += PollCheck
    }
}
