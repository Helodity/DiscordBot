namespace DiscordBotRewrite.Modules;
public class Vote {
    public readonly ulong VoterId;
    public string Choice;


    public Vote(ulong id, string choice) {
        VoterId = id;
        Choice = choice;
    }
}
