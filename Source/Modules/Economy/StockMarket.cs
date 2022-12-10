using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using SkiaSharp;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Modules.Economy {

    public static class StockMarket {


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
            "HAY"
        };


        public static void Init() {
            Bot.Database.CreateTable<UserStockInfo>();
            Bot.Database.CreateTable<Stock>();
            foreach(string s in StockNames) {
                Stock stock = Bot.Database.GetAllWithChildren<Stock>().FirstOrDefault(x => x.Name == s);
                if(stock != null) {
                    if(stock.PriceHistory == null) {
                        stock.PriceHistory = new() {stock.ShareCost}; //Put a placeholder value to prevent an error
                        Bot.Database.UpdateWithChildren(stock);
                    }
                    continue;
                };

                stock = new Stock(s, 250, GenerateRandomNumber(1, 10), GenerateRandomNumber(1, 2));
                Bot.Database.Insert(stock);
            }
            UpdateMarket();

            UpdateEvent = new TimeBasedEvent(TimeSpan.FromMinutes(1), TimedMarketUpdate);
            UpdateEvent.Start();
        }

        public static void CreateStockGraph(string stockName, ulong userId) {
            Stock stock = Bot.Modules.Economy.GetStock(stockName);
            if(stock == null)
                return;

            long maxPrice = long.MinValue;
            long minPrice = long.MaxValue;

            for(int i = 0; i < stock.PriceHistory.Count; i++) { 
                maxPrice = long.Max(stock.PriceHistory[i], maxPrice);
                minPrice = long.Min(stock.PriceHistory[i], minPrice);
            }

            float valuePerPixel = 500 / (float)(maxPrice - minPrice);
            int xBetweenPoints = 1000 / stock.PriceHistory.Count;

            SKImageInfo imageInfo = new SKImageInfo(1000, 500);
            SKSurface surface = SKSurface.Create(imageInfo);
            SKCanvas canvas = surface.Canvas;
            SKPaint paint = new SKPaint {IsAntialias = true};

            paint.Color = SKColor.FromHsv(0, 0, 0);
            paint.StrokeWidth = 1;
            for(int i = 0; i < 6; i++) {
                SKPoint p1 = new(0, 100 * i);
                SKPoint p2 = new(1000, 100 * i);
                canvas.DrawLine(p1, p2, paint);
            }

            if(stock.PriceHistory[0] < stock.ShareCost) {
                paint.Color = SKColor.FromHsv(80, 100, 100);
            } else {
                paint.Color = SKColor.FromHsv(0, 100, 100);
            }
            paint.StrokeWidth = 4;

            for(int i = 0; i < stock.PriceHistory.Count - 1; i++) {
                SKPoint p1 = new(i * xBetweenPoints, 500 - (stock.PriceHistory[i] - minPrice) * valuePerPixel);
                SKPoint p2 = new((i + 1) * xBetweenPoints, 500 - (stock.PriceHistory[i + 1] - minPrice) * valuePerPixel);
                canvas.DrawLine(p1, p2, paint);
            }

            surface.Snapshot().SaveToPng($"StockImages/img{userId}.png");
        }
        static void UpdateMarket() {
            List<Stock> stocks = Bot.Database.GetAllWithChildren<Stock>();

            foreach(Stock stock in stocks) {
                stock.SimulateStep();
                Bot.Database.UpdateWithChildren(stock);
            }
        }

        public static void TimedMarketUpdate() {
            UpdateMarket();
            UpdateEvent.Start();
        }

    }
}
