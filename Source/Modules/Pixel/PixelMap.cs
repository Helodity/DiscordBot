using System;
using SQLite;
using static DiscordBotRewrite.Modules.PixelModule;

namespace DiscordBotRewrite.Modules {
    [Table("pixel_maps")]
    public class PixelMap {
        #region Properties
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Unique, Column("guild_id")]
        public long GuildId { get; set; }
        [Column("width")]
        public int Width { get; set; }

        [Column("height")]
        public int Height { get; set; }

        [Column("cooldown_time")]
        public uint PlaceCooldown { get; set; }

        [Column("pixel_data")]
        public byte[] PixelState { get; set; }
        #endregion

        #region Constructor
        public PixelMap() {}
        public PixelMap(long id, int width = 100, int height = 100) {
            GuildId = id;
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

        public DateTime NextPlaceTime(long userId) {
            PlaceCooldown c = Bot.Modules.Pixel.GetPlaceCooldown(GuildId, userId);
            return c.EndTime;
        }
        #endregion
    }
}