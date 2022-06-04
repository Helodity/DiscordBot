using System;
using System.Collections.Generic;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus.SlashCommands;
using SkiaSharp;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Modules {
    public class PixelModule {
        #region Properties
        #region Constants
        //Update both when adding a color
        readonly Dictionary<uint, SKColor> PixelDict = new Dictionary<uint, SKColor>()
        {
        {0,  SKColors.White},
        {1,  SKColors.Red},
        {2,  SKColors.Orange},
        {3,  SKColors.Yellow},
        {4,  SKColors.Green},
        {5,  SKColors.Blue},
        {6,  SKColors.Purple},
        {7,  SKColors.Brown},
        {8,  SKColors.Pink},
        {9,  SKColors.Gray},
        {10, SKColors.Black},
        {11, SKColors.Cyan}
    };
        public enum PixelEnum {
            White,
            Red,
            Orange,
            Yellow,
            Green,
            Blue,
            Purple,
            Brown,
            Pink,
            Gray,
            Black,
            Cyan
        };
        #endregion

        readonly Dictionary<ulong, PixelMap> PixelMaps;
        #endregion

        #region Constructors
        public PixelModule() {
            PixelMaps = LoadJson<Dictionary<ulong, PixelMap>>(PixelMap.JsonLocation);
        }
        #endregion

        #region Public
        public bool TryPlacePixel(ulong guildId, ulong userId, int x, int y, PixelEnum color) {
            var map = GetPixelMap(guildId);

            if(Cooldown.UserHasCooldown(userId, ref map.PlaceCooldowns)) {
                return false;
            }
            map.PlaceCooldowns.Add(userId, new Cooldown(DateTime.Now.AddSeconds(map.PlaceCooldown)));
            map.PixelState[x, y] = color;
            SavePixelMap(map);
            return true;
        }
        public void CreateImage(InteractionContext ctx) {
            PixelMap map = GetPixelMap(ctx.Guild.Id);

            int pixelSize = Math.Max(1, 500 / Math.Max(map.Width, map.Height));
            SKPointI anchor = new SKPointI(0, 0);
            SKPointI end = new SKPointI(map.Width - 1, map.Height - 1);

            SKSurface surface = CreateSurface(map, anchor, end, pixelSize);
            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public void CreateImage(InteractionContext ctx, int x, int y, int zoom) {
            PixelMap map = GetPixelMap(ctx.Guild.Id);

            int halfDistance = zoom / 2;
            SKPointI anchor = new SKPointI(x - halfDistance, y - halfDistance);
            SKPointI end = new SKPointI(x + halfDistance, y + halfDistance);

            SKSurface surface = CreateSurface(map, anchor, end, 500 / zoom);
            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public void CreateImageWithUI(InteractionContext ctx, int x, int y, int zoom, PixelEnum selectedPixel) {
            PixelMap map = GetPixelMap(ctx.Guild.Id);
            int halfDistance = zoom / 2;
            SKPointI anchor = new SKPointI(x - halfDistance, y - halfDistance);
            SKPointI end = new SKPointI(x + halfDistance, y + halfDistance);

            SKSurface surface = CreateSurface(map, anchor, end, 500 / zoom);

            if(PixelDict.TryGetValue((uint)selectedPixel, out SKColor color))
                DrawCenterPixel(surface.Canvas, zoom, color);
            AddOutline(surface.Canvas, zoom, 2, SKColors.Silver);

            surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
        }
        public PixelMap GetPixelMap(ulong id) {
            if(!PixelMaps.TryGetValue(id, out PixelMap pixelMap)) {
                pixelMap = new PixelMap(id);
                PixelMaps.Add(id, pixelMap);
            }

            if(pixelMap.Width * pixelMap.Height != pixelMap.PixelState.Length) {
                pixelMap.Resize(pixelMap.PixelState.GetLength(0), pixelMap.PixelState.GetLength(1));
                SavePixelMap(pixelMap);
            }

            return pixelMap;
        }
        public void ResizeMap(ulong guildId, int width, int height) {
            var map = GetPixelMap(guildId);
            map.Resize(width, height);
            SavePixelMap(map);
        }
        public void SetCooldown(ulong guildId, uint duration) {
            var map = GetPixelMap(guildId);
            map.PlaceCooldown = duration;
            SavePixelMap(map);
        }
        #endregion

        #region Private
        void SavePixelMap(PixelMap data) {
            PixelMaps.AddOrUpdate(data.GuildId, data);
            SaveJson(PixelMaps, PixelMap.JsonLocation);
        }
        SKSurface CreateSurface(PixelMap map, SKPointI anchor, SKPointI end, int pixelSize) {
            int xDist = end.X + 1 - anchor.X;
            int yDist = end.Y + 1 - anchor.Y;

            SKImageInfo imageInfo = new SKImageInfo(xDist * pixelSize, yDist * pixelSize);
            SKSurface surface = SKSurface.Create(imageInfo);
            SKCanvas canvas = surface.Canvas;

            SKPaint paint = new SKPaint {
                Style = SKPaintStyle.Fill,
                IsAntialias = false
            };
            for(int x = 0; x < xDist; x++) {
                for(int y = 0; y < yDist; y++) {
                    int absX = x + anchor.X;
                    int absY = y + anchor.Y;
                    SKColor color;
                    bool exists = true;
                    if(absX < 0 || absX >= map.Width || absY < 0 || absY >= map.Height) {
                        color = SKColors.Black.WithAlpha(255);
                        exists = false;
                    } else if(!PixelDict.TryGetValue((uint)map.PixelState[absX, absY], out color)) {
                        color = SKColors.White;
                    }
                    paint.Color = color;


                    if(exists)
                        canvas.DrawRect(x * pixelSize, y * pixelSize, pixelSize, pixelSize, paint);
                    else
                        canvas.DrawHatchedRect(x * pixelSize, y * pixelSize, pixelSize, pixelSize, paint, 3);
                }
            }




            return surface;
        }
        void AddOutline(SKCanvas canvas, int zoom, int thickness, SKColor color) {
            SKPaint paint = new SKPaint {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = thickness,
                IsAntialias = false
            };

            int pixelSize = 500 / zoom;

            canvas.DrawRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint);
        }
        void DrawCenterPixel(SKCanvas canvas, int zoom, SKColor color) {
            SKPaint paint = new SKPaint {
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