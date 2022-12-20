using System.Text;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("polls")]
    public class ShortAnswerPoll : Poll {
        public ShortAnswerPoll() { }

        public ShortAnswerPoll(Poll poll) {
            Id = poll.Id;
            MessageId = poll.MessageId;
            GuildId = poll.GuildId;
            ChannelId = poll.ChannelId;
            Question = poll.Question;
            EndTime = poll.EndTime;
            Type = PollType.MultipleChoice;
        }


        public ShortAnswerPoll(DiscordMessage message, string question, DateTime endTime) : base(message, question, endTime) {
            Type = PollType.ShortAnswer;
        }

        public override async void OnEnd() {
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == MessageId).ToList();

            string voteString = string.Empty;
            foreach(Vote v in votes) {
                voteString += v.Choice + "\n";
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(this);
            try {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(voteString));
                var message = await GetMessageAsync();
                var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
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
            Vote vote = Bot.Database.Table<Vote>().ToList().FirstOrDefault(x => x.PollId == (long)e.Message.Id && x.VoterId == (long)e.User.Id);
            DiscordMessageBuilder builder;
            if(e.Id == "clear") {
                if(vote != null)
                    Bot.Database.Delete(vote);
                builder = GetActiveMessageBuilder();
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
                return;
            }
            var id = $"poll_vote_modal{DateTime.Now}{MessageId}";
            var form = new DiscordInteractionResponseBuilder()
               .WithTitle("Vote")
               .WithCustomId(id)
               .AddComponents(new TextInputComponent("Vote", "vote", "Put your vote here!", style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, form);
            var interactivity = sender.GetInteractivity();
            var input = await interactivity.WaitForModalAsync(id, e.User, timeoutOverride: TimeSpan.FromMinutes(5));

            if(input.TimedOut)
                return;

            string v = input.Result.Values["vote"];
            if(vote != null) {
                vote.Choice = v;
                Bot.Database.Update(vote);
            } else {
                vote = new Vote((long)e.Message.Id, (long)e.User.Id, v);
                Bot.Database.Insert(vote);
            }

            builder = GetActiveMessageBuilder();
            await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
        }
        public override DiscordMessageBuilder GetActiveMessageBuilder() {
            var voteComponent = new DiscordButtonComponent(ButtonStyle.Primary, "vote", "Vote");
            var clearComponent = new DiscordButtonComponent(ButtonStyle.Danger, "clear", "Clear");
            return base.GetActiveMessageBuilder().AddComponents(voteComponent, clearComponent);
        }
    }
}
