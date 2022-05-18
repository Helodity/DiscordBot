namespace DiscordBotRewrite.Modules;
public class PollModule {
    public Dictionary<ulong, GuildPollData> PollData;

    public PollModule(DiscordClient client) {
        PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
        client.InteractionCreated += OnInteraction;
    }
    public void StartPoll(InteractionContext ctx, List<string> choices, int hours = 24) {
        throw new NotImplementedException();
    }
    private Task OnInteraction(DiscordClient sender, InteractionCreateEventArgs e) {
        throw new NotImplementedException();
    }

    private void OnPollEnd(Poll poll) {
        throw new NotImplementedException();
    }


}
