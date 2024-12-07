using System.Text;
using DiscordBotRewrite.Global.Extensions;
using DiscordBotRewrite.Poll.Enums;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SQLite;

namespace DiscordBotRewrite.Poll {
    [Table("polls")]
    public class ShortAnswerPoll : Poll {
        public ShortAnswerPoll() { }

        public ShortAnswerPoll(Poll poll) {
            Id = poll.Id;
            MessageId = poll.MessageId;
            GuildId = poll.GuildId;
            ChannelId = poll.ChannelId;
            AskerId = poll.AskerId;
            Question = poll.Question;
            EndTime = poll.EndTime;
            Type = PollType.ShortAnswer;
        }


        public ShortAnswerPoll(DiscordMessage message, string question, DateTime endTime, DiscordUser asker) : base(message, question, endTime, asker) {
            Type = PollType.ShortAnswer;
        }

        public override async Task OnEnd() {

            await base.OnEnd();
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == MessageId).ToList();

            string voteString = string.Empty;
            foreach(Vote v in votes) {
                voteString += v.Choice + "\n";
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(this);
            try {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(voteString));
                DiscordMessage message = await GetMessageAsync();
                DiscordMessageBuilder builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll has ended {EndTime.ToTimestamp()}!\n{Question.ToBold()}\nAnswers are above.",
                    Color = Bot.Style.DefaultColor
                }).AddFile("votes.txt", stream);

                await message.ModifyAsync(builder);
            } catch {
                //We can't find the poll message, dont bother trying to edit it.
                return;
            }
        }
        public async override Task OnVote(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            await base.OnVote(sender, e);
            Vote vote = Bot.Database.Table<Vote>().ToList().FirstOrDefault(x => x.PollId == (long)e.Message.Id && x.VoterId == (long)e.User.Id);
            DiscordMessageBuilder builder;
            if(e.Id == "clear") {
                if(vote != null)
                {
                    Bot.Database.Delete(vote);
                }

                builder = await GetActiveMessageBuilder();
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
                return;
            }
            string id = $"poll_vote_modal{DateTime.Now}{MessageId}";
            DiscordInteractionResponseBuilder form = new DiscordInteractionResponseBuilder()
               .WithTitle("Vote")
               .WithCustomId(id)
               .AddComponents(new TextInputComponent("Vote", "vote", "Put your vote here!", style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, form);
            DSharpPlus.Interactivity.InteractivityExtension interactivity = sender.GetInteractivity();
            DSharpPlus.Interactivity.InteractivityResult<ModalSubmitEventArgs> input = await interactivity.WaitForModalAsync(id, e.User, timeoutOverride: TimeSpan.FromMinutes(5));

            if(input.TimedOut)
            {
                return;
            }

            string v = input.Result.Values["vote"];
            if(vote != null) {
                vote.Choice = v;
                Bot.Database.Update(vote);
            } else {
                vote = new Vote((long)e.Message.Id, (long)e.User.Id, v);
                Bot.Database.Insert(vote);
            }

            if(Bot.Config.DebugLogging)
            {
                Bot.Client.Logger.LogDebug($"Received vote {vote.Choice} for short answer poll {MessageId} from {e.User.Id}");
            }

            builder = await GetActiveMessageBuilder();
            await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
        }
        public async override Task<DiscordMessageBuilder> GetActiveMessageBuilder() {
            DiscordButtonComponent voteComponent = new DiscordButtonComponent(ButtonStyle.Primary, "vote", "Vote");
            DiscordButtonComponent clearComponent = new DiscordButtonComponent(ButtonStyle.Danger, "clear", "Clear");
            return (await base.GetActiveMessageBuilder()).AddComponents(voteComponent, clearComponent);
        }
    }
}
