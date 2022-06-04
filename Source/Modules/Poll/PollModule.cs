using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using static DiscordBotRewrite.Global.Global;
using static DiscordBotRewrite.Global.Style;

namespace DiscordBotRewrite.Modules {
    public class PollModule {
        #region Properties
        public Dictionary<ulong, GuildPollData> PollData;
        #endregion

        #region Constructors
        public PollModule(DiscordClient client) {
            PollData = LoadJson<Dictionary<ulong, GuildPollData>>(GuildPollData.JsonLocation);
            client.ComponentInteractionCreated += OnInteraction;
            client.GuildDownloadCompleted += RemoveFinishedPolls;
        }
        #endregion

        #region Public
        public GuildPollData GetPollData(ulong guildId) {
            if(!PollData.TryGetValue(guildId, out GuildPollData pollData)) {
                pollData = new GuildPollData(guildId);
                PollData.Add(guildId, pollData);
            }
            return pollData;
        }
        public void SetPollChannel(InteractionContext ctx) {
            var data = Bot.Modules.Poll.GetPollData(ctx.Guild.Id);
            data.PollChannelId = ctx.Channel.Id;
            Bot.Modules.Poll.SavePollData(data);
        }
        // Returns whether the poll was sucessfully created
        public async Task<bool> StartPoll(InteractionContext ctx, string question, List<string> choices, DateTime endTime) {
            GuildPollData pollData = GetPollData(ctx.Guild.Id);

            //Make sure we can make the poll without problems
            if(!pollData.HasChannelSet()) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "No poll channel has been set!",
                    Color = Bot.Style.ErrorColor,
                }, true);
                return false;
            }
            if(choices.Count < 2) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Invalid Choices, make sure there are no duplicates!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            //Create the selection component based on given choices
            var choiceSelections = new List<DiscordSelectComponentOption>();
            for(int i = 0; i < choices.Count; i++) {
                choiceSelections.Add(new DiscordSelectComponentOption(choices[i], choices[i]));
            }
            choiceSelections.Add(new DiscordSelectComponentOption("Clear", "Clear"));
            var selectionComponent = new DiscordSelectComponent("choice", "Vote here!", choiceSelections);

            //Build the message and send it
            var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll ends {endTime.ToTimestamp()}! \n {question.ToBold()}",
                    Color = Bot.Style.DefaultColor
                })
                .AddComponents(selectionComponent);
            var channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
            var message = await channel.SendMessageAsync(messageBuilder);

            Poll poll = new Poll(message, question, choices, endTime);
            pollData.ActivePolls.Add(poll);
            SavePollData(pollData);
            return true;
        }
        public async void OnPollEnd(Poll poll) {
            var pData = GetPollData(poll.GuildId);
            pData.ActivePolls.Remove(poll);
            SavePollData(pData);

            pData.HasChannelSet();

            List<string> votes = poll.Choices;

            foreach(KeyValuePair<ulong, Vote> kvp in poll.Votes) {
                votes.Add(kvp.Value.Choice);
            }

            string voteString = string.Empty;
            foreach(string choice in votes.Distinct()) {
                voteString += $"**{choice}:** {votes.Where(x => x == choice).Count() - 1} \n";
            }

            var guild = await Bot.Client.GetGuildAsync(poll.GuildId);
            var channel = guild.GetChannel((ulong)pData.PollChannelId);

            try {
                var message = await channel.GetMessageAsync(poll.MessageId);
                var builder = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Description = $"Poll has ended {poll.EndTime.ToTimestamp()}!\n {poll.Question.ToBold()} \n{voteString}",
                    Color = Bot.Style.DefaultColor
                });

                await message.ModifyAsync(builder);
            } catch {
                //We couldn't get the message, so just don't edit the message
                return;
            }
        }
        #endregion

        #region Private
        Task RemoveFinishedPolls(DiscordClient sender, GuildDownloadCompletedEventArgs e) {
            //Compile polls to be completed
            List<Poll> toComplete = new List<Poll>();
            foreach(var item in PollData) {
                toComplete.AddRange(item.Value.ActivePolls.Where(x => (x.EndTime - DateTime.Now).TotalMilliseconds < 0));
            }

            //Complete the polls
            foreach(Poll p in toComplete) {
                OnPollEnd(p);
            }
            return Task.CompletedTask;
        }
        void SavePollData(GuildPollData data) {
            PollData.AddOrUpdate(data.GuildId, data);
            SaveJson(PollData, GuildPollData.JsonLocation);
        }
        async Task OnInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            //Make sure we aren't checking dms
            if(e.Guild == null)
                return;

            //Check if message is a poll message
            GuildPollData pollData = GetPollData(e.Guild.Id);
            var potentialPolls = pollData.ActivePolls.Where(x => x.MessageId == e.Message.Id);

            if(!potentialPolls.Any()) {
                return;
            }

            //There can only be one message in any guild with an ID, so we can just choose the first poll in the list.
            Poll poll = potentialPolls.First();

            if(e.Values.First() == "Clear") {
                //Remove Vote
                poll.Votes.Remove(e.User.Id);
            } else {
                //Add Vote
                Vote vote = new Vote(e.User.Id, e.Values.First());
                poll.Votes.AddOrUpdate(e.User.Id, vote);
            }

            //Save poll status and respond to the interaction
            SavePollData(pollData);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }
        #endregion
    }
}