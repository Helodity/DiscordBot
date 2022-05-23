namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("ask", "Its like truth or dare")]
class QuestionCommands : ApplicationCommandModule {
    #region Truth
    [SlashCommand("truth", "Ask a truth question")]
    public async Task AskTruth(InteractionContext ctx,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.Modules.Question;
        Question usedQuestion = module.PickQuestion(module.TruthQuestions.ToList(), rating);
        await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
            Description = usedQuestion.Text,
            Color = DefaultColor
        });
    }
    #endregion

    #region Paranoia
    [SlashCommand("paranoia", "Ask a paranoia question")]
    public async Task AskParanoia(InteractionContext ctx,
        [Option("user", "Who is recieving the question?")] DiscordUser user = null,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.Modules.Question;
        user ??= ctx.User;
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);

        if(module.ParanoiaInProgress.Contains(member.Id)) {
            await ctx.CreateResponseAsync($"Can't send question! {member.DisplayName} already has one!");
            return;
        }

        Question usedQuestion = module.PickQuestion(module.ParanoiaQuestions.ToList(), rating);

        DiscordDmChannel channel = await member.CreateDmChannelAsync();
        var msg = await member.SendMessageAsync(new DiscordEmbedBuilder() {
            Title = "Paranoia",
            Description = $"**{ctx.Member.DisplayName} sent you a question!**\n{usedQuestion.Text}\nSend a message with your answer.",
            Color = DefaultColor
        });
        module.ParanoiaInProgress.Add(user.Id);
        await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
            Description = $"Sent a question to {member.DisplayName}! Awaiting a response.",
            Color = DefaultColor
        });

        var interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(x => x.Channel == channel && x.Author == user);

        var message = result.Result;
        if(message != null) {
            string description;
            if(GenerateRandomNumber(1, 4) > 1)
                description = $"Question is hidden \n{member.DisplayName} answered: {message.Content}";
            else
                description = $"{member.DisplayName} was asked {usedQuestion.Text}. \nThey answered: {message.Content}";

            await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                Description = description,
                Color = DefaultColor
            });
        } else {
            await msg.ModifyAsync(new DiscordEmbedBuilder() {
                Description = "Time has expired.",
                Color = ErrorColor
            }.Build());
            await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                Description = $"{member.DisplayName} never answered...",
                Color = DefaultColor
            });
        }
        module.ParanoiaInProgress.Remove(user.Id);
    }
    #endregion
}
