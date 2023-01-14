using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using SkiaSharp;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Economy.Stocks
{

    public static class StockMarket
    {


        public static TimeBasedEvent UpdateEvent;

        public static readonly string[] StockNames = {
            "SPER",
            "SMTH",
            "WOLF",
            "PWR",
            "MMC",
            "WD",
            "SHWG",
            "LORD",
            "KMN",
            "CRSN",
            "HAY",
            "GHAL"
        };


        public static void Init()
        {
            Bot.Database.CreateTable<UserStockInfo>();
            Bot.Database.CreateTable<Stock>();
            foreach (string s in StockNames)
            {
                Stock stock = Stock.GetStock(s);

                if (stock != null)
                {
                    ValidateStock(stock);
                    continue;
                };

                stock = new Stock(
                    s,
                    GenerateRandomNumber(500, 10000),
                    (float)GenerateRandomNumber(10, 100) / 10,
                    (float)GenerateRandomNumber(10, 30) / 10,
                    (float)GenerateRandomNumber(10, 50) / 10
                );
                Bot.Database.Insert(stock);
            }
            UpdateMarket();

            UpdateEvent = new TimeBasedEvent(TimeSpan.FromMinutes(1), () => { UpdateMarket(); UpdateEvent.Start(); });
            UpdateEvent.Start();
        }

        private static void ValidateStock(Stock stock)
        {
            if (stock.PriceHistory == null)
            {
                stock.PriceHistory = new() { stock.ShareCost };
            }
            if (stock.TargetPrice < 100)
            {
                stock.TargetPrice = GenerateRandomNumber(500, 10000);
            }
            Bot.Database.UpdateWithChildren(stock);
        }

        public static void CreateDetailedGraph(Stock stock, ulong userId)
        {
            if (stock == null)
                return;

            int imgHeight = 500;
            int imgWidth = 1200;

            long maxPrice = long.MinValue;
            long minPrice = long.MaxValue;

            for (int i = 0; i < stock.PriceHistory.Count; i++)
            {
                maxPrice = long.Max(stock.PriceHistory[i], maxPrice);
                minPrice = long.Min(stock.PriceHistory[i], minPrice);
            }

            float valuePerPixel = imgHeight / (float)(maxPrice - minPrice);
            float xBetweenPoints = imgWidth / (float)(float)(stock.PriceHistory.Count - 1);

            SKImageInfo imageInfo = new SKImageInfo(imgWidth, imgHeight);
            SKSurface surface = SKSurface.Create(imageInfo);
            SKCanvas canvas = surface.Canvas;
            SKPaint paint = new SKPaint { IsAntialias = true };

            paint.Color = SKColor.FromHsv(0, 0, 0);
            paint.StrokeWidth = 1;

            int separation = imgHeight / 5;
            for (int i = 0; i < 6; i++)
            {
                SKPoint p1 = new(0, separation * i);
                SKPoint p2 = new(imgWidth, separation * i);
                canvas.DrawLine(p1, p2, paint);
            }

            paint.Color = GetGraphPaintColor(stock);
            paint.StrokeWidth = 4;

            for (int i = 0; i < stock.PriceHistory.Count - 1; i++)
            {
                SKPoint p1 = new(i * xBetweenPoints, imgHeight - (stock.PriceHistory[i] - minPrice) * valuePerPixel);
                SKPoint p2 = new((i + 1) * xBetweenPoints, imgHeight - (stock.PriceHistory[i + 1] - minPrice) * valuePerPixel);
                canvas.DrawLine(p1, p2, paint);
            }

            surface.Snapshot().SaveToPng($"StockImages/img{userId}.png");
        }

        public static void CreateOverviewGraph(ulong userId)
        {
            List<Stock> stocks = Bot.Database.GetAllWithChildren<Stock>();

            int graphsPerRow = 4;
            int rows = stocks.Count / graphsPerRow;

            int graphHeight = 100;
            int graphWidth = 180;
            int spacingHeight = 50;

            int imgHeight = (graphHeight + spacingHeight) * rows;
            int imgWidth = graphWidth * graphsPerRow;

            SKImageInfo imageInfo = new SKImageInfo(imgWidth, imgHeight);
            SKSurface surface = SKSurface.Create(imageInfo);
            SKCanvas canvas = surface.Canvas;
            SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                TextSize = 25,
                Typeface = SKTypeface.Default
            };
            paint.StrokeWidth = 3;

            for (int i = 0; i < stocks.Count; i++)
            {
                Stock stock = stocks[i];

                int anchorX = i % graphsPerRow * graphWidth;
                int anchorY = i / graphsPerRow * (graphHeight + spacingHeight);

                paint.StrokeWidth = 3;

                paint.Color = GetGraphPaintColor(stock);
                long maxPrice = long.MinValue;
                long minPrice = long.MaxValue;

                for (int j = 0; j < stock.PriceHistory.Count; j++)
                {
                    maxPrice = long.Max(stock.PriceHistory[j], maxPrice);
                    minPrice = long.Min(stock.PriceHistory[j], minPrice);
                }

                float valuePerPixel = (graphHeight - 10) / (float)(maxPrice - minPrice);
                float xBetweenPoints = (graphWidth - 10) / (float)(stock.PriceHistory.Count - 1);

                for (int j = 0; j < stock.PriceHistory.Count - 1; j++)
                {
                    SKPoint p1 = new(anchorX + 5 + j * xBetweenPoints, anchorY - 5 + spacingHeight + (graphHeight - (stock.PriceHistory[j] - minPrice) * valuePerPixel));
                    SKPoint p2 = new(anchorX + 5 + (j + 1) * xBetweenPoints, anchorY - 5 + spacingHeight + (graphHeight - (stock.PriceHistory[j + 1] - minPrice) * valuePerPixel));
                    canvas.DrawLine(p1, p2, paint);
                }

                paint.Color = SKColor.FromHsv(0, 0, 0);
                SKPoint point1 = new SKPoint(0, anchorY);
                SKPoint point2 = new SKPoint(imgWidth, anchorY);
                canvas.DrawLine(point1, point2, paint);

                point1 = new SKPoint(0, anchorY + spacingHeight + graphHeight);
                point2 = new SKPoint(imgWidth, anchorY + spacingHeight + graphHeight);
                canvas.DrawLine(point1, point2, paint);

                point1 = new SKPoint(anchorX, 0);
                point2 = new SKPoint(anchorX, imgHeight);
                canvas.DrawLine(point1, point2, paint);

                point1 = new SKPoint(anchorX + graphWidth, 0);
                point2 = new SKPoint(anchorX + graphWidth, imgHeight);
                canvas.DrawLine(point1, point2, paint);

                paint.StrokeWidth = 2;
                point1 = new SKPoint(0, anchorY + spacingHeight);
                point2 = new SKPoint(imgWidth, anchorY + spacingHeight);
                canvas.DrawLine(point1, point2, paint);

                paint.Color = SKColor.FromHsv(0, 0, 100);
                paint.TextAlign = SKTextAlign.Left;
                canvas.DrawText($"{stock.Name}", anchorX + 5, anchorY + spacingHeight / 2 - 3, paint);
                canvas.DrawText($"${stock.ShareCost} ({stock.GetEarningsPercentString(2)})", anchorX + 5, anchorY + spacingHeight - 3, paint);
            }

            surface.Snapshot().SaveToPng($"StockImages/img{userId}.png");
        }

        static SKColor GetGraphPaintColor(Stock stock)
        {
            if (stock.PriceHistory[0] < stock.ShareCost)
                return SKColor.FromHsv(80, 100, 100);
            else
                return SKColor.FromHsv(0, 100, 100);
        }

        static void UpdateMarket()
        {
            List<Stock> stocks = Bot.Database.GetAllWithChildren<Stock>();

            foreach (Stock stock in stocks)
            {
                stock.SimulateStep();
                Bot.Database.UpdateWithChildren(stock);
            }
        }
    }
}
