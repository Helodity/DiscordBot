using System;
using System.Collections.Generic;
using DiscordBotRewrite.Global;
using Newtonsoft.Json;
using static DiscordBotRewrite.Modules.PixelModule;
namespace DiscordBotRewrite.Modules {
    public class UserAccount : ModuleData {
        #region Properties
        public const string JsonLocation = "Json/UserAccounts.json";

        [JsonProperty("balance")]
        public long Balance;
        [JsonProperty("bank")]
        public long Bank;
        [JsonProperty("bank_max")]
        public long BankMax;
        [JsonProperty("daily_cooldown")]
        public DateTime DailyCooldown;
        [JsonProperty("daily_streak")]
        public uint Streak;
        #endregion

        #region Constructor
        public UserAccount(ulong id) : base(id) {
            Balance = 0;
            Bank = 0;
            BankMax = 1000;
            DailyCooldown = DateTime.Now;
            Streak = 0;
        }
        #endregion
    }
}