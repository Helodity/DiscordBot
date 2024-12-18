﻿using DiscordBotRewrite.Global.Extensions;
using DSharpPlus.SlashCommands;
using SkiaSharp;

namespace DiscordBotRewrite.Pixel
{
    public class PixelModule
    {
        #region Constructors
        public PixelModule()
        {
            Bot.Database.CreateTable<PixelMap>();
            Bot.Database.CreateTable<PlaceCooldown>();
        }
        #endregion

        #region Public
        public bool TryPlacePixel(long guildId, long userId, int x, int y, PixelColor color)
        {
            PixelMap map = GetPixelMap(guildId);

            PlaceCooldown cooldown = GetPlaceCooldown(guildId, userId);
            if (cooldown != null && DateTime.Compare(DateTime.Now, cooldown.EndTime) < 0)
            {
                return false;
            }
            if (cooldown == null)
            {
                cooldown = new PlaceCooldown(guildId, userId);
                Bot.Database.Insert(cooldown);
            }

            cooldown.EndTime = DateTime.Now.AddSeconds(map.PlaceCooldown);
            Bot.Database.Update(cooldown);

            map.SetPixel(x, y, color);

            return true;
        }
        public void CreateImage(InteractionContext ctx)
        {
            PixelMap map = GetPixelMap((long)ctx.Guild.Id);

            int pixelSize = Math.Max(1, 500 / Math.Max(map.Width, map.Height));
            SKPointI anchor = new SKPointI(0, 0);
            SKPointI end = new SKPointI(map.Width - 1, map.Height - 1);

            SKSurface surface = CreateSurface(map, anchor, end, pixelSize);
            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public void CreateImage(InteractionContext ctx, int x, int y, int zoom)
        {
            PixelMap map = GetPixelMap((long)ctx.Guild.Id);

            int halfDistance = zoom / 2;
            SKPointI anchor = new SKPointI(x - halfDistance, y - halfDistance);
            SKPointI end = new SKPointI(x + halfDistance, y + halfDistance);

            SKSurface surface = CreateSurface(map, anchor, end, 500 / zoom);
            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public void CreateImageWithUI(InteractionContext ctx, int x, int y, int zoom, PixelColor selectedColor)
        {
            PixelMap map = GetPixelMap((long)ctx.Guild.Id);
            int halfDistance = zoom / 2;
            SKPointI anchor = new SKPointI(x - halfDistance, y - halfDistance);
            SKPointI end = new SKPointI(x + halfDistance, y + halfDistance);

            SKSurface surface = CreateSurface(map, anchor, end, 500 / zoom);

            DrawCenterPixel(surface.Canvas, zoom, selectedColor.Color);
            AddOutline(surface.Canvas, zoom, 2, SKColors.Silver);

            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public PixelMap GetPixelMap(long id)
        {
            PixelMap map = Bot.Database.Table<PixelMap>().FirstOrDefault(x => x.GuildId == id);
            if (map == null)
            {
                map = new PixelMap(id);
                Bot.Database.Insert(map);
            }
            if (map.Width * map.Height != map.PixelState.Length)
            {
                map.Resize(map.Width, map.Height);
                Bot.Database.Update(map);
            }
            return map;
        }
        public PlaceCooldown GetPlaceCooldown(long guildId, long userId)
        {
            return Bot.Database.Table<PlaceCooldown>().FirstOrDefault(x => x.GuildID == guildId && x.UserID == userId);
        }
        public void ResizeMap(long guildId, int width, int height)
        {
            PixelMap map = GetPixelMap(guildId);
            map.Resize(width, height);
            Bot.Database.Update(map);
        }
        public void SetCooldown(long guildId, uint duration)
        {
            PixelMap map = GetPixelMap(guildId);
            map.PlaceCooldown = duration;
            Bot.Database.Update(map);
        }
        #endregion

        #region Private
        SKSurface CreateSurface(PixelMap map, SKPointI anchor, SKPointI end, int pixelSize)
        {
            int xDist = end.X + 1 - anchor.X;
            int yDist = end.Y + 1 - anchor.Y;

            SKImageInfo imageInfo = new SKImageInfo(xDist * pixelSize, yDist * pixelSize);
            SKSurface surface = SKSurface.Create(imageInfo);
            SKCanvas canvas = surface.Canvas;

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = false
            };
            for (int x = 0; x < xDist; x++)
            {
                for (int y = 0; y < yDist; y++)
                {
                    //Cleanup
                    int absX = x + anchor.X;
                    int absY = y + anchor.Y;
                    SKColor color;
                    bool exists = true;
                    if (absX < 0 || absX >= map.Width || absY < 0 || absY >= map.Height)
                    {
                        color = SKColors.Black.WithAlpha(255);
                        exists = false;
                    }
                    else
                    {
                        PixelColor pColor = map.GetPixelColor(absX, absY);
                        if(pColor != null)
                        {
                            color = pColor.Color;
                        }
                        else
                        {
                            color = SKColors.White;
                        }
                    }
                    paint.Color = color;


                    if (exists)
                    {
                        canvas.DrawRect(x * pixelSize, y * pixelSize, pixelSize, pixelSize, paint);
                    }
                    else
                    {
                        canvas.DrawHatchedRect(x * pixelSize, y * pixelSize, pixelSize, pixelSize, paint, 3);
                    }
                }
            }




            return surface;
        }
        void AddOutline(SKCanvas canvas, int zoom, int thickness, SKColor color)
        {
            SKPaint paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = thickness,
                IsAntialias = false
            };

            int pixelSize = 500 / zoom;

            canvas.DrawRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint);
        }
        void DrawCenterPixel(SKCanvas canvas, int zoom, SKColor color)
        {
            SKPaint paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                IsAntialias = false,
                StrokeWidth = 1
            };

            int pixelSize = 500 / zoom;

            canvas.DrawHatchedRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint, 5);
        }

        #endregion
    }
}