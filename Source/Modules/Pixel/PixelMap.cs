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
        public byte[] PixelState;

        public Dictionary<ulong, Cooldown> PlaceCooldowns = new Dictionary<ulong, Cooldown>();
        #endregion

        #region Constructor
        public PixelMap(ulong id, int width = 100, int height = 100) : base(id) {
            Width = width;
            Height = height;
            PixelState = new byte[Width * Height];
            PlaceCooldown = 0;
        }
        #endregion

        #region Public
        public void Resize(int width, int height) {
            byte[] old = PixelState;
            int maxY = Math.Min(Height, height);
            int maxX = Math.Min(Width, width);

            Width = width;
            Height = height;
            PixelState = new byte[Width * Height];
            for(int y = 0; y < maxY; y++) {
                for(int x = 0; x < maxX; x++) {
                    PixelState[x + y * Width] = old[x + y * Height];
                }
            }
        }

        public void SetPixel(int x, int y, PixelEnum color) {
            PixelState[x + y * Width] = (byte)color;
        }

        public PixelEnum GetPixel(int x, int y) {
            return (PixelEnum)PixelState[x + y * Width];
        }

        public int TimeUntilNextPlace(ulong userId) {
            return (int)Cooldown.TimeUntilExpiration(userId, ref PlaceCooldowns).TotalSeconds;
        }
        #endregion
    }
}