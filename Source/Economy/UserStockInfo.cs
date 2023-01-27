using SQLite;

namespace DiscordBotRewrite.Economy
{

    [Table("user_stocks")]
    public class UserStockInfo
    {

        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("stock_name")]
        public string StockName { get; set; }

        [Column("amount")]
        public long Amount { get; set; }

        public UserStockInfo() { }

        public UserStockInfo(string stockName, long userID)
        {
            UserId = userID;
            StockName = stockName;
        }

        public void ModifyAmount(long amount, bool update = true)
        {
            Amount += amount;
            if (update)
                Bot.Database.Update(this);
        }


        public static UserStockInfo GetStockInfo(long userId, string stockName)
        {
            UserStockInfo account = Bot.Database.Table<UserStockInfo>().FirstOrDefault(x => x.UserId == userId && x.StockName == stockName);
            if (account == null)
            {
                account = new UserStockInfo(stockName, userId);
                Bot.Database.Insert(account);
            }
            return account;
        }

    }
}
