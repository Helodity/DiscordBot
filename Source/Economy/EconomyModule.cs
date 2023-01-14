using DiscordBotRewrite.Economy.Gambling;
using DiscordBotRewrite.Economy.Stocks;

namespace DiscordBotRewrite.Economy
{
    public class EconomyModule
    {
        #region Constructors
        public EconomyModule()
        {
            Bot.Database.CreateTable<UserAccount>();
            StockMarket.Init();
        }
        #endregion

        public long Transfer(long id1, long id2, long value, bool allowNegative = false)
        {
            if (!allowNegative && value <= 0)
                return 0;
            UserAccount account1 = UserAccount.GetAccount(id1);
            UserAccount account2 = UserAccount.GetAccount(id2);

            if (account1.Balance < value)
                value = account1.Balance;

            account1.Pay(value);
            account2.Receive(value);
            return value;
        }
        public double GetMultiplier(int streak, double scale = 1, double exponent = 1)
        {
            return scale * Math.Pow(streak, exponent) + 1;
        }

        #region Gambling
        public int CalculateBlackJackHandValue(List<Card> hand)
        {
            int value = 0;
            int aces = 0;
            foreach (Card c in hand)
            {
                int thisValue = (int)c.value + 2;
                if (thisValue > 10)
                    thisValue = 10;
                if (c.value == Card.Value.Ace)
                {
                    aces++;
                    thisValue = 11;
                }
                value += thisValue;
            }
            while (value > 21 && aces > 0)
            {
                value -= 10;
                aces -= 1;
            }
            return value;
        }
        #endregion
    }
}