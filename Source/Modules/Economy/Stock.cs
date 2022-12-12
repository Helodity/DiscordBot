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
        public float PriceVolatility { get; set; }

        [Column("momentum_volatility")]
        public float MomentumVolatility { get; set; }

        [Column("prev_earnings")]
        public float LastEarnings { get; set; }

        [TextBlob("PriceHistoryBlobbed")]
        public List<long> PriceHistory { get; set; }
        public string PriceHistoryBlobbed { get; set; }

        public Stock() { }

        public Stock(string name, int initialCost, float priceVolality, float momentumVolatility) {
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
            if(Momentum > MomentumVolatility)
                Momentum = MomentumVolatility;
            if(Momentum < -MomentumVolatility)
                Momentum = -MomentumVolatility;

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

            //Track the last 2 hours
            if(PriceHistory.Count > 120)
                PriceHistory.RemoveAt(0);

        }
        public void ModifySales(long amount, bool update = true) {
            Momentum += amount / 100f;
            if(update)
                Bot.Database.UpdateWithChildren(this);
        }

        public string GetEarningsPercentString(int digits = 2) {
            float percent = ((float)(ShareCost - PriceHistory[0]) / PriceHistory[0]) * 100;
            if(percent == 0)
                return "0%";

            decimal scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(percent))) - digits + 1);

            return $"{scale * Math.Round((decimal)percent / scale)}%";
        }


    }
}

