//Help reduce boiler plate code for module data
namespace DiscordBotRewrite.Modules;
public abstract class ModuleData {
    //What guild this data is for
    [JsonProperty("guild_id")]
    public readonly ulong GuildId;

    public ModuleData(ulong id) {
        GuildId = id;
    }
}
