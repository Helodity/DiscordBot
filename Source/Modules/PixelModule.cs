namespace DiscordBotRewrite.Modules;
public class PixelModule {

    Dictionary<ulong, PixelMap> PixelMaps;

    public const int MapWidth = 100;
    public const int MapHeight = 100;

    //Make sure to update both when adding a color
    public static readonly Dictionary<uint, SKColor> PixelDict = new Dictionary<uint, SKColor>()
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

    public PixelModule() {
        PixelMaps = BotUtils.LoadJson<Dictionary<ulong, PixelMap>>(PixelMap.JsonLocation);
    }

    public PixelMap GetPixelMap(ulong id) {
        if(!PixelMaps.TryGetValue(id, out PixelMap pixelMap)) {
            pixelMap = new PixelMap(id);
            PixelMaps.Add(id, pixelMap);
        }
        return pixelMap;
    }
    public void SetPixelMap(PixelMap data) {
        PixelMaps.AddOrUpdate(data.Id, data);
        BotUtils.SaveJson(PixelMaps, PixelMap.JsonLocation);
    }

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

        public void WritePixel(int x, int y, PixelEnum color) {
            PixelState[x, y] = color;
        }
    }
}