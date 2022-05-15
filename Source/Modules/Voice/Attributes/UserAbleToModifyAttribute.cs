namespace DiscordBotRewrite.Modules;
public class UserAbleToModifyAttribute : VoiceAttribute {
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
        VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection(ctx);

        if(connection.Node == null) {
            ctx.Client.Logger.LogError("Lavalink error in CanUseModifyCommand: Node does not exist");
            await ctx.CreateResponseAsync("An error occured! My owner has been notified.", true);
            return false;
        }

        if(connection.Conn == null) {
            await ctx.CreateResponseAsync("I'm not connected to a channel!", true);
            return false;
        }

        if(IsBeingUsed(connection.Conn) && !MemberInSameVoiceAsBot(connection.Conn, ctx)) {
            await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
            return false;
        }

        return true;
    }
}