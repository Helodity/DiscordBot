using System;
using System.Collections.Generic;

namespace DiscordBotRewrite.Modules {
    public class EconomyModule {
        #region Constructors
        public EconomyModule() {
            Bot.Database.CreateTable<UserAccount>();
        }
        #endregion

        public List<UserAccount> GetUserAccounts() {
            return Bot.Database.Table<UserAccount>().ToList();
        }

        public UserAccount GetAccount(long userId) {
            UserAccount account = Bot.Database.Table<UserAccount>().FirstOrDefault(x => x.UserId == userId);
            if(account == null) {
                account = new UserAccount(userId);
                Bot.Database.Insert(account);
            }
            return account;
        }

        public long Transfer(long id1, long id2, long value) {
            if(value <= 0)
                return 0;
            UserAccount account1 = GetAccount(id1);
            UserAccount account2 = GetAccount(id2);
            if(value <= 0)
                return 0;
            if(account1.Balance < value)
                value = account1.Balance;

            account1.Balance -= value;
            account2.Balance += value;
            if(value != 0) {
                Bot.Database.Update(account1);
                Bot.Database.Update(account2);
            }
            return value;
        }

        public long GetTotalBalance() {
            long total = 0;
            List<UserAccount> accounts = GetUserAccounts();
            foreach(UserAccount account in accounts) {
                total += account.Balance;
                total += account.Bank;
            }
            return total;
        }

        public double GetWinningsMultiplier(int gamesWon, double scale = 1.0) {
            return Math.Pow(2, gamesWon * scale);
        }

        public int CalculateBlackJackHandValue(List<Card> hand) {
            int value = 0;
            int aces = 0;
            foreach(Card c in hand) {
                int thisValue = (int)c.value + 2;
                if(thisValue > 10)
                    thisValue = 10;
                if(c.value == Card.Value.Ace) {
                    aces++;
                    thisValue = 11;
                }
                value += thisValue;
            }
            while(value > 21 && aces > 0) {
                value -= 10;
                aces -= 1;
            }
            return value;
        }
    }
}