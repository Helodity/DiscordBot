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
        readonly Dictionary<ulong, UserAccount> UserAccounts;
        readonly Dictionary<ulong, Cooldown> RobCooldowns;
        #region Constructors
        public EconomyModule() {
            UserAccounts = LoadJson<Dictionary<ulong, UserAccount>>(UserAccount.JsonLocation);
            RobCooldowns = new();
        }
        #endregion

        public List<UserAccount> GetUserAccounts() {
            return UserAccounts.Values.ToList();
        }

        public UserAccount GetAccount(ulong id) {
            if(!UserAccounts.TryGetValue(id, out UserAccount account)) {
                account = new UserAccount(id);
                UserAccounts.Add(id, account);
            }
            return account;
        }

        public long Transfer(ulong id1, ulong id2, long value) {
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
                SaveJson(UserAccounts, UserAccount.JsonLocation);
            }
            return value;
        }

        public long GetTotalBalance() {
            long total = 0;
            foreach(KeyValuePair<ulong, UserAccount> kvp in UserAccounts) {
                total += kvp.Value.Balance;
                total += kvp.Value.Bank;
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

        public void SaveAccounts() {
            SaveJson(UserAccounts, UserAccount.JsonLocation);
        }
    }
}