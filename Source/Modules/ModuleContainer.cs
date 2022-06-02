using DSharpPlus;

namespace DiscordBotRewrite.Modules {
    public class ModuleContainer {
        #region Properties
        public readonly VoiceModule Voice;

        public readonly QuoteModule Quote;

        public readonly PixelModule Pixel;

        public readonly QuestionModule Question;

        public readonly PollModule Poll;
        #endregion

        #region Constructors
        public ModuleContainer(DiscordClient client) {
            Quote = new QuoteModule(client);

            Question = new QuestionModule();

            Pixel = new PixelModule();

            Poll = new PollModule(client);
            if(Bot.Config.UseVoice) {
                Voice = new VoiceModule(client);
                Voice.EnableLavalink().Wait();
            }
        }
        #endregion
    }
}