namespace DiscordBotRewrite.Modules;
public class PollModule {
    public Dictionary<ulong, GuildPollData> PollData;

    public PollModule(DiscordClient client) {
        PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
        client.InteractionCreated += OnInteraction;
    }
    public GuildPollData GetPollData(ulong guildId) {
        if(!PollData.TryGetValue(guildId, out GuildPollData pollData)) {
            pollData = new GuildPollData(guildId);
            PollData.Add(guildId, pollData);
        }
        return pollData;
    }

    public void StartPoll(InteractionContext ctx, List<string> choices, int hours = 24) {
        GuildPollData pollData = GetPollData(ctx.Guild.Id);
    }
    private Task OnInteraction(DiscordClient sender, InteractionCreateEventArgs e) {
        //Make sure we aren't checking dms
        if(e.Interaction.GuildId == null)
            return Task.CompletedTask;
        //Check if message is a poll message
        //GuildPollData pollData = GetPollData(e.Interaction.Application;
        //if(pollData.ActivePolls.Any(x => x.MessageId == e.Interaction.))

        return Task.CompletedTask;
        throw new NotImplementedException();
    }

    private void OnPollEnd(Poll poll) {
        throw new NotImplementedException();
    }


}
