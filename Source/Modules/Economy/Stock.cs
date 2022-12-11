using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Modules.Economy {

    [Table("stock_market")]
    public class Stock {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
        [Column("cost")]
        public long ShareCost { get; set; }

        [Column("momentum")]
        public float Momentum { get; set; }

        [Column("price_volatility")]
        public int PriceVolatility { get; set; }

        [Column("momentum_volatility")]
        public int MomentumVolatility { get; set; }

        [Column("prev_earnings")]
        public float LastEarnings { get; set; }

        [TextBlob("PriceHistoryBlobbed")]
        public List<long> PriceHistory { get; set; }
        public string PriceHistoryBlobbed { get; set; }

        public Stock() { }

        public Stock(string name, int initialCost, int priceVolality, int momentumVolatility) {
            Name = name;
            PriceVolatility = priceVolality;
            MomentumVolatility = momentumVolatility;
            ShareCost = initialCost;
            LastEarnings = 0;
            Momentum = 0;
            PriceHistory = new() {initialCost};
        }

        public void SimulateStep() {
            float momentumShift = ((float)GenerateRandomNumber(-100, 100) / 100 + LastEarnings + 0.01f) * MomentumVolatility;
            Momentum += momentumShift;
            if(Momentum > 2)
                Momentum = 2;
            if(Momentum < -2)
                Momentum = -2;

            LastEarnings = (float)GenerateRandomNumber(-40, 40) * PriceVolatility / 10000 + (float)Momentum / 100;

            float toChange = ShareCost * LastEarnings;
            if(toChange > -1 && toChange < 1) {
                toChange /= Math.Abs(toChange);
            }

            if(PriceHistory == null)
                PriceHistory = new();

            ShareCost += (long)Math.Floor(toChange);
            if(ShareCost < 10)
                ShareCost = 10;

            PriceHistory.Add(ShareCost);
            if(PriceHistory.Count > 100)
                PriceHistory.RemoveAt(0);

        }
        public void ModifySales(long amount, bool update = true) {
            Momentum += amount / 100f;
            if(update)
                Bot.Database.UpdateWithChildren(this);
        }

        public float GetOverallEarningPercentage(int decimals = 2) {
            return (float)Math.Round((float)(ShareCost - PriceHistory[0]) * Math.Pow(10, 2 + decimals) / PriceHistory[0]) / (float)Math.Pow(10, decimals);
        }


    }
}
