using DSharpPlus.Entities;

namespace DiscordBotRewrite.Global.Extensions
{
    public static class DiscordUserExtensions
    {
        public static bool IsOwner(this DiscordUser user)
        {
            return Bot.Client.CurrentApplication.Owners.Contains(user);
        }
    }
}