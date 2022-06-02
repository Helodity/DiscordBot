using System;
using System.Collections.Generic;
using DiscordBotRewrite.Global;
using Newtonsoft.Json;
using static DiscordBotRewrite.Modules.PixelModule;
namespace DiscordBotRewrite.Modules {
    public class PixelMap : ModuleData {
        #region Properties
        public const string JsonLocation = "Json/PixelMaps.json";

        [JsonProperty("width")]
        public int Width;
        [JsonProperty("height")]
        public int Height;
        [JsonProperty("cooldown_sec")]
        public uint PlaceCooldown;
        [JsonProperty("pixel_data")]
        public PixelEnum[,] PixelState;

        public Dictionary<ulong, Cooldown> PlaceCooldowns = new Dictionary<ulong, Cooldown>();
        #endregion

        #region Constructor
        public PixelMap(ulong id, int width = 100, int height = 100) : base(id) {
            Width = width;
            Height = height;
            PixelState = new PixelEnum[Width, Height];
            PlaceCooldown = 0;
        }
        #endregion

        #region Public
        public void Resize(int width, int height) {
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

        public int TimeUntilNextPlace(ulong userId) {
            return (int)Cooldown.TimeUntilExpiration(userId, ref PlaceCooldowns).TotalSeconds;
        }
        #endregion
    }
}