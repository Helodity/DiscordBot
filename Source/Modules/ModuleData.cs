//Help reduce boiler plate code for module data
namespace DiscordBotRewrite.Modules;
public abstract class ModuleData {
    //What guild this data is for
    [JsonProperty("guild_id")]
    public readonly ulong Id;

    public ModuleData(ulong id) {
        Id = id;
    }
}
