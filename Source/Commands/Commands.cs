using DiscordBotRewrite.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {
    class UnsortedCommands : ApplicationCommandModule {
        #region Ping
        [SlashCommand("ping", "Check if the bot is on")]
        public async Task Ping(InteractionContext ctx) {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "Pong!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region How
        [SlashCommand("how", "Find out how __ you are")]
        public async Task How(InteractionContext ctx, [Option("what", "how what you are")] string what) {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You are {GenerateRandomNumber(0, 100)}% {what}.",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region SCP
        [SlashCommand("scp", "Get an SCP article to read")]
        public async Task RandomScp(InteractionContext ctx) {
            int number = GenerateRandomNumber(1, 2000);
            string link = "http://www.scpwiki.com/scp-";

            //SCP articles are titled 001, 096, with a minimum of three digits. Add any extra zeros here.
            int numLength = number.ToString().Length;
            for(int i = numLength; i < 3; i++) {
                link += "0";
            }
            link += number.ToString();

            await ctx.CreateResponseAsync(link);
        }
        #endregion

        #region Eight Ball
        [SlashCommand("8ball", "Ask a question and The Ball shall answer")]
        public async Task EightBall(InteractionContext ctx, [Option("question", "The question for The Ball to answer")] string question) {
            string thinkStr = GenerateRandomNumber(1, 5) switch {
                1 => "ponders",
                2 => "imagines",
                3 => "thinks",
                4 => "judges",
                _ => "reckons",
            };
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"{ctx.Member.DisplayName} questions The Ball. It {thinkStr}...",
                Color = Bot.Style.DefaultColor
            });

            await Task.Delay(GenerateRandomNumber(1000, 3000));
            string output = GenerateRandomNumber(1, 5) switch {
                1 => "Likely",
                2 => "Unlikely",
                3 => "Chances say yes",
                4 => "Probably not",
                _ => "Ask again",
            };
            await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                Description = $"{ctx.Member.DisplayName} asks: \"{question}\" \n{output}.",
                Color = Bot.Style.DefaultColor
            });

        }
        #endregion
    }
}