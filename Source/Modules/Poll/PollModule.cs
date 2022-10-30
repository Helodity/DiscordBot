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

            List<Poll> pollList = Bot.Database.Table<Poll>().ToList();
            for(int i = 0; i < pollList.Count; i++) {
                Poll p = pollList[i];

                switch(p.Type) {
                    case Poll.PollType.ShortAnswer:
                        p = new ShortAnswerPoll(p);
                        break;
                    case Poll.PollType.MultipleChoice:
                        p = new MultipleChoicePoll(p);
                        break;
                }

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
        public async Task StartPoll(InteractionContext ctx, string question, DateTime endTime, List<string> choices = null) {
            GuildPollData pollData = GetPollData((long)ctx.Guild.Id);

            DiscordChannel channel = ctx.Guild.GetChannel((ulong)pollData.PollChannelId);
            //Send a dummy message to update
            var message = await channel.SendMessageAsync(new DiscordEmbedBuilder {
                Description = "Preparing poll...",
                Color = Bot.Style.DefaultColor
            });

            Poll poll;
            if(choices != null) {
                poll = new MultipleChoicePoll(message, question, choices, endTime);
            } else {
                poll = new ShortAnswerPoll(message, question, endTime);
            }
            var messageBuilder = poll.GetActiveMessageBuilder();
            await message.ModifyAsync(messageBuilder);

            Bot.Database.Insert(poll);
        }
        #endregion

        #region Events
        Task RemoveFinishedPolls(DiscordClient sender, GuildDownloadCompletedEventArgs args) {
            //Compile polls to be completed
            List<Poll> toComplete = GetAllPolls();
            toComplete = toComplete.Where(x => DateTime.Compare(DateTime.Now, x.EndTime) > 0).ToList();

            //Complete the polls
            foreach(Poll p in toComplete) {
                p.OnEnd();
            }
            return Task.CompletedTask;
        }
        async Task OnInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            //Make sure we aren't checking dms
            if(e.Guild == null)
                return;

            if(e.Id.StartsWith("poll_vote_modal"))
                return;

            //Make sure message is a poll message
            GuildPollData pollData = GetPollData((long)e.Guild.Id);
            List<Poll> polls = GetAllPolls();
            Poll poll = polls.FirstOrDefault(x => x.MessageId == (long)e.Message.Id);
            if(poll == null)
                return;
            Task.Run(async () => { await poll.OnVote(sender, e); });
        }
        #endregion

        #region Private

        List<Poll> GetAllPolls() {
            List<Poll> pollList = Bot.Database.Table<Poll>().ToList();
            for(int i = 0; i < pollList.Count; i++) {
                Poll p = pollList[i];

                switch(p.Type) {
                    case Poll.PollType.ShortAnswer:
                        pollList[i] = new ShortAnswerPoll(p);
                        break;
                    case Poll.PollType.MultipleChoice:
                        pollList[i] = new MultipleChoicePoll(p);
                        break;
                }
            }
            return pollList;
        }

        GuildPollData GetPollData(long guildId) {
            GuildPollData guildPollData = Bot.Database.Table<GuildPollData>().FirstOrDefault(x => x.GuildId == guildId);
            if(guildPollData == null) {
                guildPollData = new GuildPollData(guildId);
                Bot.Database.Insert(guildPollData);
            }
            return guildPollData;
        }
        #endregion
    }
}