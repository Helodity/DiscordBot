using System;
using SQLite;
using static DiscordBotRewrite.Modules.PixelModule;

namespace DiscordBotRewrite.Modules {
    [Table("pixel_maps")]
    public class PixelMap {
        #region Properties
        [PrimaryKey, Unique, Column("id")]
        public long Id { get; set; }
        [Column("width")]
        public int Width { get; set; }

        [Column("height")]
        public int Height { get; set; }

        [Column("cooldown_time")]
        public uint PlaceCooldown { get; set; }

        [Column("pixel_data")]
        public byte[] PixelState { get; set; }

        //public Dictionary<ulong, Cooldown> PlaceCooldowns = new Dictionary<ulong, Cooldown>();
        #endregion

        #region Constructor
        public PixelMap() {}
        public PixelMap(long id, int width = 100, int height = 100) {
            Id = id;
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
            return 0; // (int)Cooldown.TimeUntilExpiration(userId, ref PlaceCooldowns).TotalSeconds;
        }
        #endregion
    }
}