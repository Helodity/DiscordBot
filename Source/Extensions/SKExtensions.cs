using System;
using System.IO;
using SkiaSharp;

namespace DiscordBotRewrite.Extensions {
    public static class SKExtensions {
        public static void SaveToPng(this SKImage image, string path) {
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            if(!File.Exists(path))
                FileExtension.CreateFileWithPath(path);

            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);
        }
        public static void DrawHatchedRect(this SKCanvas canvas, float x, float y, float w, float h, SKPaint paint, int hatchDistance) {
            SKPath path = new SKPath();
            for(int i = 0; i < w; i += hatchDistance) {
                path.MoveTo(x + i, y);
                float size = Math.Min(h, Math.Min(w, w - i));
                path.LineTo(x + i + size, y + size);
            }
            for(int i = 0; i < h; i += hatchDistance) {
                path.MoveTo(x, y + i);
                float size = Math.Min(w, Math.Min(h, h - i));
                path.LineTo(x + size, y + i + size);
            }
            //Hatching only works with stroke paint, temporarily set it to that to ensure proper drawing
            SKPaintStyle oldStyle = paint.Style;
            paint.Style = SKPaintStyle.Stroke;
            canvas.DrawPath(path, paint);
            paint.Style = oldStyle;
        }
    }
}