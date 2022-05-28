using System.Linq;
using DSharpPlus.Entities;

namespace DiscordBotRewrite.Extensions {
    public static class DiscordUserExtensions {
        public static bool IsOwner(this DiscordUser user) {
            return Bot.Client.CurrentApplication.Owners.Contains(user);
        }
    }
}