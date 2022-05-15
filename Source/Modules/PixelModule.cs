﻿namespace DiscordBotRewrite.Modules;
public class PixelModule {
    #region Constants
    //Make sure to update both when adding a color
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

    Dictionary<ulong, PixelMap> PixelMaps;
    public PixelModule() {
        PixelMaps = LoadJson<Dictionary<ulong, PixelMap>>(PixelMap.JsonLocation);
    }

    #region Public
    public void PlacePixel(ulong guildId, int x, int y, PixelEnum color) {
        var map = GetPixelMap(guildId);
        map.PixelState[x, y] = color;
        SavePixelMap(map);
    }
    public void CreateImage(InteractionContext ctx) {
        PixelMap map = GetPixelMap(ctx.Guild.Id);
        int pixelSize = 500 / Math.Max(map.Width, map.Height);
        SKSurface surface = CreateSurface(map, 0, 0, map.Width - 1, map.Height - 1, Math.Max(1, pixelSize));
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
    }
    public void CreateImage(InteractionContext ctx, int x, int y, int zoom) {
        PixelMap map = GetPixelMap(ctx.Guild.Id);
        int halfDistance = zoom / 2;
        SKSurface surface = CreateSurface(map, x - halfDistance, y - halfDistance, x + halfDistance, y + halfDistance, 500 / zoom);
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
    }
    public void CreateImageWithUI(InteractionContext ctx, int x, int y, int zoom, PixelEnum selectedPixel) {
        PixelMap map = GetPixelMap(ctx.Guild.Id);
        int halfDistance = zoom / 2;
        SKSurface surface = CreateSurface(map, x - halfDistance, y - halfDistance, x + halfDistance, y + halfDistance, 500 / zoom);

        if(PixelDict.TryGetValue((uint)selectedPixel, out SKColor color))
            DrawCenterPixel(surface.Canvas, zoom, color);
        AddOutline(surface.Canvas, zoom, 2, SKColors.Silver);

        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
    }

    public Point GetMapSize(ulong guildId) {
        var map = GetPixelMap(guildId);
        return new Point(map.Width, map.Height);
    }

    public void ResizeMap(ulong guildId, int width, int height) {
        var map = GetPixelMap(guildId);
        map.ChangeSize(width, height);
        SavePixelMap(map);
    }
    #endregion

    #region Private
    PixelMap GetPixelMap(ulong id) {
        if(!PixelMaps.TryGetValue(id, out PixelMap pixelMap)) {
            pixelMap = new PixelMap(id);
            PixelMaps.Add(id, pixelMap);
        }

        if(pixelMap.Width * pixelMap.Height != pixelMap.PixelState.Length) {
            pixelMap.ChangeSize(pixelMap.PixelState.GetLength(0), pixelMap.PixelState.GetLength(1));
            SavePixelMap(pixelMap);
        }

        return pixelMap;
    }
    void SavePixelMap(PixelMap data) {
        PixelMaps.AddOrUpdate(data.Id, data);
        SaveJson(PixelMaps, PixelMap.JsonLocation);
    }
    SKSurface CreateSurface(PixelMap map, int anchorX, int anchorY, int endX, int endY, int pixelSize) {
        int xDist = endX + 1 - anchorX;
        int yDist = endY + 1 - anchorY;

        SKImageInfo imageInfo = new SKImageInfo(xDist * pixelSize, yDist * pixelSize);
        SKSurface surface = SKSurface.Create(imageInfo);
        SKCanvas canvas = surface.Canvas;

        SKPaint paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = false;
        for(int x = 0; x < xDist; x++) {
            for(int y = 0; y < yDist; y++) {
                int absX = x + anchorX;
                int absY = y + anchorY;
                SKColor color;
                bool exists = true;
                if(absX < 0 || absX >= map.Width || absY < 0 || absY >= map.Height) {
                    color = SKColors.Red.WithAlpha(255);
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
        paint.Style = SKPaintStyle.Stroke;
        paint.IsAntialias = false;
        paint.StrokeWidth = 1;

        int pixelSize = 500 / zoom;

        canvas.DrawHatchedRect(pixelSize * (zoom / 2), pixelSize * (zoom / 2), pixelSize, pixelSize, paint, 5);
    }
    #endregion
    class PixelMap {
        public const string JsonLocation = "Json/PixelMaps.json";

        [JsonProperty("guild_id")]
        public readonly ulong Id;
        [JsonProperty("width")]
        public int Width;
        [JsonProperty("height")]
        public int Height;
        [JsonProperty("pixel_data")]
        public PixelEnum[,] PixelState;

        public PixelMap(ulong id, int width = 100, int height = 100) {
            Width = width;
            Height = height;
            PixelState = new PixelEnum[Width, Height];
            Id = id;
        }

        public void ChangeSize(int width, int height) {
            PixelEnum[,] old = PixelState;
            int maxY = Math.Min(old.GetLength(1), height);
            int maxX = Math.Min(old.GetLength(0), width);

            Width = width;
            Height = height;
            PixelState = new PixelEnum[Width, Height];
            for(int y = 0; y < maxY; y++) {
                for(int x = 0; x < maxX; x++) {
                    PixelState[x, y] = old[x, y];
                }
            }
        }

    }
}
public static class SKExtensions {
    public static void SaveToPng(this SKImage image, string path) {
        using(SKData data = image.Encode(SKEncodedImageFormat.Png, 100)) {
            if(!File.Exists(path))
                FileExtension.CreateFileWithPath(path);

            using(var stream = File.OpenWrite(path)) {
                data.SaveTo(stream);
            }
        }
    }
    public static void DrawHatchedRect(this SKCanvas canvas, float x, float y, float w, float h, SKPaint paint, int hatchDistance) {
        SKPath path = new SKPath();
        for(int i = 0; i < w; i += hatchDistance) {
            path.MoveTo(x + i, y);
            float size = Math.Min(h,Math.Min(w, w - i));
            path.LineTo(x + i + size, y + size);
        }
        for(int i = 0; i < h; i += hatchDistance) {
            path.MoveTo(x, y + i);
            float size = Math.Min(w,Math.Min(h, h - i));
            path.LineTo(x + size, y + i + size);
        }
        //Hatching only works with stroke paint, temporarily set it to that to ensure proper drawing
        SKPaintStyle oldStyle = paint.Style;
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawPath(path, paint);
        paint.Style = oldStyle;
    }
}