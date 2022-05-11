namespace DiscordBotRewrite.Modules;
public class PixelModule {
    #region Constants
    public const int MapWidth = 100;
    public const int MapHeight = 100;
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
    public void CreateImage(InteractionContext ctx, int x, int y, int zoom) {
        SKSurface surface = CreateSurface(GetPixelMap(ctx.Guild.Id), x - zoom / 2, y - zoom / 2, zoom);
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
    }
    public void CreateImageWithUI(InteractionContext ctx, int x, int y, int zoom, PixelEnum selectedPixel) {
        SKSurface surface = CreateSurface(GetPixelMap(ctx.Guild.Id), x - zoom / 2, y - zoom / 2, zoom);
        if(PixelDict.TryGetValue((uint)selectedPixel, out SKColor color))
            DrawCenterPixel(surface.Canvas, zoom, color);
        AddOutline(surface.Canvas, zoom, 2, SKColors.Silver);
        surface.Snapshot().SaveToPng($"PixelImages/img{ctx.User.Id}.png");
    }
    #endregion

    #region Private
    void SavePixelMap(PixelMap data) {
        PixelMaps.AddOrUpdate(data.Id, data);
        SaveJson(PixelMaps, PixelMap.JsonLocation);
    }
    PixelMap GetPixelMap(ulong id) {
        if(!PixelMaps.TryGetValue(id, out PixelMap pixelMap)) {
            pixelMap = new PixelMap(id);
            PixelMaps.Add(id, pixelMap);
        }
        return pixelMap;
    }
    SKSurface CreateSurface(PixelMap map, int anchorX, int anchorY, int distance) {
        int scale = 500 / distance;

        SKImageInfo imageInfo = new SKImageInfo(500, 500);
        SKSurface surface = SKSurface.Create(imageInfo);
        SKCanvas canvas = surface.Canvas;

        for(int x = 0; x < distance; x++) {
            for(int y = 0; y < distance; y++) {
                int curX = x + anchorX;
                int curY = y + anchorY;
                SKColor color;
                bool exists = true;
                if(curX < 0 || curX >= MapWidth || curY < 0 || curY >= MapHeight) {
                    color = SKColors.Black.WithAlpha(255);
                    exists = false;
                } else if(!PixelDict.TryGetValue((uint)map.PixelState[curX, curY], out color)) {
                    color = SKColors.White;
                }
                SKPaint paint = new SKPaint();
                paint.Color = color;
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = false;

                if(exists)
                    canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                else
                    canvas.DrawHatchedRect(x * scale, y * scale, scale, scale, paint, 5);
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

    public class PixelMap {
        public const string JsonLocation = "Json/PixelMaps.json";

        [JsonProperty("guild_id")]
        public readonly ulong Id;
        [JsonProperty("pixel_data")]
        public PixelEnum[,] PixelState;

        public PixelMap(ulong id){
            PixelState = new PixelEnum[MapWidth, MapHeight];
            Id = id;
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