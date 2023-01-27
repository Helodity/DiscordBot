using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace DiscordBotRewrite.Global.Extensions
{
    public static class LavalinkGuildExtensions
    {
        public static List<DiscordMember> MembersInChannel(this LavalinkGuildConnection conn)
        {
            return conn.Channel.Users.Where(x => !x.IsBot).ToList();
        }
    }
}