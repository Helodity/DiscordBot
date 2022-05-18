﻿namespace DiscordBotRewrite.Attributes;
public class UserAbleToModifyAttribute : VoiceAttribute {
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
        VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection(ctx);

        if(connection.Node == null) {
            ctx.Client.Logger.LogError("Lavalink error in UserAbleToModifyAttribute: Node does not exist");
            await ctx.CreateResponseAsync("An error occured!", true);
            return false;
        }

        if(!MemberInSameVoiceAsBot(connection.Conn, ctx)) {
            await ctx.CreateResponseAsync("You need to be in the same channel as me!", true);
            return false;
        }
        return true;
    }
}