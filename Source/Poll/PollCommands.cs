﻿using DiscordBotRewrite.Global.Attributes;
using DiscordBotRewrite.Poll;
using DiscordBotRewrite.Poll.Enums;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DiscordBotRewrite.Global.Enums;
using DiscordBotRewrite.Global.Extensions;

namespace DiscordBotRewrite.Poll
{
    [SlashCommandGroup("poll", "Polling!")]
    class PollCommands : ApplicationCommandModule
    {

        #region Start
        [SlashCommand("start", "Start a new poll")]
        public async Task StartPoll(InteractionContext ctx,
            [Option("Type", "what type of poll is this?")] PollType type,
            [Option("Duration", "How many units will this poll last?")] long unitAmt,
            [Option("Units", "How long is a unit?")] TimeUnit unit)
        {

            //Make sure we can make the poll without problems
            if (Bot.Modules.Poll.HasChannelSet(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = "No poll channel has been set!",
                    Color = Bot.Style.ErrorColor,
                }, true);
                return;
            }
            string id = $"poll_start_modal{DateTime.Now}";
            DiscordInteractionResponseBuilder form = new DiscordInteractionResponseBuilder()
              .WithTitle("Start a poll!")
              .WithCustomId(id)
              .AddComponents(new TextInputComponent("Question", "question", "What do you want to ask?", max_length: 500));
            if (type == PollType.MultipleChoice)
            {
                form.AddComponents(new TextInputComponent("Choices", "choices", "Separate each choice with a comma.", style: TextInputStyle.Paragraph));
            }

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, form);

            DSharpPlus.Interactivity.InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.EventArgs.ModalSubmitEventArgs> input = await interactivity.WaitForModalAsync(id, ctx.User, timeoutOverride: TimeSpan.FromMinutes(5));

            if (input.TimedOut)
            {
                return;
            }

            List<string> choices = null;
            if (type == PollType.MultipleChoice)
            {
                choices = input.Result.Values["choices"].Split(new char[] { ',', '\n' }).ToList();
                choices.ForEach(choice => { choice = choice.Trim(); });
                choices.RemoveAll(string.IsNullOrWhiteSpace);
                choices = choices.Distinct().ToList();

                if (choices.Count < 2) {
                    await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder {
                                Description = "Invalid choices: Make sure there are at least two unique choices!",
                                Color = Bot.Style.ErrorColor
                            })
                            .AsEphemeral()
                    );
                    return;
                }
            }

            DateTime endTime = DateTime.Now.AddTime((int)unitAmt, unit);
            await Bot.Modules.Poll.StartPoll(ctx, input.Result.Values["question"], endTime, choices);
            await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder()
                .AddEmbed(new DiscordEmbedBuilder {
                    Description = "Started the poll!",
                    Color = Bot.Style.SuccessColor
                })
                .AsEphemeral());
        }
        #endregion

        #region Set Channel
        [SlashCommand("channel", "Sets this channel to the server's poll channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetPollChannel(InteractionContext ctx)
        {
            //Ensure we picked a text channel
            if (ctx.Channel.Type != ChannelType.Text)
            {
                await ctx.CreateResponseAsync("Invalid channel!");
                return;
            }

            Bot.Modules.Poll.SetPollChannel(ctx);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Set this server's quote channel to {ctx.Channel.Mention}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

    }
}