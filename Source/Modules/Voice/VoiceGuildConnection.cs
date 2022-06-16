using System;
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
        public LavalinkTrack PlayingTrack => Conn.CurrentState.CurrentTrack;
        public LavalinkTrack LastPlayedTrack { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsPaused;
        public bool IsLooping;
        public bool IsShuffling;

        readonly ulong GuildId;
        readonly TimeBasedEvent IdleDisconnectEvent;
        #endregion

        #region Constructors
        public VoiceGuildConnection(DiscordClient client, DiscordGuild guild) {
            Node = client.GetLavalink().ConnectedNodes.Values.First();
            Conn = Node.GetGuildConnection(guild);
            TrackQueue = new List<LavalinkTrack>();
            IsConnected = IsPaused = IsLooping = IsShuffling = false;
            GuildId = guild.Id;
            IdleDisconnectEvent = new TimeBasedEvent(TimeSpan.FromMinutes(5), async () => {
                if(!IsTrackPlaying()) {
                    await Disconnect();
                }
            });
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
            IdleDisconnectEvent.Cancel();
        }
        public async Task ProgressQueue() {
            if(IsLooping) {
                QueueTrack(LastPlayedTrack);
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
                return;
            }

            if(args.Reason == TrackEndReason.Finished) {
                await ProgressQueue();
            }

            if(!IsTrackPlaying()) {
                IdleDisconnectEvent.Start();
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
            if(track == null) {
                Bot.Client.Logger.LogCritical("Tried to queue null track!");
                return;
            }
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
            if(track == null) {
                Bot.Client.Logger.LogCritical("Tried to play null track!");
                return;
            }

            await Conn.PlayAsync(track);
            LastPlayedTrack = track;
        }
        async Task StopPlaying() {
            await Conn.StopAsync();
        }
        bool IsTrackPlaying() {
            return PlayingTrack != null;
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