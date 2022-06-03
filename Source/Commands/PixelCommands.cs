using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Attributes;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using static DiscordBotRewrite.Global.Style;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("pixel", "r/place but inside a discord bot")]
    class PixelCommands : ApplicationCommandModule {
        #region View
        [SlashCommand("view", "look at the current canvas")]
        public async Task ViewCanvas(InteractionContext ctx) {
            await ctx.CreateResponseAsync($"Loading...", true);
            string imagePath = $"PixelImages/img{ctx.User.Id}.png";
            Bot.Modules.Pixel.CreateImage(ctx);
            using(var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read)) {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddFile(imagePath, fs).AsEphemeral());
            }
            File.Delete(imagePath);
        }
        #endregion

        #region Interact
        [SlashCommand("interact", "View an interactable map")]
        public async Task Interact(InteractionContext ctx,
            [Option("x", "x to start at")] long x = 5,
            [Option("y", "y to start at")] long y = 5) {

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

            List<DiscordSelectComponentOption> colorOptions = new List<DiscordSelectComponentOption>();
            ((int[])Enum.GetValues(typeof(PixelModule.PixelEnum))).ToList().ForEach(x => {
                string name = Enum.GetName(typeof(PixelModule.PixelEnum), x);
                colorOptions.Add(new DiscordSelectComponentOption(name, name));
            });

            DiscordSelectComponent colorSelectComponent = new DiscordSelectComponent("color", "Select color to place:", colorOptions);

            PixelMap map = Bot.Modules.Pixel.GetPixelMap(ctx.Guild.Id);

            int curX = (int)Math.Clamp(x, 0, map.Width - 1);
            int curY = (int)Math.Clamp(y, 0, map.Height - 1);
            int zoom = 9;
            int jumpAmount = 1;
            string imagePath = $"PixelImages/img{ctx.User.Id}.png";
            PixelModule.PixelEnum curColor = PixelModule.PixelEnum.White;

            await ctx.CreateResponseAsync("Check DMs for an interactive canvas! (It may take some time to load)", true);

            DiscordMessage msg;
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Pixel")
                .WithColor(DefaultColor)
                .WithImageUrl($"attachment://{Path.GetFileName(imagePath)}");

            var interactivity = ctx.Client.GetInteractivity();
            while(true) {
                embed.WithDescription($"{ctx.Guild.Name}'s canvas. ({curX},{curY}) is selected. {zoom} zoom. {jumpAmount} tiles per move.");
                Bot.Modules.Pixel.CreateImageWithUI(ctx, curX, curY, zoom, curColor);
                using(var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read)) {
                    msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5).AddEmbed(embed).WithFile(Path.GetFileName(imagePath), fs));
                }
                var input = await interactivity.WaitForButtonAsync(msg, ctx.User, timeoutOverride: TimeSpan.FromMinutes(1));

                if(input.TimedOut)
                    break;
                await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                //From what I can tell there isn't a better way to do this :/
                if(input.Result.Id == "colorChange") {
                    await msg.DeleteAsync();
                    await Task.Delay(500);
                    embed.WithDescription("");
                    msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(colorSelectComponent).AddEmbed(embed));

                    var cInput = await interactivity.WaitForSelectAsync(msg, ctx.User, "color", timeoutOverride: TimeSpan.FromMinutes(1));

                    if(cInput.TimedOut)
                        break;
                    await cInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                    if(Enum.TryParse(typeof(PixelModule.PixelEnum), cInput.Result.Values.First(), true, out var type)) {
                        curColor = (PixelModule.PixelEnum)type;
                    }
                }
                if(input.Result.Id == "exit") {
                    break;
                }
                if(input.Result.Id == "place") {
                    if(!Bot.Modules.Pixel.TryPlacePixel(ctx.Guild.Id, ctx.User.Id, curX, curY, curColor))
                        await ctx.Member.SendMessageAsync($"Can't place a pixel, wait {map.TimeUntilNextPlace(ctx.User.Id)} seconds!");
                }
                if(input.Result.Id == "moveUp") {
                    for(int i = 0; i < jumpAmount; i++) {
                        if(curY > 0)
                            curY--;
                    }
                }
                if(input.Result.Id == "moveDown") {
                    for(int i = 0; i < jumpAmount; i++) {
                        if(curY <= map.Height)
                            curY++;
                    }
                }
                if(input.Result.Id == "moveLeft") {
                    for(int i = 0; i < jumpAmount; i++) {
                        if(curX > 0)
                            curX--;
                    }
                }
                if(input.Result.Id == "moveRight") {
                    for(int i = 0; i < jumpAmount; i++) {
                        if(curX <= map.Width)
                            curX++;
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

                await msg.DeleteAsync();
                await Task.Delay(500);
            }

            using(var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read)) {
                await msg.DeleteAsync();
                embed.WithDescription($"{ctx.Guild.Name}'s canvas.");
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).WithFile(Path.GetFileName(imagePath), fs));
            }
            File.Delete(imagePath);
        }
        #endregion

        #region Resize
        [SlashCommand("resize", "Scale the canvas to fit your needs")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Resize(InteractionContext ctx,
            [Option("x", "new x size")] long x,
            [Option("y", "new y size")] long y) {

            Bot.Modules.Pixel.ResizeMap(ctx.Guild.Id, (int)x, (int)y);
            await ctx.CreateResponseAsync($"Resized Canvas to ({x},{y})!");
        }
        #endregion

        #region Set Cooldown
        [SlashCommand("cooldown", "Set how often a pixel can be placed")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Cooldown(InteractionContext ctx, [Option("duration", "Time in seconds")] long duration) {
            duration = System.Math.Max(0, duration);
            Bot.Modules.Pixel.SetCooldown(ctx.Guild.Id, (uint)duration);
            await ctx.CreateResponseAsync($"Set cooldown to {duration} seconds");
        }
        #endregion
    }
}