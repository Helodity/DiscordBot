namespace DiscordBotRewrite.Modules;
public class PollModule {
    public Dictionary<ulong, GuildPollData> PollData;

    public PollModule(DiscordClient client) {
        PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
        client.ComponentInteractionCreated += OnInteraction;
        client.Ready += RemoveFinishedPolls;
    }

    private Task RemoveFinishedPolls(DiscordClient sender, ReadyEventArgs e) {
        List<Poll> toComplete = new();
        foreach(var item in PollData) {
            toComplete.AddRange(item.Value.ActivePolls.Where(x => (x.EndTime - DateTime.Now).TotalMilliseconds < 0));
        }

        //Ensure the client has enough time to connect.
        //We aren't depending on this being ran right away, so no need to await it and slow down everything else.
        Task.Delay(1000).ContinueWith(x => {
            foreach(Poll p in toComplete) {
                OnPollEnd(p);
            }
        });
        return Task.CompletedTask;
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


    public void SetPollChannel(InteractionContext ctx) {
        var data = Bot.Modules.Poll.GetPollData(ctx.Guild.Id);
        data.PollChannelId = ctx.Channel.Id;
        Bot.Modules.Poll.SavePollData(data);
    }

    // Returns whether the poll was sucessfully created
    public async Task<bool> StartPoll(InteractionContext ctx, string question, List<string> choices, int hours = 24) {
        GuildPollData pollData = GetPollData(ctx.Guild.Id);
        if(!pollData.HasChannelSet()) {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "No poll channel has been set!",
                Color = ErrorColor,
            }, true);
            return false;
        }
        if(choices.Count < 2) {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "Invalid Choices, make sure there are no duplicates!",
                Color = ErrorColor
            }, true);
            return false;
        }
        //Create buttons based on given choices
        List<DiscordSelectComponentOption> choiceSelections = new();
        for(int i = 0; i < choices.Count; i++) {
            choiceSelections.Add(new DiscordSelectComponentOption(choices[i], choices[i]));
        }
        choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));

        var selectionComponent = new DiscordSelectComponent("choice", "Vote here!", choiceSelections);

        DateTime endTime = DateTime.Now.AddHours(hours);

        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder {
                Description = $"Poll ends {endTime.ToTimestamp()}! \n {question.ToBold()}",
                Color = DefaultColor
            })
            .AddComponents(selectionComponent);
        var channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
        var message = await channel.SendMessageAsync(messageBuilder);

        Poll poll = new Poll(message, choices, endTime);
        pollData.ActivePolls.Add(poll);
        SavePollData(pollData);
        return true;
    }

    async Task OnInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
        //Make sure we aren't checking dms
        if(e.Guild == null)
            return;

        //Check if message is a poll message
        GuildPollData pollData = GetPollData(e.Guild.Id);
        var potentialPolls = pollData.ActivePolls.Where(x => x.MessageId == e.Message.Id);

        if(!potentialPolls.Any()) {
            return;
        }

        //There can only be one message in any guild with an ID, so we can just choose the first poll in the list.
        Poll poll = potentialPolls.First();

        if(e.Values.First() == "Clear") {
            //Remove Vote
            poll.Votes.Remove(e.User.Id);
        } else {
            //Add Vote
            Vote vote = new Vote(e.User.Id, e.Values.First());
            poll.Votes.AddOrUpdate(e.User.Id, vote);
        }

        //Save poll status and respond to the interaction
        SavePollData(pollData);

        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    public async void OnPollEnd(Poll poll) {
        var pData = GetPollData(poll.GuildId);
        pData.ActivePolls.Remove(poll);
        SavePollData(pData);

        pData.HasChannelSet();

        List<string> votes = poll.Choices;

        foreach(KeyValuePair<ulong, Vote> kvp in poll.Votes) {
            votes.Add(kvp.Value.Choice);
        }

        string voteString = string.Empty;
        foreach(string choice in votes.Distinct()) {
            voteString += $"**{choice}:** {votes.Where(x => x == choice).Count() - 1} \n";
        }

        var guild = await Bot.Client.GetGuildAsync(poll.GuildId);
        var channel = guild.GetChannel((ulong)pData.PollChannelId);
        var message = await channel.GetMessageAsync(poll.MessageId);

        var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
            Description = $"Poll has ended {poll.EndTime.ToTimestamp()}! \n{voteString}",
            Color = DefaultColor
        });

        await message.ModifyAsync(builder);
    }


}
