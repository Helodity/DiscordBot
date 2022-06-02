//Help reduce boiler plate code for module data
using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public abstract class ModuleData {
        #region Properties
        //What guild this data is for
        [JsonProperty("guild_id")]
        public readonly ulong GuildId;
        #endregion

        #region Constructors
        public ModuleData(ulong id) {
            GuildId = id;
        }
        #endregion
    }
}