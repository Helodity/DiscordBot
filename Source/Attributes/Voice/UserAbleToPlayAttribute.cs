﻿namespace DiscordBotRewrite.Attributes;
public class UserAbleToPlayAttribute : VoiceAttribute {
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
        VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection(ctx);

        if(connection.Node == null) {
            ctx.Client.Logger.LogError("Lavalink error in UserAbleToPlayAttribute: Node does not exist");
            await ctx.CreateResponseAsync("An error occured! My owner has been notified.", true);
            return false;
        }

        if(!MemberInSameVoiceAsBot(connection.Conn, ctx)) {
            if(!connection.IsConnected) {
                if(ctx.Member.VoiceState == null) {
                    await ctx.CreateResponseAsync("You need to be in a voice channel!", true);
                    return false;
                }
            } else {
                if(IsBeingUsed(connection.Conn)) {
                    await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
                    return false;
                }
            }
        }

        return true;
    }

}