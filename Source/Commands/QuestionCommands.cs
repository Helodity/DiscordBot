namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("ask", "its like truth or dare")]
class QuestionCommands : ApplicationCommandModule {
    #region truth
    [SlashCommand("truth", "Asks a truth question")]
    public async Task AskTruth(InteractionContext ctx,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.Modules.Question;
        Question usedQuestion = module.PickQuestion(module.TruthQuestions.ToList(), rating);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(usedQuestion.Text));
    }
    #endregion

    #region paranoia
    [SlashCommand("paranoia", "Asks a paranoia question")]
    public async Task AskParanoia(InteractionContext ctx,
        [Option("user", "Who is recieving the question?")] DiscordUser user = null,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.Modules.Question;
        user ??= ctx.User;
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);

        if(module.ParanoiaInProgress.Contains(member.Id)) {
            await ctx.CreateBasicResponse($"Can't' send question! {member.DisplayName} already has one!");
            return;
        }

        Question usedQuestion = module.PickQuestion(module.ParanoiaQuestions.ToList(), rating);

        DiscordDmChannel channel = await member.CreateDmChannelAsync().ConfigureAwait(false);
        await member.SendMessageAsync(ctx.Member.DisplayName + " sent you a question:\n" + usedQuestion.Text + "\nReply with your answer.");
        module.ParanoiaInProgress.Add(user.Id);
        await ctx.CreateBasicResponse($"Sent a question to {member.DisplayName}! Awaiting a response.");

        var interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(x => x.Channel == channel && x.Author == user);

        var message = result.Result;
        if(message != null) {
            if(GenerateRandomNumber(1, 4) > 1)
                await ctx.EditBasicResponse($"Question is hidden \n{member.DisplayName} answered: {message.Content}");
            else
                await ctx.EditBasicResponse($"{member.DisplayName} was asked {usedQuestion.Text}. \nThey answered: {message.Content}");
        } else {
            await member.SendMessageAsync("Time has expired.").ConfigureAwait(false);
            await ctx.EditBasicResponse($"{member.DisplayName} never answered...");
        }
        module.ParanoiaInProgress.Remove(user.Id);
    }
    #endregion
}
