namespace DiscordBotRewrite.Extensions;

public static class DiscordUserExtensions {
    public static bool IsOwner(this DiscordUser user) {
        return user.Id == Bot.Config.OwnerId;
    }
}