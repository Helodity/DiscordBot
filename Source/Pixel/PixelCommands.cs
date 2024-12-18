﻿using DiscordBotRewrite.Global.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DiscordBotRewrite.Global.Extensions;
using SkiaSharp;
using System.Linq;

namespace DiscordBotRewrite.Pixel
{
    [SlashCommandGroup("pixel", "r/place but inside a discord bot")]
    class PixelCommands : ApplicationCommandModule
    {
        #region View
        [SlashCommand("view", "look at the current canvas")]
        public async Task ViewCanvas(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync($"Loading...", true);
            string imagePath = $"PixelImages/img{ctx.User.Id}.png";
            Bot.Modules.Pixel.CreateImage(ctx);
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddFile(imagePath, fs).AsEphemeral());
            }
            File.Delete(imagePath);
        }
        #endregion

        #region Interact
        [SlashCommand("interact", "View an interactable map")]
        public async Task Interact(InteractionContext ctx,
            [Option("x", "x to start at")] long x = 5,
            [Option("y", "y to start at")] long y = 5)
        {

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

            foreach(PixelColor c in PixelColor.ColorDict) {
                colorOptions.Add(new DiscordSelectComponentOption(c.Name, c.Name));
            }

            DiscordSelectComponent colorSelectComponent = new DiscordSelectComponent("color", "Select color to place:", colorOptions);

            PixelMap map = Bot.Modules.Pixel.GetPixelMap((long)ctx.Guild.Id);

            int curX = (int)Math.Clamp(x, 0, map.Width - 1);
            int curY = (int)Math.Clamp(y, 0, map.Height - 1);
            int zoom = 9;
            int jumpAmount = 1;
            string imagePath = $"PixelImages/img{ctx.User.Id}.png";
            PixelColor curColor = PixelColor.ColorDict[0];

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Description = "Check dms for a canvas!",
                Color = Bot.Style.SuccessColor
            }).AsEphemeral());

            DiscordMessage msg;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Pixel")
                .WithColor(Bot.Style.DefaultColor)
                .WithImageUrl($"attachment://{Path.GetFileName(imagePath)}")
                .WithDescription($"{ctx.Guild.Name}'s canvas. ({curX},{curY}) is selected. {zoom} zoom. {jumpAmount} tiles per move.");
            Bot.Modules.Pixel.CreateImageWithUI(ctx, curX, curY, zoom, curColor);
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                msg = await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5).AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
            }
            DSharpPlus.Interactivity.InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            while (true)
            {
                DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs> input = await interactivity.WaitForButtonAsync(msg, ctx.User);

                if (input.TimedOut)
                {
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        await msg.DeleteAsync();
                        embed.WithDescription($"{ctx.Guild.Name}'s canvas.").WithColor(Bot.Style.DefaultColor);
                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
                    }
                    File.Delete(imagePath);
                    return;
                }
                await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                bool recreateMessage = true;

                if (input.Result.Id == "colorChange")
                {
                    await msg.DeleteAsync();
                    msg = await input.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(colorSelectComponent));

                    DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs> cInput = await interactivity.WaitForSelectAsync(msg, ctx.User, "color");

                    if (cInput.TimedOut)
                    {
                        break;
                    }

                    await cInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    PixelColor newColor = PixelColor.ColorDict.FirstOrDefault(x => x.Name == cInput.Result.Values.First());
                    if(newColor != null) {
                        curColor = newColor;
                    }
                    recreateMessage = false;
                    await msg.DeleteAsync();
                    Bot.Modules.Pixel.CreateImageWithUI(ctx, curX, curY, zoom, curColor);
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        msg = await cInput.Result.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5)
                            .AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
                    }
                }
                if (input.Result.Id == "exit")
                {
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        await msg.DeleteAsync();
                        embed.WithDescription($"{ctx.Guild.Name}'s canvas.").WithColor(Bot.Style.DefaultColor);
                        await input.Result.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder().AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
                    }
                    File.Delete(imagePath);
                    return;
                }
                if (input.Result.Id == "moveUp")
                {
                    for (int i = 0; i < jumpAmount; i++)
                    {
                        if (curY > 0)
                        {
                            curY--;
                        }
                    }
                }
                if (input.Result.Id == "moveDown")
                {
                    for (int i = 0; i < jumpAmount; i++)
                    {
                        if (curY <= map.Height)
                        {
                            curY++;
                        }
                    }
                }
                if (input.Result.Id == "moveLeft")
                {
                    for (int i = 0; i < jumpAmount; i++)
                    {
                        if (curX > 0)
                        {
                            curX--;
                        }
                    }
                }
                if (input.Result.Id == "moveRight")
                {
                    for (int i = 0; i < jumpAmount; i++)
                    {
                        if (curX <= map.Width)
                        {
                            curX++;
                        }
                    }
                }
                if (input.Result.Id == "zoomIn")
                {
                    if (zoom > 3)
                    {
                        zoom -= 2;
                    }
                }
                if (input.Result.Id == "zoomOut")
                {
                    zoom += 2;
                }
                if (input.Result.Id == "jumpSubtract")
                {
                    if (jumpAmount > 1)
                    {
                        jumpAmount--;
                    }
                }
                if (input.Result.Id == "jumpAdd")
                {
                    jumpAmount++;
                }
                embed.WithDescription($"{ctx.Guild.Name}'s canvas. ({curX},{curY}) is selected. {zoom} zoom. {jumpAmount} tiles per move.");
                embed.WithColor(Bot.Style.DefaultColor);
                if (input.Result.Id == "place")
                {
                    if (!Bot.Modules.Pixel.TryPlacePixel((long)ctx.Guild.Id, (long)ctx.User.Id, curX, curY, curColor))
                    {
                        embed.WithColor(Bot.Style.ErrorColor);
                        embed.WithDescription($"You can place another pixel {map.NextPlaceTime((long)ctx.User.Id).ToTimestamp()}!");
                    }
                }

                if (recreateMessage)
                {
                    Bot.Modules.Pixel.CreateImageWithUI(ctx, curX, curY, zoom, curColor);
                    await msg.DeleteAsync();
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        msg = await input.Result.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .AddComponents(row1).AddComponents(row2).AddComponents(row3).AddComponents(row4).AddComponents(row5)
                            .AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
                    }
                }
            }
        }
        #endregion

        #region Resize
        [SlashCommand("resize", "Scale the canvas to fit your needs")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Resize(InteractionContext ctx,
            [Option("x", "new x size")] long x,
            [Option("y", "new y size")] long y)
        {

            Bot.Modules.Pixel.ResizeMap((long)ctx.Guild.Id, (int)x, (int)y);
            await ctx.CreateResponseAsync($"Resized Canvas to ({x},{y})!");
        }
        #endregion

        #region Set Cooldown
        [SlashCommand("cooldown", "Set how often a pixel can be placed")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Cooldown(InteractionContext ctx, [Option("duration", "Time in seconds")] long duration)
        {
            duration = Math.Max(0, duration);
            Bot.Modules.Pixel.SetCooldown((long)ctx.Guild.Id, (uint)duration);
            await ctx.CreateResponseAsync($"Set cooldown to {duration} seconds");
        }
        #endregion
    }
}