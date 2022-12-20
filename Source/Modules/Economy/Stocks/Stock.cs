using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Modules.Economy.Stocks {
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

        [Column("max_momentum")]
        public float MaxMomentum { get; set; }

        [Column("prev_earnings")]
        public float LastEarnings { get; set; }

        [TextBlob("PriceHistoryBlobbed")]
        public List<long> PriceHistory { get; set; }
        public string PriceHistoryBlobbed { get; set; }

        public Stock() { }

        public Stock(string name, int initialCost, float priceVolality, float momentumVolatility, float maxMomentum) {
            Name = name;
            PriceVolatility = priceVolality;
            MomentumVolatility = momentumVolatility;
            MaxMomentum = maxMomentum;
            ShareCost = initialCost;
            LastEarnings = 0;
            Momentum = 0;
            PriceHistory = new() { initialCost };
        }

        public void SimulateStep() {
            float momentumShift = ((float)GenerateRandomNumber(-100, 100) / 100 + LastEarnings) * MomentumVolatility;
            Momentum += momentumShift;
            if(Momentum > MaxMomentum)
                Momentum = MaxMomentum;
            if(Momentum < -MaxMomentum)
                Momentum = -MaxMomentum;

            LastEarnings = (GenerateRandomNumber(-30, 40) * PriceVolatility / 100 + Momentum - (float)Math.Pow(ShareCost - 100, 0.6) / 100) / 1000;

            float toChange = ShareCost * LastEarnings;
            if(toChange > -1 && toChange < 1) {
                toChange /= Math.Abs(toChange);
            }

            ShareCost += (long)Math.Floor(toChange);
            if(ShareCost < 100)
                ShareCost = 100;


            if(PriceHistory == null)
                PriceHistory = new();
            PriceHistory.Add(ShareCost);
            //Track the last 2 hours
            if(PriceHistory.Count > 120)
                PriceHistory.RemoveAt(0);
        }
        public void ModifySales(long amount, bool update = true) {
            Momentum += (float)Math.Pow(amount, 0.5f) / 1000f;

            if(update)
                Bot.Database.UpdateWithChildren(this);
        }

        public string GetEarningsPercentString(int digits = 2) {
            float percent = (float)(ShareCost - PriceHistory[0]) / PriceHistory[0];
            return ToPercent((decimal)percent, digits);
        }

        //Somethin somethin fix this
        public string ToPercent(decimal toConvert, int digits = 2) {
            if(toConvert == 0)
                return "0%";

            decimal scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)Math.Abs(toConvert))) - digits + 1);

            string formatter = "";
            if(scale <= (decimal)0.0001) {
                formatter += "0.";
            }
            for(int i = 0; i < digits; i++) {

                double log = Math.Log10((double)scale);
                decimal curScale = (decimal)Math.Pow(10, log + digits - 1 - i);

                formatter += "0";
                if(curScale == (decimal)0.01) {
                    formatter += ".";
                }
            }

            return $"{(scale * Math.Round(toConvert / scale) * 100).ToString(formatter)}%";
        }


        public static Stock GetStock(string name) {
            //Don't insert if the name isn't valid here. We dont want to be inserting random stocks now.
            return Bot.Database.GetAllWithChildren<Stock>().FirstOrDefault(x => x.Name == name);
        }
    }
}

