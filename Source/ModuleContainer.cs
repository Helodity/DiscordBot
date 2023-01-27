using DiscordBotRewrite.Economy;
using DiscordBotRewrite.Global;
using DiscordBotRewrite.Pixel;
using DiscordBotRewrite.Poll;
using DiscordBotRewrite.Question;
using DiscordBotRewrite.Quote;
using DiscordBotRewrite.Voice;
using DSharpPlus;

namespace DiscordBotRewrite
{
    public class ModuleContainer
    {
        #region Properties
        public readonly EconomyModule Economy;

        public readonly VoiceModule Voice;

        public readonly QuoteModule Quote;

        public readonly PixelModule Pixel;

        public readonly QuestionModule Question;

        public readonly PollModule Poll;
        #endregion

        #region Constructors
        public ModuleContainer(DiscordClient client)
        {
            Bot.Database.CreateTable<Cooldown>();

            Economy = new EconomyModule();

            Quote = new QuoteModule(client);

            Question = new QuestionModule();

            Pixel = new PixelModule();

            Poll = new PollModule(client);

            if (Bot.Config.UseVoice)
            {
                Voice = new VoiceModule(client);
                Voice.EnableLavalink().Wait();
            }
        }
        #endregion
    }
}