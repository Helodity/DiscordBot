using System.Threading.Tasks;
using DiscordBotRewrite.Modules;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Attributes {
    public class UserAbleToPlayAttribute : VoiceAttribute {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx) {
            VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection((InteractionContext)ctx);

            if(connection.Node == null) {
                ctx.Client.Logger.LogError("Lavalink error in UserAbleToPlayAttribute: Node does not exist");
                await ctx.CreateResponseAsync("An error occured! My owner has been notified.", true);
                return false;
            }

            if(!MemberInSameVoiceAsBot(connection.Conn, (InteractionContext)ctx)) {
                if(connection.IsConnected) {
                    if(IsBeingUsed(connection.Conn)) {
                        await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
                        return false;
                    }
                } else {
                    if(ctx.Member.VoiceState == null) {
                        await ctx.CreateResponseAsync("You need to be in a voice channel!", true);
                        return false;
                    }
                }
            }

            return true;
        }

    }
}