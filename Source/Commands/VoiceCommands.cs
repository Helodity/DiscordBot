namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("voice", "voice")]
class VoiceCommands : ApplicationCommandModule {

    [SlashCommand("join", "Join Channel")]
    public async Task JoinChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Modules.Voice.CanUserSummon(ctx);
        if(!canUse)
            return;

        await VGconn.Connect(ctx.Member.VoiceState.Channel);
        await BotUtils.CreateBasicResponse(ctx, $"Joined {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("leave", "Leave channel")]
    public async Task LeaveChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        await VGconn.Disconnect();
        await BotUtils.CreateBasicResponse(ctx, $"Left {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("search", "what to play")] string search) {
        VoiceModule module = Bot.Modules.Voice;
        VoiceGuildConnection VGconn = module.GetGuildConnection(ctx);
        bool canUse;
        if(VGconn.Conn == null) {
            (canUse, VGconn) = await module.CanUserSummon(ctx);
        } else {
            if(module.IsBeingUsed(VGconn.Conn))
                (canUse, VGconn) = await module.CanUserUseModifyCommand(ctx);
            else
                (canUse, VGconn) = await module.CanUserSummon(ctx);
        }

        if(!canUse)
            return;

        await ctx.DeferAsync();
        var tracks = await module.GetTrackAsync(search, VGconn.Node);
        if(tracks == null) {
            await BotUtils.CreateBasicResponse(ctx, "No results found!", true);
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
        await BotUtils.EditBasicResponse(ctx, output);
    }

    [SlashCommand("skip", "Skip the currently playing song")]
    public async Task Skip(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);

        if(!canUse)
            return;

        VGConn.CurrentTrack = null; //some shitty workaround to avoid looping the current song~!
        await VGConn.ProgressQueue();
        await BotUtils.CreateBasicResponse(ctx, "Skipped!");
    }

    [SlashCommand("volume", "make music quiet or loud")]
    public async Task Volume(InteractionContext ctx, [Option("volume", "how loud")] long volume) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        //Clamp the volume
        if(volume < 1)
            volume = 1;
        if(volume > 200)
            volume = 200;
        await VGConn.Conn.SetVolumeAsync((int)volume / 2);
        await BotUtils.CreateBasicResponse(ctx, $"Set the volume to {volume}%");
    }

    [SlashCommand("loop", "Loop your queue")]
    public async Task Loop(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);

        if(!canUse)
            return;

        VGConn.IsLooping = !VGConn.IsLooping;
        if(VGConn.IsLooping)
            await BotUtils.CreateBasicResponse(ctx, "Looping enabled!");
        else
            await BotUtils.CreateBasicResponse(ctx, "Looping disabled!");
    }

    [SlashCommand("shuffle", "Shuffle your queue")]
    public async Task Shuffle(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        VGConn.IsShuffling = !VGConn.IsShuffling;
        if(VGConn.IsShuffling)
            await BotUtils.CreateBasicResponse(ctx, "Shuffling enabled!");
        else
            await BotUtils.CreateBasicResponse(ctx, "Shuffling disabled!");
    }

    [SlashCommand("pause", "Pause the player")]
    public async Task Pause(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        VGConn.IsPaused = !VGConn.IsPaused;
        if(VGConn.IsPaused) {
            await BotUtils.CreateBasicResponse(ctx, "Paused!");
            await VGConn.Conn.PauseAsync();
        } else {
            await BotUtils.CreateBasicResponse(ctx, "Resuming!");
            await VGConn.Conn.ResumeAsync();
        }
    }

    [SlashCommand("clear", "Clear the queue")]
    public async Task Clear(InteractionContext ctx) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int songCount = VGConn.TrackQueue.Count();
        VGConn.TrackQueue = new();
        await BotUtils.CreateBasicResponse(ctx, $"Cleared {songCount} songs from queue!");
    }

    [SlashCommand("queue", "Show the queue")]
    public async Task List(InteractionContext ctx, [Option("page", "what page")] long page = 1) {
        (bool canUse, var VGConn) = await Bot.Modules.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int pageCount = (int)Math.Ceiling((decimal)VGConn.TrackQueue.Count / (decimal)10);
        int activePage = Math.Min(Math.Max(1, (int)page), pageCount);
        int endNumber = Math.Min((activePage) * 10, VGConn.TrackQueue.Count());

        var embed = new DiscordEmbedBuilder {
            Title = "Queue",
            Color = DiscordColor.Blue
        };

        string description = string.Empty;
        if(pageCount == 0) {
            description = "Queue is empty!";
        } else {
            for(int i = (activePage - 1) * 10; i < endNumber; i++) {
                description += $"{i + 1}: `{VGConn.TrackQueue[i].Title}` by `{VGConn.TrackQueue[i].Author}` \n";
            }
        }
        embed.WithDescription(description);

        embed.WithFooter($"Page {activePage} / {pageCount}");
        await ctx.CreateResponseAsync(embed);
    }
}