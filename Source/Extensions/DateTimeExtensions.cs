using DSharpPlus;
using System;

namespace DiscordBotRewrite.Extensions {
    public static class DateTimeExtensions {
        public static string ToTimestamp(this DateTime time, TimestampFormat format = TimestampFormat.RelativeTime) {
            return $"<t:{((DateTimeOffset)time).ToUnixTimeSeconds()}:{(char)format}>";
        }

        public static DateTime AddTime(this DateTime time, int amount, TimeUnit unit = TimeUnit.Seconds) {
            switch(unit) {
                case TimeUnit.Seconds: return time.AddSeconds(amount);
                case TimeUnit.Minutes: return time.AddMinutes(amount);
                case TimeUnit.Hours: return time.AddHours(amount);
                case TimeUnit.Days: return time.AddDays(amount);
                case TimeUnit.Months: return time.AddMonths(amount);
                case TimeUnit.Years: return time.AddYears(amount);
                default: throw new ArgumentOutOfRangeException(nameof(unit));
            }
        }
    }
}