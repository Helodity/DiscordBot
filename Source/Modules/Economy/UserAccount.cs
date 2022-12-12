using System;
using DiscordBotRewrite.Global;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("economy_accounts")]
    public class UserAccount {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Unique, Column("user_id")]
        public long UserID { get; set; }
        [Column("balance")]
        public long Balance { get; set; }
        [Column("bank")]
        public long Bank { get; set; }
        [Column("Debt")]
        public long Debt { get; set; }
        public long NetWorth => Balance + Bank - Debt;
        [Column("bank_max")]
        public long BankMax { get; set; }
        [Column("daily_streak")]
        public int Streak { get; set; }

        #region Constructor
        public UserAccount() {
            Balance = 0;
            Bank = 0;
            Debt = 0;
            BankMax = 1000;
            Streak = 0;
        }
        public UserAccount(long id) {
            UserID = id;
            Balance = 0;
            Bank = 0;
            BankMax = 1000;
            Streak = 0;
        }
        #endregion

        public List<UserAccount> GetAllAccounts() {
            return Bot.Database.Table<UserAccount>().ToList();
        }

        public static UserAccount GetAccount(long userID) {
            UserAccount account = Bot.Database.Table<UserAccount>().FirstOrDefault(x => x.UserID == userID);
            if(account == null) {
                account = new UserAccount(userID);
                Bot.Database.Insert(account);
            }
            return account;
        }

        public void Pay(long amount) {
            if(amount < 0) {
                Receive(-amount);
                return;
            }

            long amtFromBank = Math.Max(0, amount - Balance);
            long amtToDebt = Math.Max(0, amtFromBank - Bank);
            Debt += amtToDebt;
            ModifyBank(-amtFromBank);
            ModifyBalance(-amount);
        }
        public void Receive(long amount) {
            if(amount < 0) {
                Pay(-amount);
                return;
            }
            if(Debt > 0) {
                long toDebt = Math.Min(amount / 2, Debt);
                Debt -= toDebt;
                amount -= toDebt;
            }
            ModifyBalance(amount);
        }
        public void ModifyBankMax(long amount, bool update = true) {
            amount = Math.Max(-BankMax, amount);
            BankMax += amount;
            if(update)
                Bot.Database.Update(this);
        }
        public void IncrementStreak(bool update = true) {
            Streak += 1;
            if(update)
                Bot.Database.Update(this);
        }
        public void ResetStreak(bool update = true) {
            Streak = 1;
            if(update)
                Bot.Database.Update(this);
        }

        //Returns amount put in bank
        public long TransferToBank(long amount, bool update = true) {
            if(Bank + amount > BankMax)
                amount = BankMax - Bank;
            if(amount > Balance)
                amount = Balance;
            Balance -= amount;
            Bank += amount;

            if(update)
                Bot.Database.Update(this);

            return amount;
        }
        //Returns amount put in bank
        public long TransferToBalance(long amount, bool update = true) {
            if(amount > Bank)
                amount = Bank;
            Balance += amount;
            Bank -= amount;

            if(update)
                Bot.Database.Update(this);

            return amount;
        }

        public void ModifyDebt(long amount, bool update = true) {
            Debt += amount;
            if(update)
                Bot.Database.Update(this);
        }
        void ModifyBalance(long amount, bool update = true) {
            amount = Math.Max(-Balance, amount);
            Balance += amount;
            if(update)
                Bot.Database.Update(this);
        }
        void ModifyBank(long amount, bool update = true) {
            amount = Math.Max(-Bank, amount);
            Bank = Math.Min(Bank + amount, BankMax);
            if(update)
                Bot.Database.Update(this);
        }
    }
}