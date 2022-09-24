using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus.SlashCommands;
using SkiaSharp;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Modules {
    public class EconomyModule {
        readonly Dictionary<ulong, Cooldown> RobCooldowns;
        #region Constructors
        public EconomyModule() {
            Bot.Database.CreateTable<UserAccount>();
            RobCooldowns = new();
        }
        #endregion

        public List<UserAccount> GetUserAccounts() {
            return Bot.Database.Table<UserAccount>().ToList();
        }

        public UserAccount GetAccount(long id) {
            UserAccount account = Bot.Database.Find<UserAccount>(id);
            if(account == null) {
                account = new UserAccount(id);
                Bot.Database.InsertOrReplace(account);
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
        public void AddRobCooldown(ulong id) {
            RobCooldowns.AddOrUpdate(id, new Cooldown(DateTime.Now.AddSeconds(30)));
        }

        public bool HasRobCooldown(ulong id, out Cooldown robCooldown) {
            if(!RobCooldowns.TryGetValue(id, out robCooldown)) { 
                return false;
            }
            if(robCooldown.IsOver) {
                RobCooldowns.Remove(id);
                return false;
            }
            return true;
        }

        public double GetWinningsMultiplier(int gamesWon, double scale = 1.0) {
            return Math.Pow(2, gamesWon * scale);
        }
    }
}