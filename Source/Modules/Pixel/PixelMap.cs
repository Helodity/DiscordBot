namespace DiscordBotRewrite.Modules;
using static DiscordBotRewrite.Modules.PixelModule;
public class PixelMap {
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
