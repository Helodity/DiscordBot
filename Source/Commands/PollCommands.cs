using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Attributes;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("poll", "Polling!")]
    class PollCommands : ApplicationCommandModule {

        #region Start
        [SlashCommand("start", "Start a new poll")]
        public async Task StartPoll(InteractionContext ctx,
            [Option("Duration", "How many units will this poll last?")] long unitAmt,
            [Option("Units", "How long is a unit?")] TimeUnit unit) {

            //Make sure we can make the poll without problems
            if(Bot.Modules.Poll.HasChannelSet(ctx)) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "No poll channel has been set!",
                    Color = Bot.Style.ErrorColor,
                }, true);
                return;
            }
            var id = $"poll_start_modal{DateTime.Now}";
            var form = new DiscordInteractionResponseBuilder()
              .WithTitle("Start a poll!")
              .WithCustomId(id)
              .AddComponents(new TextInputComponent("Question", "question", "What do you want to ask?", max_length: 100))
              .AddComponents(new TextInputComponent("Choices", "choices", "Separate each choice with a comma.", style: TextInputStyle.Paragraph));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, form);

            var interactivity = ctx.Client.GetInteractivity();
            var input = await interactivity.WaitForModalAsync(id, ctx.User);

            if(input.TimedOut)
                return;

            List<string> choices = input.Result.Values["choices"].Split(new char[] {',','\n'}).ToList();
            choices.ForEach(choice => { choice = choice.Trim(); });

            choices.RemoveAll(x => string.IsNullOrWhiteSpace(x));;
            if(choices.Count < 2) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Invalid choices: Make sure there are at least two unique choices!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            DateTime endTime = DateTime.Now.AddTime((int)unitAmt, unit);
            await Bot.Modules.Poll.StartPoll(ctx, input.Result.Values["question"], choices, endTime);
            await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder {
                Description = "Started the poll!",
                Color = Bot.Style.SuccessColor
            }).AsEphemeral());
            
        }
        #endregion

        #region Set Channel
        [SlashCommand("channel", "Sets this channel to the server's poll channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetPollChannel(InteractionContext ctx) {
            //Ensure we picked a text channel
            if(ctx.Channel.Type != ChannelType.Text) {
                await ctx.CreateResponseAsync("Invalid channel!");
                return;
            }

            Bot.Modules.Poll.SetPollChannel(ctx);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Set this server's quote channel to {ctx.Channel.Mention}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

    }
}