namespace DiscordBotRewrite.Commands;

class UnsortedCommands : ApplicationCommandModule {
    [SlashCommand("ping", "Check if the bot is on.")]
    public async Task Ping(InteractionContext ctx) {
        await ctx.CreateResponseAsync("Pong!");
    }

    [SlashCommand("how", "Find out how __ you are.")]
    public async Task How(InteractionContext ctx, [Option("what", "how what you are")] string what) {
        await ctx.CreateResponseAsync($"You are {GenerateRandomNumber(0, 100)}% {what}.");
    }

    [SlashCommand("scp", "Gives you an SCP article to read")]
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

    [SlashCommand("8ball", "Ask a question and The Ball shall answer.")]
    public async Task EightBall(InteractionContext ctx, [Option("question", "The question for The Ball to answer")] string question) {
        string thinkStr;
        switch(GenerateRandomNumber(1, 5)) {
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
        await ctx.CreateResponseAsync($"{ctx.Member.DisplayName} questions The Ball. It {thinkStr}...");

        await Task.Delay(GenerateRandomNumber(1000, 3000));

        string output;
        switch(GenerateRandomNumber(1, 5)) {
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
        await ctx.EditResponseAsync($"{ctx.Member.DisplayName} asks: \"{question}\" \n{output}.");
    }
}