﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.Extensions.Logging;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Modules {
    public class VoiceGuildConnection {
        #region Properites
        public LavalinkNodeConnection Node;
        public LavalinkGuildConnection Conn;
        public List<LavalinkTrack> TrackQueue;
        public LavalinkTrack CurrentTrack => Conn.CurrentState.CurrentTrack;

        public bool IsConnected { get; private set; }
        public bool IsPaused;
        public bool IsLooping;
        public bool IsShuffling;

        readonly ulong GuildId;
        TimeBasedEvent IdleDisconnectEvent;
        #endregion

        #region Constructors
        public VoiceGuildConnection(DiscordClient client, DiscordGuild guild) {
            Node = client.GetLavalink().ConnectedNodes.Values.First();
            Conn = Node.GetGuildConnection(guild);
            TrackQueue = new List<LavalinkTrack>();
            IsConnected = false;
            IsPaused = false;
            IsLooping = false;
            IsShuffling = false;
            GuildId = guild.Id;
        }
        #endregion

        #region Public
        public async Task Connect(DiscordChannel channel) {
            Conn = await Node.ConnectAsync(channel);
            await Conn.SetVolumeAsync(100);
            Conn.PlaybackFinished += OnPlaybackFinish;
            Conn.DiscordWebSocketClosed += OnChannelDisconnect;
            IsConnected = true;
        }

        public async Task Disconnect() {
            await Conn.DisconnectAsync();
            await OnChannelDisconnect(Conn, null);
            IdleDisconnectEvent.Cancel();
        }
        public async Task RequestTrackAsync(LavalinkTrack track) {
            if(track == null) {
                Bot.Client.Logger.LogError("PlaySong had a null track!");
                return;
            }

            if(IsTrackPlaying()) {
                QueueTrack(track);
            } else {
                await PlayTrack(track);
            }
        }
        public async Task ProgressQueue() {
            if(IsLooping && CurrentTrack != null) {
                QueueTrack(CurrentTrack);
            }
            await PlayNextTrackInQueue();
        }
        public async Task SkipTrack() {
            await PlayNextTrackInQueue();
        }
        #endregion

        #region Events
        async Task OnPlaybackFinish(LavalinkGuildConnection conn, TrackFinishEventArgs args) {
            Bot.Client.Logger.LogDebug($"Lavalink PlaybackFinished triggered. Reason: {args.Reason}");
            if(!Conn.MembersInChannel().Any()) {
                await Disconnect();
            }

            if(args.Reason == TrackEndReason.Finished) {
                await ProgressQueue();
            }

            if(!IsTrackPlaying()) {
                //Automatically disconnect after 5 minutes if no tracks are playing
                IdleDisconnectEvent = new TimeBasedEvent(TimeSpan.FromMinutes(5), async () => {
                    if(!IsTrackPlaying()) {
                        await Disconnect();
                    }
                });
            }
        }

        private Task OnChannelDisconnect(LavalinkGuildConnection sender, WebSocketCloseEventArgs e) {
            IsConnected = false;
            Bot.Modules.Voice.OnVoiceGuildDisconnect(GuildId);
            Bot.Client.Logger.LogDebug($"Web socket closed at {GuildId}");
            return Task.CompletedTask;
        }
        #endregion

        #region Private
        void QueueTrack(LavalinkTrack track) {
            TrackQueue.Add(track);
        }
        async Task PlayNextTrackInQueue() {
            if(TrackQueue.Count > 0) {
                await PlayTrack(GetNextSongInQueue());
            } else {
                await StopPlaying();
            }
        }
        async Task PlayTrack(LavalinkTrack track) {
            if(!IsConnected) {
                Bot.Client.Logger.LogCritical("Tried to play null track!");
                return;
            }

            await Conn.PlayAsync(track);
        }
        async Task StopPlaying() {
            await Conn.StopAsync();
        }
        bool IsTrackPlaying() {
            return CurrentTrack != null;
        }
        LavalinkTrack GetNextSongInQueue() {
            //The most recent song queued is index [Count - 1], so if there's enough songs, we reduce the max by one more to prevent repeating!
            int songIndex = IsShuffling ? GenerateRandomNumber(0, TrackQueue.Count - Math.Min(2, TrackQueue.Count)) : 0;

            LavalinkTrack track = TrackQueue[songIndex];
            TrackQueue.RemoveAt(songIndex);
            return track;
        }
        #endregion
    }
}