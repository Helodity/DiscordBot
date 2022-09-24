using System;
using System.Collections.Generic;
using DiscordBotRewrite.Global;
using Newtonsoft.Json;
using SQLite;


namespace DiscordBotRewrite.Modules {
    [Table("economy_accounts")]
    public class UserAccount {
        [PrimaryKey]
        [Unique]
        [Column("id")]
        public long Id { get; set; }
        [Column("balance")]
        public long Balance { get; set; }
        [Column("bank")]
        public long Bank { get; set; }
        [Column("bank_max")]
        public long BankMax { get; set; }
        [Column("daily_cooldown")]
        public DateTime DailyCooldown { get; set; }
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
            Id = id;
            Balance = 0;
            Bank = 0;
            BankMax = 1000;
            DailyCooldown = DateTime.Now;
            Streak = 0;
        }
        #endregion
    }
}