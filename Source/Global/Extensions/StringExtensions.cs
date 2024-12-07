namespace DiscordBotRewrite.Global.Extensions
{
    public static class StringExtensions
    {
        public static string ToItalics(this string value)
        {
            return "*" + value + "*";
        }
        public static string ToBold(this string value)
        {
            return "**" + value + "**";
        }
        public static string ToCode(this string value)
        {
            return "`" + value + "`";
        }
        public static string ToStrikethrough(this string value)
        {
            return "~~" + value + "~~";
        }
        public static string ToUnderline(this string value)
        {
            return "__" + value + "__";
        }
        public static string ToFirstUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return char.ToUpper(str[0]) + str[1..].ToLower();
        }
    }
}