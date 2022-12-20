using DiscordBotRewrite.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Attributes {
    public abstract class VoiceAttribute : SlashCheckBaseAttribute {
        public bool IsBeingUsed(LavalinkGuildConnection conn) {
            return conn != null && conn.CurrentState.CurrentTrack != null && conn.MembersInChannel().Any();
        }
        public bool MemberInSameVoiceAsBot(LavalinkGuildConnection conn, InteractionContext ctx) {
            return ctx.Member.VoiceState != null && conn != null && ctx.Member.VoiceState.Channel == conn.Channel;
        }
    }
}