using System;
using System.Collections.Generic;

namespace DiscordBotRewrite.Global {
    public readonly struct Cooldown {
        #region Properties
        public readonly DateTime EndTime;
        public bool IsOver => DateTime.Compare(DateTime.Now, EndTime) >= 0;
        #endregion

        #region Constructors
        public Cooldown(DateTime endTime) {
            EndTime = endTime;
        }
        #endregion

        #region Public
        public static bool UserHasCooldown(ulong userId, ref Dictionary<ulong, Cooldown> cooldowns) {
            if(cooldowns.TryGetValue(userId, out Cooldown cooldown)) {
                if(!cooldown.IsOver)
                    return true;
                cooldowns.Remove(userId);
            }
            return false;
        }
        public static TimeSpan TimeUntilExpiration(ulong userId, ref Dictionary<ulong, Cooldown> cooldowns) {
            if(cooldowns.TryGetValue(userId, out Cooldown cooldown)) {
                if(!cooldown.IsOver)
                    return cooldown.EndTime - DateTime.Now;
                cooldowns.Remove(userId);
            }
            return TimeSpan.Zero;
        }
        #endregion

        #region Private
        public TimeSpan TimeUntilExpiration() {
            return EndTime - DateTime.Now;
        }
        #endregion
    }
}