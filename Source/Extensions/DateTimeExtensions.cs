namespace DiscordBotRewrite.Extensions;

public static class DateTimeExtensions {
    public static string ToTimestamp(this DateTime time, TimestampFormat format = TimestampFormat.RelativeTime) {
        return $"<t:{((DateTimeOffset)time).ToUnixTimeSeconds()}:{(char)format}>";
    }
}
