namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("poll", "Polling!")]
class PollCommands : ApplicationCommandModule {

    [SlashCommand("start", "Start a new poll")]
    public async Task StartPoll(InteractionContext ctx,
        [Option("Question", "What's your question")] string question,
        [Option("Duration", "How long (in units) will this poll last?")] long unitAmt,
        [Option("Units", "How long is a unit?")] TimeUnit unit,
        [Option("Choice_1", "First Choice")] string choice1,
        [Option("Choice_2", "Second Choice")] string choice2,
        [Option("Choice_3", "Third Choice")] string choice3 = null,
        [Option("Choice_4", "Fourth Choice")] string choice4 = null) {

        List<string> choices = new() {
            choice1,
            choice2,
            choice3,
            choice4
        };
        choices.RemoveAll(x => x == null);

        choices = choices.Distinct().ToList();

        DateTime endTime = DateTime.Now.AddTime((int)unitAmt, unit);

        if(await Bot.Modules.Poll.StartPoll(ctx, question, choices, endTime)) {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "Started the poll!",
                Color = SuccessColor
            }, true);
        };
    }

    #region Set Channel
    [SlashCommand("channel", "Sets this channel to the server's poll channel")]
    public async Task SetPollChannel(InteractionContext ctx) {
        //Ensure we picked a text channel
        if(ctx.Channel.Type != ChannelType.Text) {
            await ctx.CreateResponseAsync("Invalid channel!");
            return;
        }

        Bot.Modules.Poll.SetPollChannel(ctx);

        await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
            Description = $"Set this server's quote channel to {ctx.Channel.Mention}!",
            Color = SuccessColor
        });
    }
    #endregion

}
