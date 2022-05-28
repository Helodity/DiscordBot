using System.Threading.Tasks;
using DiscordBotRewrite.Modules;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Attributes {
    public class UserAbleToSummonAttribute : VoiceAttribute {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
            VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection(ctx);

            if(connection.Node == null) {
                ctx.Client.Logger.LogError("Lavalink error in UserAbleToSummonAttribute: Node does not exist");
                await ctx.CreateResponseAsync("An error occured!", true);
                return false;
            }

            if(ctx.Member.VoiceState == null) {
                await ctx.CreateResponseAsync("You need to be in a voice channel!", true);
                return false;
            }

            if(IsBeingUsed(connection.Conn)) {
                await ctx.CreateResponseAsync("I'm already being used by someone else!", true);
                return false;
            }

            return true;
        }
    }
}