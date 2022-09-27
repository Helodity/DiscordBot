using System;
using SQLite;

namespace DiscordBotRewrite.Modules {
    [Table("economy_accounts")]
    public class UserAccount {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; private set; }
        [Unique, Column("user_id")]
        public long UserId { get; private set; }
        [Column("balance")]
        public long Balance { get; private set; }
        [Column("bank")]
        public long Bank { get; private set; }
        [Column("bank_max")]
        public long BankMax { get; private set; }
        [Column("daily_cooldown")]
        public DateTime DailyCooldown { get; private set; }
        [Column("rob_cooldown")]
        public DateTime RobCooldown { get; private set; }
        [Column("daily_streak")]
        public int Streak { get; set; }

        #region Constructor
        public UserAccount() {
            Balance = 0;
            Bank = 0;
            BankMax = 1000;
            DailyCooldown = DateTime.Now;
            Streak = 0;
        }
        public UserAccount(long id) {
            UserId = id;
            Balance = 0;
            Bank = 0;
            BankMax = 1000;
            DailyCooldown = DateTime.Now;
            Streak = 0;
        }
        #endregion

        public void ModifyBalance(long amount, bool update = true) {
            amount = Math.Max(-Balance, amount);
            Balance += amount;
            if(update)
                Bot.Database.Update(this);
        }
        public void ModifyBank(long amount, bool update = true) {
            amount = Math.Max(-Bank, amount);
            Bank = Math.Min(Bank + amount, BankMax);
            if(update)
                Bot.Database.Update(this);
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
            Streak = 0;
            if(update)
                Bot.Database.Update(this);
        }
        public void SetDailyCooldown(DateTime time, bool update = true) {
            DailyCooldown = time;
            if(update)
                Bot.Database.Update(this);
        }
        public void SetRobCooldown(DateTime time, bool update = true) {
            RobCooldown = time;
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
    }
}