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

    public async Task<List<LavalinkTrack>> GetTracksAsync(string search, LavalinkNodeConnection node) {
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
