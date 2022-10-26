using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Modules {
    public class PollModule {
        public PollModule(DiscordClient client) {
            Bot.Database.CreateTable<GuildPollData>();
            Bot.Database.CreateTable<Poll>();
            Bot.Database.CreateTable<PollChoice>();
            Bot.Database.CreateTable<Vote>();
            client.ComponentInteractionCreated += OnInteraction;
            client.GuildDownloadCompleted += RemoveFinishedPolls;

            foreach(Poll p in Bot.Database.Table<Poll>()) {
                p.StartWatching();
            }
        }

        #region Public
        public void SetPollChannel(InteractionContext ctx) {
            var data = Bot.Modules.Poll.GetPollData((long)ctx.Guild.Id);
            data.PollChannelId = (long)ctx.Channel.Id;
            Bot.Database.Update(data);
        }

        public bool HasChannelSet(InteractionContext ctx) {
            GuildPollData pollData = GetPollData((long)ctx.Guild.Id);
            DiscordChannel channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
            return channel == null;
        }

        // Returns whether the poll was sucessfully created
        public async Task StartPoll(InteractionContext ctx, string question, List<string> choices, DateTime endTime) {
            GuildPollData pollData = GetPollData((long)ctx.Guild.Id);

            DiscordChannel channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
            //Send a dummy message to update
            var message = await channel.SendMessageAsync(new DiscordEmbedBuilder {
                Description = "Preparing poll...",
                Color = Bot.Style.DefaultColor
            });

            Poll poll = new Poll(message, question, choices, endTime);
            var messageBuilder = GetActivePollMessageBuilder(poll);
            await message.ModifyAsync(messageBuilder);

            Bot.Database.Insert(poll);
        }
        public async void OnPollEnd(Poll poll) {
            List<Vote> votes = Bot.Database.Table<Vote>().Where(x => x.PollId == poll.MessageId).ToList();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == poll.MessageId).ToList();

            string voteString = string.Empty;
            foreach(PollChoice c in choices) {
                voteString += $"**{c.Name}:** {votes.Where(x => x.Choice == c.Name).Count()} \n";
                Bot.Database.Delete(c);
            }
            foreach(Vote v in votes) {
                Bot.Database.Delete(v);
            }
            Bot.Database.Delete(poll);

            try {
                var message = await poll.GetMessageAsync();
                var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll has ended {poll.EndTime.ToTimestamp()}!\n {poll.Question.ToBold()} \n{voteString}",
                    Color = Bot.Style.DefaultColor
                });

                await message.ModifyAsync(builder);
            } catch {
                //We can't find the poll message, dont bother trying to edit it.
                return;
            }
        }



        #endregion

        #region Events
        Task RemoveFinishedPolls(DiscordClient sender, GuildDownloadCompletedEventArgs args) {
            //Compile polls to be completed
            List<Poll> toComplete = Bot.Database.Table<Poll>().ToList();
            toComplete = toComplete.Where(x => DateTime.Compare(DateTime.Now, x.EndTime) > 0).ToList();

            //Complete the polls
            foreach(Poll p in toComplete) {
                OnPollEnd(p);
            }
            return Task.CompletedTask;
        }
        async Task OnInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            //Make sure we aren't checking dms
            if(e.Guild == null)
                return;

            //Make sure message is a poll message
            GuildPollData pollData = GetPollData((long)e.Guild.Id);
            List<Poll> polls = Bot.Database.Table<Poll>().ToList();
            Poll poll = polls.FirstOrDefault(x => x.MessageId == (long)e.Message.Id);
            if(poll == null)
                return;
            Vote vote = Bot.Database.Table<Vote>().ToList().FirstOrDefault(x => x.PollId == (long)e.Message.Id && x.VoterId == (long)e.User.Id);
            if(e.Values.First() == "Clear") {
                if(vote != null)
                    Bot.Database.Delete(vote);
            } else {
                if(vote != null) {
                    vote.Choice = e.Values.First();
                    Bot.Database.Update(vote);
                } else {
                    vote = new Vote((long)e.Message.Id, (long)e.User.Id, e.Values.First());
                    Bot.Database.Insert(vote);
                }
            }
            var builder = GetActivePollMessageBuilder(poll);
            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
        }
        #endregion

        #region Private
        GuildPollData GetPollData(long guildId) {
            GuildPollData guildPollData = Bot.Database.Table<GuildPollData>().FirstOrDefault(x => x.GuildId == guildId);
            if(guildPollData == null) {
                guildPollData = new GuildPollData(guildId);
                Bot.Database.Insert(guildPollData);
            }
            return guildPollData;
        }
        DiscordMessageBuilder GetActivePollMessageBuilder(Poll poll) {
            //Create the selection component based on given choices
            var choiceSelections = new List<DiscordSelectComponentOption>();
            List<PollChoice> choices = Bot.Database.Table<PollChoice>().Where(x => x.PollId == poll.MessageId).ToList();
            foreach(PollChoice c in choices) {
                choiceSelections.Add(new DiscordSelectComponentOption(c.Name, c.Name));
            }
            choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));
            var selectionComponent = new DiscordSelectComponent("choice", "Vote here!", choiceSelections);

            int voteCount = Bot.Database.Table<Vote>().Count(x => x.PollId == poll.MessageId);
            return new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll ends {poll.EndTime.ToTimestamp()}! \n{poll.Question.ToBold()}\n{voteCount} members have voted.",
                    Color = Bot.Style.DefaultColor
                })
                .AddComponents(selectionComponent);
        }
        #endregion
    }
}