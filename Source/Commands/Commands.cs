namespace DiscordBotRewrite.Commands;

class UnsortedCommands : ApplicationCommandModule {
    [SlashCommand("ping", "Check if the bot is on.")]
    public async Task Ping(InteractionContext ctx) {
        await BotUtils.CreateBasicResponse(ctx, $"Pong!");
    }

    [SlashCommand("how", "Find out how __ you are.")]
    public async Task How(InteractionContext ctx, [Option("what", "how what you are")] string what) {
        await BotUtils.CreateBasicResponse(ctx, $"You are {BotUtils.GenerateRandomNumber(0, 100)}% {what}.");
    }

    [SlashCommand("scp", "Gives you an SCP article to read")]
    public async Task RandomScp(InteractionContext ctx) {
        int number = BotUtils.GenerateRandomNumber(1, 2000);
        string output = "http://www.scpwiki.com/scp-";

        if(number < 10) {
            output += "00";
        } else if(number < 100) {
            output += "0";
        }
        output += number.ToString();

        await BotUtils.CreateBasicResponse(ctx, output);
    }

    [SlashCommand("8ball", "Ask a question and The Ball shall answer.")]
    public async Task EightBall(InteractionContext ctx, [Option("question", "The question for The Ball to answer")] string question) {
        int thinkingNum = BotUtils.GenerateRandomNumber(1, 5);
        string thinkStr;
        switch(thinkingNum) {
            case 1:
                thinkStr = "ponders";
                break;
            case 2:
                thinkStr = "imagines";
                break;
            case 3:
                thinkStr = "thinks";
                break;
            case 4:
                thinkStr = "judges";
                break;
            default:
                thinkStr = "reckons";
                break;

        }
        await BotUtils.CreateBasicResponse(ctx, $"{ctx.Member.DisplayName} questions The Ball. It {thinkStr}...");

        int delay = BotUtils.GenerateRandomNumber(1000, 3000);
        await Task.Delay(delay);

        int result = BotUtils.GenerateRandomNumber(1, 5);
        string output;
        switch(result) {
            case 1:
                output = "Likely";
                break;
            case 2:
                output = "Unlikely";
                break;
            case 3:
                output = "Chances say yes";
                break;
            case 4:
                output = "Probably not";
                break;
            default:
                output = "Ask again";
                break;

        }
        await BotUtils.EditBasicResponse(ctx, $"{ctx.Member.DisplayName} asks: \"{question}\" \n{output}.");
    }
}