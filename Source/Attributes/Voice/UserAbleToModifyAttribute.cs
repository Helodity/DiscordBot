using System.Threading.Tasks;

using DiscordBotRewrite.Modules;

using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Attributes {
    public class UserAbleToModifyAttribute : VoiceAttribute {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx) {
            VoiceGuildConnection connection = Bot.Modules.Voice.GetGuildConnection((InteractionContext)ctx);

            if(connection.Node == null) {
                ctx.Client.Logger.LogError("Lavalink error in UserAbleToModifyAttribute: Node does not exist");
                await ctx.CreateResponseAsync("An error occured!", true);
                return false;
            }

            if(!MemberInSameVoiceAsBot(connection.Conn, (InteractionContext)ctx)) {
                await ctx.CreateResponseAsync("You need to be in the same channel as me!", true);
                return false;
            }
            return true;
        }
    }
}