namespace DiscordBotRewrite.Modules;
public class PollModule {
    public Dictionary<ulong, GuildPollData> PollData;

    public PollModule(DiscordClient client) {
        PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
        client.ComponentInteractionCreated += OnInteraction;
    }
    public GuildPollData GetPollData(ulong guildId) {
        if(!PollData.TryGetValue(guildId, out GuildPollData pollData)) {
            pollData = new GuildPollData(guildId);
            PollData.Add(guildId, pollData);
        }
        return pollData;
    }
    void SavePollData(GuildPollData data) {
        PollData.AddOrUpdate(data.GuildId, data);
        SaveJson(PollData, GuildPollData.JsonLocation);
    }
    public async Task StartPoll(InteractionContext ctx, string question, List<string> choices, int hours = 24) {
        GuildPollData pollData = GetPollData(ctx.Guild.Id);

        //Create buttons based on given choices
        List<DiscordSelectComponentOption> choiceSelections = new();
        for(int i = 0; i < choices.Count; i++) {
            choiceSelections.Add(new DiscordSelectComponentOption(choices[i], choices[i]));
        }
        choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));

        var selectionComponent = new DiscordSelectComponent("choice", "Vote here!", choiceSelections);
        var messageBuilder = new DiscordMessageBuilder().WithContent(question).AddComponents(selectionComponent);
        var channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
        var message = await channel.SendMessageAsync(messageBuilder);

        Poll poll = new Poll(message, choices, DateTime.Now.AddHours(hours));
        pollData.ActivePolls.Add(poll);
    }
    async Task OnInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
        //Make sure we aren't checking dms
        if(e.Guild == null)
            return;

        //Check if message is a poll message
        GuildPollData pollData = GetPollData(e.Guild.Id);
        var potentialPolls = pollData.ActivePolls.Where(x => x.MessageId == e.Message.Id);

        if(potentialPolls.Any()) {
            Poll poll = potentialPolls.First();
            Vote vote = new Vote(e.User.Id, e.Values.First());
            //Add vote to poll
            poll.Votes.AddOrUpdate(e.User.Id, vote);

            //Save poll status
            SavePollData(pollData);
        }
    }

    private void OnPollEnd(Poll poll) {
        throw new NotImplementedException();
    }


}
