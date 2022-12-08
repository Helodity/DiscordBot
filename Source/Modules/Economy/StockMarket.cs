using DiscordBotRewrite.Global;

namespace DiscordBotRewrite.Modules.Economy {

    public static class StockMarket {


        public static TimeBasedEvent UpdateEvent;

        public static readonly string[] StockNames = {
            "SPER",
            "SMTH",
            "WOLF",
            "PWR",
            "MMC",
            "WD"
        };


        public static void Init() {
            Bot.Database.CreateTable<UserStockInfo>();
            Bot.Database.CreateTable<Stock>();
            foreach(string s in StockNames) {
                Stock stock = Bot.Database.Table<Stock>().FirstOrDefault(x => x.Name == s);
                if(stock != null) continue;

                stock = new Stock(s, 250, GenerateRandomNumber(1, 10), GenerateRandomNumber(1, 2));
                Bot.Database.Insert(stock);
            }

            UpdateEvent = new TimeBasedEvent(TimeSpan.FromSeconds(1), UpdateMarket);
            UpdateEvent.Start();
        }


        public static void UpdateMarket() {
            List<Stock> stocks = Bot.Database.Table<Stock>().ToList();

            foreach(Stock stock in stocks) {
                stock.SimulateStep();
                Bot.Database.Update(stock);    
            }

            UpdateEvent.Start();
        }

    }
}
