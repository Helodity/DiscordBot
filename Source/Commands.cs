namespace DiscordBotRewrite;

class UnsortedCommands : ApplicationCommandModule {
    [SlashCommand("ping", "Check if the bot is on.")]
    public async Task Ping(InteractionContext ctx) {
        await BotUtils.CreateBasicResponse(ctx, $"Pong!");
    }

    [SlashCommand("how", "Find out how __ you are.")]
    public async Task How(InteractionContext ctx, [Option("what", "how what you are")] string what) {
        await BotUtils.CreateBasicResponse(ctx, $"You are {BotUtils.GenerateRandomNumber(0, 100)}% {what}.");
    }

    [SlashCommand("scp", "Gives you an SCP article to read")]
    public async Task RandomScp(InteractionContext ctx) {
        int number = BotUtils.GenerateRandomNumber(1, 2000);
        string output = "http://www.scpwiki.com/scp-";

        if(number < 10) {
            output += "00";
        } else if(number < 100) {
            output += "0";
        }
        output += number.ToString();

        await BotUtils.CreateBasicResponse(ctx, output);
    }

    [SlashCommand("8ball", "Ask a question and The Ball shall answer.")]
    public async Task EightBall(InteractionContext ctx, [Option("question", "The question for The Ball to answer")] string question) {
        int thinkingNum = BotUtils.GenerateRandomNumber(1, 5);
        string thinkStr;
        switch(thinkingNum) {
            case 1:
                thinkStr = "ponders";
                break;
            case 2:
                thinkStr = "imagines";
                break;
            case 3:
                thinkStr = "thinks";
                break;
            case 4:
                thinkStr = "judges";
                break;
            default:
                thinkStr = "reckons";
                break;

        }
        await BotUtils.CreateBasicResponse(ctx, $"{ctx.Member.DisplayName} questions The Ball. It {thinkStr}...");

        int delay = BotUtils.GenerateRandomNumber(1000, 3000);
        await Task.Delay(delay);

        int result = BotUtils.GenerateRandomNumber(1, 5);
        string output;
        switch(result) {
            case 1:
                output = "Likely";
                break;
            case 2:
                output = "Unlikely";
                break;
            case 3:
                output = "Chances say yes";
                break;
            case 4:
                output = "Probably not";
                break;
            default:
                output = "Ask again";
                break;

        }
        await BotUtils.EditBasicResponse(ctx, $"{ctx.Member.DisplayName} asks: \"{question}\" \n{output}.");
    }

    #region quote
    //[ContextMenu(ApplicationCommandType.MessageContextMenu, "Quote")]
    //public async Task Quote(ContextMenuContext ctx) {
    //    var data = Bot.Quote.GetQuoteData(ctx.Guild.Id);
    //    if(ctx.Guild.Channels.TryGetValue(data.QuoteChannel, out var channel)) {
    //        await ctx.DeferAsync();

    //        var embed = new DiscordEmbedBuilder()
    //            .WithColor(DiscordColor.LightGray)
    //            .WithAuthor($"{ctx.TargetMessage.Author.Username}#{ctx.TargetMessage.Author.Discriminator}", iconUrl: string.IsNullOrEmpty(ctx.TargetMessage.Author.AvatarHash) ? ctx.TargetMessage.Author.DefaultAvatarUrl : ctx.TargetMessage.Author.AvatarUrl)
    //            .WithDescription(ctx.TargetMessage.Content + $"\n\n[Context]({ctx.TargetMessage.JumpLink})");

    //        if(ctx.TargetMessage.Attachments.Any())
    //            embed.WithImageUrl(ctx.TargetMessage.Attachments[0].Url);

    //        await ctx.Client.SendMessageAsync(channel, embed);

    //        await ctx.DeleteResponseAsync();
    //    }
    //}
    #endregion
}

class QuoteCommands : ApplicationCommandModule {

    [SlashCommand("set_quote_channel", "Sets this channel to the server's quote channel")]
    public async Task SetQuoteChannel(InteractionContext ctx) {
        //Ensure we picked a text channel
        if(ctx.Channel.Type != ChannelType.Text) {
            await BotUtils.CreateBasicResponse(ctx, "Invalid channel!");
            return;
        }

        var data = Bot.Quote.GetQuoteData(ctx.Guild.Id);
        data.QuoteChannelId = ctx.Channel.Id;
        Bot.Quote.SetQuoteData(data);
        await BotUtils.CreateBasicResponse(ctx, $"Set this server's quote channel to {ctx.Channel.Mention}!");
    }
    [SlashCommand("set_quote_emoji", "Sets this server's quote emoji")]
    public async Task SetQuoteEmoji(InteractionContext ctx) {
        var data = Bot.Quote.GetQuoteData(ctx.Guild.Id);
        await BotUtils.CreateBasicResponse(ctx, "React to this message with the emoji to use!");

        //Get the user's emoji they want
        var interactivity = ctx.Client.GetInteractivity();
        var reaction = await interactivity.WaitForReactionAsync(await ctx.GetOriginalResponseAsync(), ctx.User, TimeSpan.FromMinutes(1));

        //Ensure they sent an emoji
        if(reaction.TimedOut) {
            await BotUtils.EditBasicResponse(ctx, $"No response: quote emoji remains as {DiscordEmoji.FromGuildEmote(ctx.Client, data.QuoteEmojiId)}");
            return;
        }
        await BotUtils.EditBasicResponse(ctx, $"Set the server's quote emoji to {reaction.Result.Emoji}");

        data.QuoteEmojiId = reaction.Result.Emoji.Id;
        Bot.Quote.SetQuoteData(data);
    }
    [SlashCommand("set_quote_emoji_amount", "Sets how many reactions are needed to quote a message")]
    public async Task SetQuoteEmojiAmount(InteractionContext ctx, [Option("amount", "how many")] long amount) {
        var data = Bot.Quote.GetQuoteData(ctx.Guild.Id);
        data.EmojiAmountToQuote = (ushort)amount;
        Bot.Quote.SetQuoteData(data);
        await BotUtils.CreateBasicResponse(ctx, $"Set this server's quote channel to {ctx.Channel.Mention}!");
    }
}


[SlashCommandGroup("ask", "its like truth or dare")]
class QuestionCommands : ApplicationCommandModule {
    #region truth
    [SlashCommand("truth", "Asks a truth question")]
    public async Task AskTruth(InteractionContext ctx,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.QuestionModule;
        Question usedQuestion = module.PickQuestion(module.TruthQuestions.ToList(), rating);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(usedQuestion.Text));
    }
    #endregion

    #region paranoia
    [SlashCommand("paranoia", "Asks a paranoia question")]
    public async Task AskParanoia(InteractionContext ctx,
        [Option("user", "Who is recieving the question?")] DiscordUser user = null,
        [Option("rating", "How risky is the question?")] Question.DepthGroup rating = Question.DepthGroup.G) {

        QuestionModule module = Bot.QuestionModule;
        user ??= ctx.User;
        DiscordMember member = await BotUtils.IdToMember(ctx.Guild, user.Id);

        if(module.ParanoiaInProgress.Contains(member.Id)) {
            await BotUtils.CreateBasicResponse(ctx, $"Can't' send question! {member.DisplayName} already has one!");
            return;
        }

        Question usedQuestion = module.PickQuestion(module.ParanoiaQuestions.ToList(), rating);

        DiscordDmChannel channel = await member.CreateDmChannelAsync().ConfigureAwait(false);
        await member.SendMessageAsync(ctx.Member.DisplayName + " sent you a question:\n" + usedQuestion.Text + "\nReply with your answer.");
        module.ParanoiaInProgress.Add(user.Id);
        await BotUtils.CreateBasicResponse(ctx, $"Sent a question to {member.DisplayName}! Awaiting a response.");

        var interactivity = ctx.Client.GetInteractivity();
        InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(x => x.Channel == channel && x.Author == user);

        var message = result.Result;
        if(message != null) {
            if(BotUtils.GenerateRandomNumber(1, 4) > 1)
                await BotUtils.EditBasicResponse(ctx, $"Question is hidden \n{member.DisplayName} answered: {message.Content}");
            else
                await BotUtils.EditBasicResponse(ctx, $"{member.DisplayName} was asked {usedQuestion.Text}. \nThey answered: {message.Content}");
        } else {
            await member.SendMessageAsync("Time has expired.").ConfigureAwait(false);
            await BotUtils.EditBasicResponse(ctx, $"{member.DisplayName} never answered...");
        }
        module.ParanoiaInProgress.Remove(user.Id);
    }
    #endregion
}

[SlashCommandGroup("pixel", "r/place but inside a discord bot")]
class PixelCommands : ApplicationCommandModule {

    public SKSurface GetPixelSurface(InteractionContext ctx, int anchorX, int anchorY, int distance) {
        var mapData = Bot.Pixel.GetPixelMap(ctx.Guild.Id);
        int scale = 500 / distance;

        SKImageInfo imageInfo = new SKImageInfo(500, 500);
        SKSurface surface = SKSurface.Create(imageInfo);
        SKCanvas canvas = surface.Canvas;

        for(int x = 0; x < distance; x++) {
            for(int y = 0; y < distance; y++) {
                int curX = x + anchorX;
                int curY = y + anchorY;
                SKColor color;
                if(curX < 0 || curX >= PixelModule.MapWidth || curY < 0 || curY >= PixelModule.MapHeight) {
                    color = SKColors.Black.WithAlpha(100);
                } else if(!PixelModule.PixelDict.TryGetValue((uint)mapData.PixelState[curX, curY], out color)) {
                    color = SKColors.White;
                }
                SKPaint paint = new SKPaint();
                paint.Color = color;
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = false;

                canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
            }
        }

        return surface;
    }
    
    public void AddInteractUI(SKSurface surface, int zoom, PixelModule.PixelEnum colorEnum) {
        if(PixelModule.PixelDict.TryGetValue((uint)colorEnum, out SKColor color))
            DrawCenterPixel(surface.Canvas, zoom, color.WithAlpha(100));
        AddOutline(surface.Canvas, zoom, 2, SKColors.Silver);
    }
    void AddOutline(SKCanvas canvas, int zoom, int thickness, SKColor color) {
        SKPaint paint = new SKPaint();
        paint.Color = color;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = thickness;
        paint.IsAntialias = false;

        int pixelSize = 500 / zoom;

        canvas.DrawRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint);
    }
    
    void DrawCenterPixel(SKCanvas canvas, int zoom, SKColor color) {
        SKPaint paint = new SKPaint();
        paint.Color = color;
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = false;

        int pixelSize = 500 / zoom;

        canvas.DrawRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint);
    }

    [SlashCommand("view", "look at the current canvas")]
    public async Task ViewCanvas(InteractionContext ctx) {
        await BotUtils.CreateBasicResponse(ctx, $"Loading...", true);
        SKSurface canvas = GetPixelSurface(ctx, 0, 0, PixelModule.MapHeight);
        canvas.Snapshot().SaveToPng("img.png");
        using(var fs = new FileStream($"img.png", FileMode.Open, FileAccess.Read)) {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddFile("img.png", fs).AsEphemeral());
        }
    }
    [SlashCommand("interact", "look with buttons")]
    public async Task Interact(InteractionContext ctx,
        [Option("x", "x to start at")] long start_x = 5,
        [Option("y", "y to start at")] long start_y = 5) {

        DiscordButtonComponent[] row1 = {
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled11", " ", true),
            new DiscordButtonComponent(ButtonStyle.Primary, "moveUp", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled13", " ", true),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled14", " ", true),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled15", " ", true)
        };
        DiscordButtonComponent[] row2 = {
            new DiscordButtonComponent(ButtonStyle.Primary, "moveLeft", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "place", "Place"),
            new DiscordButtonComponent(ButtonStyle.Primary, "moveRight", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled24", " ", true),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled25", " ", true)
        };
        DiscordButtonComponent[] row3 = {
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled31", " ", true),
            new DiscordButtonComponent(ButtonStyle.Primary, "moveDown", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled33", " ", true),
            new DiscordButtonComponent(ButtonStyle.Primary, "colorChange", "Color"),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled35", " ", true)
        };
        DiscordButtonComponent[] row4 = {
            new DiscordButtonComponent(ButtonStyle.Secondary, "zoomIcon", null, true, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":mag_right:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "zoomIn", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "zoomOut", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_minus_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled44", " ", true),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled45", " ", true)
        };
        DiscordButtonComponent[] row5 = {
            new DiscordButtonComponent(ButtonStyle.Secondary, "jumpIcon", null, true, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":man_walking:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "jumpAdd", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Primary, "jumpSubtract", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_minus_sign:"))),
            new DiscordButtonComponent(ButtonStyle.Secondary, "disabled54", " ", true),
            new DiscordButtonComponent(ButtonStyle.Danger, "exit", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:")))
        };
        DiscordSelectComponentOption[] colorOptions = new DiscordSelectComponentOption[Enum.GetValues(typeof(PixelModule.PixelEnum)).Length];
        foreach(int i in Enum.GetValues(typeof(PixelModule.PixelEnum))) {
            string name = Enum.GetName(typeof(PixelModule.PixelEnum), i);
            colorOptions[i] = new DiscordSelectComponentOption(name, name);
        }
        DiscordSelectComponent colorSelectComponent = new DiscordSelectComponent("color", "Select color to place:", colorOptions);

        if(start_x < 0) {
            start_x = 0;
        }
        if(start_x >= PixelModule.MapWidth) {
            start_x = PixelModule.MapWidth - 1;
        }
        if(start_y < 0) {
            start_y = 0;
        }
        if(start_y >= PixelModule.MapHeight) {
            start_y = PixelModule.MapHeight - 1;
        }

        int x = (int)start_x;
        int y = (int)start_y;
        int zoom = 9;
        int jumpAmount = 1;
        PixelModule.PixelEnum selectedColorEnum = PixelModule.PixelEnum.White;

        var embed = new DiscordEmbedBuilder {
            Title = "Pixel",
            Color = DiscordColor.Blue,
            ImageUrl = $"attachment://img{ctx.User.Id}.png"
        };
        embed.WithDescription($"{ctx.Guild.Name}'s canvas. ({x},{y}) is selected. {zoom} zoom. {jumpAmount} tiles per move.");

        await ctx.DeferAsync(true);

        //Create the initial bitmap
        SKSurface surface = GetPixelSurface(ctx, x - zoom / 2, y - zoom / 2, zoom);
        AddInteractUI(surface, zoom, selectedColorEnum);
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Check DMs for an interactive canvas!"));
        DiscordMessage msg;
        using(var fs = new FileStream($"PixelImages/img{ctx.User.Id}.png", FileMode.Open, FileAccess.Read)) {
            msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5).AddEmbed(embed).WithFile($"img{ctx.User.Id}.png", fs));
        }
        var interactivity = ctx.Client.GetInteractivity();
        while(true) {

            var input = await interactivity.WaitForButtonAsync(msg, ctx.User, timeoutOverride: TimeSpan.FromMinutes(1));

            if(input.TimedOut)
                break;
            await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

            //This is some unoptimal shit, whatever
            if(input.Result.Id == "colorChange") {
                await msg.DeleteAsync();
                await Task.Delay(500);
                embed.WithImageUrl("");
                embed.WithDescription("");
                msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(colorSelectComponent).AddEmbed(embed));

                var cInput = await interactivity.WaitForSelectAsync(msg, ctx.User, "color", timeoutOverride: TimeSpan.FromMinutes(1));

                if(cInput.TimedOut)
                    break;
                await cInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                if(Enum.TryParse(typeof(PixelModule.PixelEnum), cInput.Result.Values.First(), true, out var type)) {
                    selectedColorEnum = (PixelModule.PixelEnum)type;
                }
            }
            if(input.Result.Id == "exit") {
                break;
            }
            if(input.Result.Id == "place") {
                var map = Bot.Pixel.GetPixelMap(ctx.Guild.Id);
                map.WritePixel(x, y, selectedColorEnum);
                Bot.Pixel.SetPixelMap(map);
            }
            if(input.Result.Id == "moveUp") {
                for(int i = 0; i < jumpAmount; i++) {
                    if(y > 0)
                        y--;
                }
            }
            if(input.Result.Id == "moveDown") {
                for(int i = 0; i < jumpAmount; i++) {
                    if(y <= PixelModule.MapHeight)
                        y++;
                }
            }
            if(input.Result.Id == "moveLeft") {
                for(int i = 0; i < jumpAmount; i++) {
                    if(x > 0)
                        x--;
                }
            }
            if(input.Result.Id == "moveRight") {
                for(int i = 0; i < jumpAmount; i++) {
                    if(x <= PixelModule.MapHeight)
                        x++;
                }
            }
            if(input.Result.Id == "zoomIn") {
                if(zoom > 3)
                    zoom -= 2;
            }
            if(input.Result.Id == "zoomOut") {
                zoom += 2;
            }
            if(input.Result.Id == "jumpSubtract") {
                if(jumpAmount > 1)
                    jumpAmount--;
            }
            if(input.Result.Id == "jumpAdd") {
                jumpAmount ++;
            }

            //Update and show image
            surface = GetPixelSurface(ctx, x - zoom / 2, y - zoom / 2, zoom);
            AddInteractUI(surface, zoom, selectedColorEnum);

            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
            embed.WithImageUrl($"attachment://img{ctx.User.Id}.png");
            embed.WithDescription($"{ctx.Guild.Name}'s canvas. ({x},{y}) is selected. {zoom} zoom. {jumpAmount} tiles per move.");
            using(var fs = new FileStream($"PixelImages/img{ctx.User.Id}.png", FileMode.Open, FileAccess.Read)) {
                await msg.DeleteAsync();
                await Task.Delay(500);
                msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5).AddEmbed(embed).WithFile($"img{ctx.User.Id}.png", fs));
            }
        }
        using(var fs = new FileStream($"img{ctx.User.Id}.png", FileMode.Open, FileAccess.Read)) {
            await msg.DeleteAsync();
            embed.WithDescription($"{ctx.Guild.Name}'s canvas.");
            await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).WithFile($"img{ctx.User.Id}.png", fs));
        }
        File.Delete($"img{ctx.User.Id}.png");
    }
}

[SlashCommandGroup("voice", "voice")]
class VoiceCommands : ApplicationCommandModule {

    [SlashCommand("join", "Join Channel")]
    public async Task JoinChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Voice.CanUserSummon(ctx);
        if(!canUse)
            return;

        await VGconn.Connect(ctx.Member.VoiceState.Channel);
        await BotUtils.CreateBasicResponse(ctx, $"Joined {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("leave", "Leave channel")]
    public async Task LeaveChannel(InteractionContext ctx) {
        (bool canUse, var VGconn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        await VGconn.Disconnect();
        await BotUtils.CreateBasicResponse(ctx, $"Left {ctx.Member.VoiceState.Channel.Name}!");
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("search", "what to play")] string search) {
        VoiceGuildConnection VGconn = Bot.Voice.GetGuildConnection(ctx);
        bool canUse;
        if(VGconn.Conn == null) {
            (canUse, VGconn) = await Bot.Voice.CanUserSummon(ctx);
        } else {
            if(Bot.Voice.IsBeingUsed(VGconn.Conn))
                (canUse, VGconn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
            else
                (canUse, VGconn) = await Bot.Voice.CanUserSummon(ctx);
        }

        if(!canUse)
            return;

        await ctx.DeferAsync();
        var tracks = await Bot.Voice.GetTrackAsync(search, VGconn.Node);
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
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);

        if(!canUse)
            return;

        VGConn.CurrentTrack = null; //some shitty workaround to avoid looping the current song~!
        await VGConn.ProgressQueue();
        await BotUtils.CreateBasicResponse(ctx, "Skipped!");
    }

    [SlashCommand("volume", "make music quiet or loud")]
    public async Task Volume(InteractionContext ctx, [Option("volume", "how loud")] long volume) {
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
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
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);

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
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
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
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
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
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int songCount = VGConn.TrackQueue.Count();
        VGConn.TrackQueue = new();
        await BotUtils.CreateBasicResponse(ctx, $"Cleared {songCount} songs from queue!");
    }

    [SlashCommand("queue", "Show the queue")]
    public async Task List(InteractionContext ctx, [Option("page", "what page")] long page = 1) {
        (bool canUse, var VGConn) = await Bot.Voice.CanUserUseModifyCommand(ctx);
        if(!canUse)
            return;

        int pageCount = (int)Math.Ceiling((decimal)VGConn.TrackQueue.Count / (decimal)10);
        int activePage = Math.Min(Math.Max(1,(int)page), pageCount);
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
