//Help reduce boiler plate code for module data
using Newtonsoft.Json;

namespace DiscordBotRewrite.Modules {
    public abstract class ModuleData {
        #region Properties
        //What guild/user this data is for
        [JsonProperty("id")]
        public readonly ulong Id;
        #endregion

        #region Constructors
        public ModuleData(ulong id) {
            Id = id;
        }
        #endregion
    }
}