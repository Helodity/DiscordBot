namespace DiscordBotRewrite.Modules;
public abstract class VoiceAttribute : SlashCheckBaseAttribute {
    public bool IsBeingUsed(LavalinkGuildConnection conn) {
        return conn != null && conn.CurrentState.CurrentTrack != null && conn.AmountOfMembersInChannel() > 0;
    }
    public bool MemberInSameVoiceAsBot(LavalinkGuildConnection conn, InteractionContext ctx) {
        return ctx.Member.VoiceState != null && conn != null && ctx.Member.VoiceState.Channel == conn.Channel;
    }
}