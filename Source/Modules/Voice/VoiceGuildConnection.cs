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
        public LavalinkTrack CurrentTrack => Conn.CurrentState.CurrentTrack;
        public List<LavalinkTrack> LastPlayedTracks { get; private set; }
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
            LastPlayedTracks = new List<LavalinkTrack>();
            IsConnected = IsPaused = IsLooping = IsShuffling = false;
            GuildId = guild.Id;
            IdleDisconnectEvent = new TimeBasedEvent(TimeSpan.FromMinutes(5), async () => {
                if(!IsPlayingTrack())
                    await Disconnect();
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
        public async Task RequestTracksAsync(List<LavalinkTrack> tracks) {
            if(tracks == null) {
                Bot.Client.Logger.LogError("RequestTracksAsync had a null list!");
                return;
            }

            if(!tracks.Any())
                return;

            if(IsShuffling)
                tracks.Randomize();

            foreach(LavalinkTrack track in tracks) {
                if(track == null)
                    continue;

                if(IsPlayingTrack())
                    QueueTrack(track);
                else
                    await PlayTrack(track);
            }
            IdleDisconnectEvent.Cancel();
        }
        public async Task ProgressQueue() {
            if(IsLooping)
                QueueTrack(LastPlayedTracks.First());
            await PlayNextTrackInQueue();
        }
        public async Task SkipTrack() {
            await PlayNextTrackInQueue();
        }
        public bool IsPlayingTrack() {
            return CurrentTrack != null;
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

            if(!IsPlayingTrack()) {
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
            LastPlayedTracks.Insert(0, track);
        }
        async Task StopPlaying() {
            await Conn.StopAsync();
        }
        LavalinkTrack GetNextSongInQueue() {
            int songIndex = GetNextSongIndex();

            LavalinkTrack track = TrackQueue[songIndex];
            TrackQueue.RemoveAt(songIndex);
            return track;
        }

        int GetNextSongIndex() {
            if(!IsShuffling) {
                LastPlayedTracks.Clear();
                return 0;
            }

            //If we're not looping, the song gets removed at the end anyway, so no point is using the bag system
            if(!IsLooping) {
                LastPlayedTracks.Clear();
                return GenerateRandomNumber(0, TrackQueue.Count - 1);
            }


            //Implement a "bag" system. All songs will be played before being readded to the possible songs that can be chosen.
            List<LavalinkTrack> potentialTracks = TrackQueue.Where(t => !LastPlayedTracks.Contains(t)).ToList();

            if(!potentialTracks.Any()) {
                potentialTracks = TrackQueue.GetRange(1, potentialTracks.Count - 1); //Avoid repeating the same song on a "bag" reset
                LastPlayedTracks.Clear();
            }

            //Just pick one at random and send it back!
            return GenerateRandomNumber(0, potentialTracks.Count - 1);
        }

        #endregion
    }
}