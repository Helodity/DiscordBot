using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Modules {
    public class VoiceGuildConnection {
        #region Properites
        public LavalinkNodeConnection Node;
        public LavalinkGuildConnection Conn;
        public List<LavalinkTrack> TrackQueue;
        public LavalinkTrack LastPlayedTrack { get; private set; }
        public List<LavalinkTrack> UsedShuffleTracks { get; private set; }

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
            GuildId = guild.Id;
            IdleDisconnectEvent = new TimeBasedEvent(TimeSpan.FromMinutes(5), async () => {
                if(!IsPlayingTrack())
                    await Disconnect();
            });
            SetDefaultState();
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
            if(!IsConnected)
                return;

            await Conn.StopAsync();
            await Conn.DisconnectAsync();
            await OnChannelDisconnect(Conn, null);
            IsConnected = false;
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
                QueueTrack(LastPlayedTrack);
            await PlayNextTrackInQueue();
        }
        public async Task SkipTrack() {
            if(!TrackQueue.Any()) {
                await Conn.StopAsync();
                return;
            }
            await PlayNextTrackInQueue();
        }
        public bool IsPlayingTrack() {
            return Conn.CurrentState.CurrentTrack != null;
        }
        #endregion

        #region Events
        async Task OnPlaybackFinish(LavalinkGuildConnection conn, TrackFinishEventArgs args) {
            args.Handled = true;
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
        private Task OnChannelDisconnect(LavalinkGuildConnection sender, WebSocketCloseEventArgs args) {
            if(args != null)
                args.Handled = true;
            Bot.Client.Logger.LogDebug($"Web socket closed at {GuildId}");
            SetDefaultState();
            IdleDisconnectEvent.Cancel();
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
            if(TrackQueue.Any()) {
                await PlayTrack(GetNextSongInQueue());
            }
        }
        async Task PlayTrack(LavalinkTrack track) {
            if(track == null) {
                Bot.Client.Logger.LogCritical("Tried to play null track!");
                return;
            }

            await Conn.PlayAsync(track);
            LastPlayedTrack = track;
            UsedShuffleTracks.Add(track);
        }
        LavalinkTrack GetNextSongInQueue() {
            int songIndex = GetNextSongIndex();

            LavalinkTrack track = TrackQueue[songIndex];
            TrackQueue.RemoveAt(songIndex);
            return track;
        }
        int GetNextSongIndex() {
            //We're not shuffling, so we don't need the bag system
            if(!IsShuffling) {
                UsedShuffleTracks.Clear();
                return 0;
            }

            //If we're not looping, the song gets removed at the end anyway, so no point is using the bag system
            if(!IsLooping) {
                UsedShuffleTracks.Clear();
                return GenerateRandomNumber(0, TrackQueue.Count - 1);
            }


            //Implement a "bag" system. All songs will be played before being readded to the possible songs that can be chosen.
            List<LavalinkTrack> potentialTracks = TrackQueue.Where(t => !UsedShuffleTracks.Contains(t)).ToList();

            if(!potentialTracks.Any()) {
                potentialTracks = TrackQueue.Where(t => t != LastPlayedTrack).ToList(); //Avoid repeating the same song on a "bag" reset
                UsedShuffleTracks.Clear();
            }

            //Just pick one at random and send it back!
            return GenerateRandomNumber(0, potentialTracks.Count - 1);
        }
        void SetDefaultState() {
            TrackQueue = new List<LavalinkTrack>();
            UsedShuffleTracks = new List<LavalinkTrack>();
            IsConnected = IsPaused = IsLooping = IsShuffling = false;
        }
        #endregion
    }
}