namespace DiscordBotRewrite.Commands;

[SlashCommandGroup("pixel", "r/place but inside a discord bot")]
class PixelCommands : ApplicationCommandModule {

    public SKSurface GetPixelSurface(InteractionContext ctx, int anchorX, int anchorY, int distance) {
        var mapData = Bot.Modules.Pixel.GetPixelMap(ctx.Guild.Id);
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

        await BotUtils.CreateBasicResponse(ctx, "Check DMs for an interactive canvas! (It may take some time to load)", true);

        //Create the initial bitmap
        SKSurface surface = GetPixelSurface(ctx, x - zoom / 2, y - zoom / 2, zoom);
        AddInteractUI(surface, zoom, selectedColorEnum);
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");

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
                var map = Bot.Modules.Pixel.GetPixelMap(ctx.Guild.Id);
                map.WritePixel(x, y, selectedColorEnum);
                Bot.Modules.Pixel.SetPixelMap(map);
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
                jumpAmount++;
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
