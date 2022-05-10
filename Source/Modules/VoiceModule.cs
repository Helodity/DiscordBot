namespace DiscordBotRewrite.Modules;

public class VoiceModule {

    public DiscordClient Client;
    public LavalinkConfiguration Config;
    public LavalinkExtension LavalinkExtension;

    Dictionary<ulong, VoiceGuildConnection> GuildConnections = new();

    #region Setup
    public VoiceModule(DiscordClient client) {
        var endpoint = new ConnectionEndpoint {
            Hostname = "127.0.0.1", // From your server configuration.
            Port = 2333 // From your server configuration
        };

        Config = new LavalinkConfiguration {
            Password = "youshallnotpass", // From your server configuration.
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };
        Process.Start("Lavalink/lava_start.bat");
        Client = client;
        Client.Ready += OnClientReady;
    }
    public async Task EnableLavalink() {
        await Task.Delay(3000);
        LavalinkExtension = Client.UseLavalink();
    }
    async Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
        await LavalinkExtension.ConnectAsync(Config);
    }
    #endregion

    #region Connection Interface
    public  VoiceGuildConnection GetGuildConnection(InteractionContext ctx) {
        if(GuildConnections.ContainsKey(ctx.Guild.Id)) {
            return GuildConnections[ctx.Guild.Id];
        }
        return CreateNewConnection(ctx.Client, ctx.Guild);
    }
    VoiceGuildConnection CreateNewConnection(DiscordClient client, DiscordGuild guild) {
        VoiceGuildConnection conn = new VoiceGuildConnection(client, guild);
        GuildConnections.Add(guild.Id, conn);
        return GuildConnections[guild.Id];
    }

    public void OnVoiceGuildDisconnect(ulong guild_id) {
        GuildConnections.Remove(guild_id);
        Bot.Client.Logger.LogDebug(GuildConnections.Count().ToString());
    }

    #endregion

    #region Command Checks
    public async Task<(bool, VoiceGuildConnection)> CanUserUseModifyCommand(InteractionContext ctx) {

        VoiceGuildConnection connection = GetGuildConnection(ctx);

        if(connection.Node == null) {
            ctx.Client.Logger.LogError("Lavalink error in CanUseModifyCommand: Node does not exist");
            await ctx.CreateResponseAsync("An error occured! My owner has been notified.", true);
            return (false, connection);
        }

        if(connection.Conn == null) {
            await ctx.CreateResponseAsync("I'm not connected to a channel!", true);
            return (false, connection);
        }

        if(IsBeingUsed(connection.Conn) && !MemberInSameVoiceAsBot(connection.Conn, ctx)) {
            await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
            return (false, connection);
        }

        return (true, connection);
    }
    public async Task<(bool, VoiceGuildConnection)> CanUserSummon(InteractionContext ctx) {

        VoiceGuildConnection connection = GetGuildConnection(ctx);

        if(connection.Node == null) {
            ctx.Client.Logger.LogError("Lavalink error in CanUserSummon: Node does not exist");
            await ctx.CreateResponseAsync("An error occured! My owner has been notified.", true);
            return (false, connection);
        }

        if(ctx.Member.VoiceState == null) {
            await ctx.CreateResponseAsync("You need to be in a voice channel!", true);
            return (false, connection);
        }

        if(IsBeingUsed(connection.Conn) && !MemberInSameVoiceAsBot(connection.Conn, ctx)) {
            await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
            return (false, connection);
        }

        return (true, connection);
    }
    public bool IsBeingUsed(LavalinkGuildConnection conn) {
      return conn != null && conn.CurrentState.CurrentTrack != null && conn.AmountOfMembersInChannel() > 0;
    }
    public bool IsBeingUsed(InteractionContext ctx, out VoiceGuildConnection VGConn) {
        VGConn = GetGuildConnection(ctx);
        return IsBeingUsed(VGConn.Conn);
    }

    bool MemberInSameVoiceAsBot(LavalinkGuildConnection conn, InteractionContext ctx) {
        return ctx.Member.VoiceState != null && ctx.Member.VoiceState.Channel == conn.Channel;
    }
    #endregion
    public async Task<List<LavalinkTrack>> GetTrackAsync(string search, LavalinkNodeConnection node) {
        LavalinkLoadResult loadResult;

        Uri uri = new(search, UriKind.RelativeOrAbsolute);
        if(uri != null) {
            loadResult = await node.Rest.GetTracksAsync(uri);
            if(loadResult.LoadResultType != LavalinkLoadResultType.LoadFailed
                && loadResult.LoadResultType != LavalinkLoadResultType.NoMatches
                && loadResult.Tracks.Count() > 0) {
                return loadResult.Tracks.ToList(); //Here we could send a playlist, wheras a single song url can only return one song, so here we can take every result
            }
        }
        //Youtube search
        loadResult = await node.Rest.GetTracksAsync(search);
        if(loadResult.LoadResultType != LavalinkLoadResultType.LoadFailed
            && loadResult.LoadResultType != LavalinkLoadResultType.NoMatches &&
            loadResult.Tracks.Count() > 0) {
            List<LavalinkTrack> track = new List<LavalinkTrack> {
                loadResult.Tracks.First() //We cant take all the results since this would add 20ish songs, when only one makes sense to add
            };
            return track;
        }
        //Soundcloud
        loadResult = await node.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud);
        if(loadResult.LoadResultType != LavalinkLoadResultType.LoadFailed
            && loadResult.LoadResultType != LavalinkLoadResultType.NoMatches 
            && loadResult.Tracks.Count() > 0) {
            List<LavalinkTrack> track = new List<LavalinkTrack> {
                loadResult.Tracks.First()
            };
            return track;
        }
        return new();
    }
}

public class VoiceGuildConnection {
    public LavalinkNodeConnection Node;
    public LavalinkGuildConnection Conn;
    public List<LavalinkTrack> TrackQueue;
    public LavalinkTrack CurrentTrack;

    public bool IsConnected { get; private set;}
    public bool IsPaused;
    public bool IsLooping;
    public bool IsShuffling;

    readonly ulong Id;

    #region Constructors
    public VoiceGuildConnection(DiscordClient client, DiscordGuild guild) {
        Node = client.GetLavalink().ConnectedNodes.Values.First();
        Conn = Node.GetGuildConnection(guild);
        TrackQueue = new List<LavalinkTrack>();
        CurrentTrack = null;
        IsConnected = false;
        IsPaused = false;
        IsLooping = false;
        IsShuffling = false;
        Id = guild.Id;
    }
    #endregion

    #region Public
    public async Task Connect(DiscordChannel channel) {
        Conn = await Node.ConnectAsync(channel);
        await Conn.SetVolumeAsync(50);
        Conn.PlaybackFinished += OnPlaybackFinish;
        Conn.DiscordWebSocketClosed += OnChannelDisconnect;
        IsConnected = true;
    }

    public async Task Disconnect() {
        await Conn.DisconnectAsync();
        await OnChannelDisconnect(Conn, null);
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
        if(TrackQueue.Count > 0) {
            await PlayTrack(GetNextSongInQueue());
        } else
            await StopPlaying();
    }
    #endregion

    #region Events
    async Task OnPlaybackFinish(LavalinkGuildConnection conn, TrackFinishEventArgs args) {
        Bot.Client.Logger.LogDebug($"Lavalink PlaybackFinished triggered. Reason: {args.Reason}");
        if(Conn.AmountOfMembersInChannel() == 0) {
            await Disconnect();
        }

        if(args.Reason == TrackEndReason.Finished) {
            await ProgressQueue();
        }
    }

    private Task OnChannelDisconnect(LavalinkGuildConnection sender, WebSocketCloseEventArgs e) {
        IsConnected = false;
        Bot.Modules.Voice.OnVoiceGuildDisconnect(Id);
        Bot.Client.Logger.LogDebug($"Web socket closed at {Id}");
        return Task.CompletedTask;
    }

    #endregion

    #region Private
    void QueueTrack(LavalinkTrack track) {
        TrackQueue.Add(track);
    }
    async Task PlayTrack(LavalinkTrack track) {
        if(!IsConnected) {
            Bot.Client.Logger.LogCritical("Tried to play null track!");
            return;
        }
           
        CurrentTrack = track;
        await Conn.PlayAsync(track);
    }

    async Task StopPlaying() {
        CurrentTrack = null;
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

public static class LavalinkGuildExtensions {
    public static int AmountOfMembersInChannel(this LavalinkGuildConnection conn) {
        List<DiscordMember> members = conn.Channel.Users.ToList();
        int totalMembers = 0;
        for(int i = 0; i < members.Count; i++) {
            if(!members[i].IsBot)
                totalMembers++;
        }
        return totalMembers;
    }
}