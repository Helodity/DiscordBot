using SQLite;

namespace DiscordBotRewrite.Modules.Economy {

    [Table("user_stocks")]
    public class UserStockInfo {

        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("stock_name")]
        public string StockName { get; set; }

        [Column("amount")]
        public long Amount { get; set; }

        public UserStockInfo() { }

        public UserStockInfo(string stockName, long userID) {
            UserId = userID;
            StockName = stockName;
        }

        public void ModifyAmount(long amount, bool update = true) {
            amount = Math.Max(-Amount, amount);
            Amount += amount;
            if(update)
                Bot.Database.Update(this);
        }


    }
}
