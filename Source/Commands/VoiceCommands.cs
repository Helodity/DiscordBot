namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("voice", "voice")]
class VoiceCommands : ApplicationCommandModule {

    [SlashCommand("join", "Join Channel")]
    public async Task JoinChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Modules.Voice.CanUserSummon(ctx);
        if(!canUse)
            return;

        await VGconn.Connect(ctx.Member.VoiceState.Channel);
        await ctx.CreateResponseAsync($"Joined {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("leave", "Leave channel")]
    public async Task LeaveChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        await VGconn.Disconnect();
        await ctx.CreateResponseAsync($"Left {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("search", "what to play")] string search) {
        VoiceModule module = Bot.Modules.Voice;
        VoiceGuildConnection VGconn = module.GetGuildConnection(ctx);
        bool canUse;
        if(VGconn.IsConnected) {
            (canUse, VGconn) = await module.CanUserSummon(ctx);
        } else {
            if(module.IsBeingUsed(VGconn.Conn))
                (canUse, VGconn) = await module.CanUserUseModifyCommand(ctx);
            else
                (canUse, VGconn) = await module.CanUserSummon(ctx);
        }

        if(!canUse)
            return;

        if(!VGconn.IsConnected)
            await VGconn.Connect(ctx.Member.VoiceState.Channel);

        await ctx.DeferAsync();
        var tracks = await module.GetTrackAsync(search, VGconn.Node);
        if(tracks.Count == 0) {
            await ctx.EditResponseAsync( "No results found!");
            return;
        }

        bool canPlayFirstSong = VGconn.CurrentTrack == null;
        if(VGconn.IsShuffling)
            tracks.Randomize();

        foreach(var track in tracks) {
            await VGconn.RequestTrackAsync(track);
        }

        string output = canPlayFirstSong ? $"Now playing `{tracks[0].Title}`" : $"Enqueued `{tracks[0].Title}`";
        if(tracks.Count > 1) {
            output += $" and {tracks.Count - 1} more songs";
        }
        output += "!";
        await ctx.EditResponseAsync(output);
    }

    [SlashCommand("skip", "Skip the currently playing song.")]
    public async Task Skip(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);

        if(!canUse)
            return;

        VGConn.CurrentTrack = null; //some shitty workaround to avoid looping the current song~!
        await VGConn.ProgressQueue();
        await ctx.CreateResponseAsync("Skipped!");
    }

    [SlashCommand("volume", "Make your music louder.")]
    public async Task Volume(InteractionContext ctx, [Option("volume", "how loud")] long volume) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        Math.Clamp(volume, 1, 200);
        await VGConn.Conn.SetVolumeAsync((int)volume / 2);
        await ctx.CreateResponseAsync($"Set the volume to {volume}%");
    }

    [SlashCommand("loop", "Loop your queue")]
    public async Task Loop(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);

        if(!canUse)
            return;

        VGConn.IsLooping = !VGConn.IsLooping;
        await ctx.CreateResponseAsync($"Looping {(VGConn.IsLooping ? "enabled" : "disabled")}!");
    }

    [SlashCommand("shuffle", "Play songs randomly")]
    public async Task Shuffle(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        VGConn.IsShuffling = !VGConn.IsShuffling;
        await ctx.CreateResponseAsync($"Shuffling {(VGConn.IsShuffling ? "enabled" : "disabled")}!");
    }

    [SlashCommand("pause", "Pause the player.")]
    public async Task Pause(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        VGConn.IsPaused = !VGConn.IsPaused;
        if(VGConn.IsPaused) {
            await ctx.CreateResponseAsync("Paused!");
            await VGConn.Conn.PauseAsync();
        } else {
            await ctx.CreateResponseAsync("Resuming!");
            await VGConn.Conn.ResumeAsync();
        }
    }

    [SlashCommand("clear", "Clear the queue.")]
    public async Task Clear(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int songCount = VGConn.TrackQueue.Count();
        VGConn.TrackQueue = new();
        await ctx.CreateResponseAsync($"Cleared {songCount} songs from queue!");
    }

    [SlashCommand("remove", "Remove the song at the specified index..")]
    public async Task Clear(InteractionContext ctx, [Option("index", "index")] long index) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        index--; //Convert from 1 being the first song to 0
        int songCount = VGConn.TrackQueue.Count();
        if(index > songCount || index < 1) {
            await ctx.CreateResponseAsync($"Index out of bounds!", true);
            return;
        }
        VGConn.TrackQueue.RemoveAt((int)index);
        await ctx.CreateResponseAsync($"Removed {VGConn.TrackQueue[(int)index].Title} from the queue!");
    }

    [SlashCommand("queue", "Show the queue")]
    public async Task List(InteractionContext ctx, [Option("page", "what page")] long page = 1) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int pageCount = (int)Math.Ceiling((decimal)VGConn.TrackQueue.Count / (decimal)10);
        int activePage = Math.Min(Math.Max(1, (int)page), pageCount);
        int endNumber = Math.Min(activePage * 10, VGConn.TrackQueue.Count());

        var embed = new DiscordEmbedBuilder {
            Title = "Queue",
            Color = DiscordColor.Blue
        };

        string description = string.Empty;
        if(pageCount == 0) {
            description = "Queue is empty!";
        } else {
            for(int i = (activePage - 1) * 10; i < endNumber; i++) {
                description += $"**{i + 1}:** `{VGConn.TrackQueue[i].Title}` by `{VGConn.TrackQueue[i].Author}` \n";
            }
        }
        embed.WithDescription(description);

        embed.WithFooter($"Page {activePage} / {pageCount}");
        await ctx.CreateResponseAsync(embed);
    }
}