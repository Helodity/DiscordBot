﻿using DiscordBotRewrite.Global.Extensions;
using DiscordBotRewrite.Poll.Enums;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SQLite;

namespace DiscordBotRewrite.Poll
{
    [Table("polls")]
    public class MultipleChoicePoll : Poll
    {
        public MultipleChoicePoll() { }

        public MultipleChoicePoll(Poll poll)
        {
            Id = poll.Id;
            MessageId = poll.MessageId;
            GuildId = poll.GuildId;
            ChannelId = poll.ChannelId;
            AskerId = poll.AskerId;
            Question = poll.Question;
            EndTime = poll.EndTime;
            Type = PollType.MultipleChoice;
        }


        public MultipleChoicePoll(DiscordMessage message, string question, List<string> choices, DateTime endTime, DiscordUser asker) : base(message, question, endTime, asker)
        {
            Type = PollType.MultipleChoice;
            foreach (string choice in choices.Distinct())
            {
                if (!Bot.Database.Table<PollChoice>().Any(x => x.PollId == MessageId && x.Name == choice))
                {
                    Bot.Database.Insert(new PollChoice(MessageId, choice));
                }
            }
        }

        public override async Task OnEnd()
        {
            await base.OnEnd();
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == MessageId).ToList();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == MessageId).ToList();

            string voteString = string.Empty;
            foreach (PollChoice c in choices)
            {
                voteString += $"**{c.Name}:** {votes.Where(x => x.Choice == c.Name).Count()}\n";
                Bot.Database.Delete(c);
            }
            foreach (Vote v in votes)
            {
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(this);
            try
            {
                DiscordMessage message = await GetMessageAsync();
                DiscordMessageBuilder builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Description = $"Poll has ended {EndTime.ToTimestamp()}!\n{Question.ToBold()}\n{voteString}",
                    Color = Bot.Style.DefaultColor
                });

                await message.ModifyAsync(builder);
            }
            catch
            {
                //We can't find the poll message, dont bother trying to edit it.
                return;
            }
        }
        public async override Task OnVote(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            Vote vote = Bot.Database.Table<Vote>().ToList().FirstOrDefault(x => x.PollId == (long)e.Message.Id && x.VoterId == (long)e.User.Id);
            if (e.Values.First() == "Clear")
            {
                if (vote != null)
                {
                    Bot.Database.Delete(vote);
                }
            }
            else
            {
                if (vote != null)
                {
                    vote.Choice = e.Values.First();
                    Bot.Database.Update(vote);
                }
                else
                {
                    vote = new Vote((long)e.Message.Id, (long)e.User.Id, e.Values.First());
                    Bot.Database.Insert(vote);
                }
            }

            if(Bot.Config.DebugLogging)
            {
                Bot.Client.Logger.LogDebug($"Received vote {vote.Choice} for multiple choice poll {MessageId} from {e.User.Id}");
            }

            DiscordMessageBuilder builder = await GetActiveMessageBuilder();
            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
        }
        public async override Task<DiscordMessageBuilder> GetActiveMessageBuilder()
        {
            //Create the selection component based on given choices
            List<DiscordSelectComponentOption> choiceSelections = new List<DiscordSelectComponentOption>();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == MessageId).ToList();
            foreach (PollChoice c in choices)
            {
                choiceSelections.Add(new DiscordSelectComponentOption(c.Name, c.Name));
            }
            choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));
            DiscordSelectComponent selectionComponent = new DiscordSelectComponent("vote", "Vote here!", choiceSelections);

            return (await base.GetActiveMessageBuilder()).AddComponents(selectionComponent);
        }

    }
}
