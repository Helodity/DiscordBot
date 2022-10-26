using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DSharpPlus.Entities;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("polls")]
    public class MultipleChoicePoll : Poll {
        public MultipleChoicePoll() { }

        public MultipleChoicePoll(Poll poll) {
            MessageId = poll.MessageId;
            GuildId = poll.GuildId;
            ChannelId = poll.ChannelId;
            Question = poll.Question;
            EndTime = poll.EndTime;
            Type = PollType.MultipleChoice;
        }


        public MultipleChoicePoll(DiscordMessage message, string question, List<string> choices, DateTime endTime) : base(message, question, endTime) {
            Type = PollType.MultipleChoice;
            foreach(string choice in choices.Distinct()) {
                if(!Bot.Database.Table<PollChoice>().Any(x => x.PollId == MessageId && x.Name == choice))
                    Bot.Database.Insert(new PollChoice(MessageId, choice));
            }
        }

        public override async void OnEnd() {
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == MessageId).ToList();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == MessageId).ToList();

            string voteString = string.Empty;
            foreach(PollChoice c in choices) {
                voteString += $"**{c.Name}:** {votes.Where(x => x.Choice == c.Name).Count()} \n";
                Bot.Database.Delete(c);
            }
            foreach(Vote v in votes) {
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(this);

            try {
                var message = await GetMessageAsync();
                var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll has ended {EndTime.ToTimestamp()}!\n {Question.ToBold()} \n{voteString}",
                    Color = Bot.Style.DefaultColor
                });

                await message.ModifyAsync(builder);
            } catch {
                //We can't find the poll message, dont bother trying to edit it.
                return;
            }
        }

        public override DiscordMessageBuilder GetActiveMessageBuilder() {
            //Create the selection component based on given choices
            var choiceSelections = new List<DiscordSelectComponentOption>();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == MessageId).ToList();
            foreach(PollChoice c in choices) {
                choiceSelections.Add(new DiscordSelectComponentOption(c.Name, c.Name));
            }
            choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));
            var selectionComponent = new DiscordSelectComponent("choice", "Vote here!", choiceSelections);

            return base.GetActiveMessageBuilder().AddComponents(selectionComponent);
        }

    }
}
