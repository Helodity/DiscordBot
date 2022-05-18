namespace DiscordBotRewrite.Modules;

public class ModuleContainer {
    public VoiceModule Voice;

    public QuoteModule Quote;

    public PixelModule Pixel;

    public QuestionModule Question;


    public ModuleContainer(DiscordClient client) {
        Quote = new QuoteModule(client);

        Question = new QuestionModule();

        Pixel = new PixelModule();

        Voice = new VoiceModule(client);
        Voice.EnableLavalink().Wait();
    }
}
