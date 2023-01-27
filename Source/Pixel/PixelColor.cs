using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SkiaSharp;

namespace DiscordBotRewrite.Pixel {
    public class PixelColor {

        public readonly SKColor Color;
        public readonly string Name;
        public readonly uint ID;

        public PixelColor(uint id, SKColor color, string name) {
            ID = id;
            Color = color;
            Name = name;
        }

        public static readonly List<PixelColor> ColorDict = new()
        {
            {new(0, SKColors.White, "White")},
            {new(1, SKColors.Red, "Red")},
            {new(2, SKColors.Orange, "Orange")},
            {new(3, SKColors.Yellow, "Yellow")},
            {new(4, SKColors.Green, "Green")},
            {new(5, SKColors.Blue, "Blue")},
            {new(6, SKColors.Purple, "Purple")},
            {new(7, SKColors.Brown, "Brown")},
            {new(8, SKColors.Pink, "Pink")},
            {new(9, SKColors.Gray, "Gray")},
            {new(10, SKColors.Black, "Black")},
            {new(11, SKColors.Cyan, "Cyan")}
        };

        public static PixelColor GetFromID(uint id) {
            return ColorDict.FirstOrDefault(x => x.ID == id);
        }

    }
}
