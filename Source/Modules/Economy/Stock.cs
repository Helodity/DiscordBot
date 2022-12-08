using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Modules.Economy {

    [Table("stock_market")]
    public class Stock {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
        [Column("cost")]
        public int ShareCost { get; set; }

        [Column("momentum")]
        public float Momentum { get; set; }

        [Column("price_volatility")]
        public int PriceVolatility { get; set; }

        [Column("momentum_volatility")]
        public int MomentumVolatility { get; set; }

        [Column("prev_earnings")]
        public float LastEarnings { get; set; }

        [TextBlob("PriceHistoryBlobbed")]
        public List<int> PriceHistory { get; set; }
        public string PriceHistoryBlobbed { get; set; }

        public Stock() {}

        public Stock(string name, int initialCost, int priceVolality, int momentumVolatility) {
            Name = name;
            PriceVolatility = priceVolality;
            MomentumVolatility = momentumVolatility;
            ShareCost = initialCost;
            LastEarnings = 0;
            Momentum = 0;
            PriceHistory= new();
        }


        public void SimulateStep() {
            LastEarnings = ((float)GenerateRandomNumber(-10, 10) * PriceVolatility / 1000) + (float)Momentum / 100;
            float momentumShift = ((float)GenerateRandomNumber(-100, 100) / 100 + LastEarnings) * MomentumVolatility;

            float toChange = ShareCost * LastEarnings;
            if(toChange > -1 && toChange < 1) {
                toChange /= Math.Abs(toChange);
            }

            if(PriceHistory == null)
                PriceHistory = new();

            PriceHistory.Add(ShareCost);
            if(PriceHistory.Count > 10)
                PriceHistory.RemoveAt(0);

            ShareCost += (int)Math.Floor(toChange);

            if(ShareCost < 100)
                ShareCost = 100;

            Momentum += momentumShift;
            if(Momentum > 2)
                Momentum = 2;
            if(Momentum < -2)
                Momentum = -2;
        }
    }
}
