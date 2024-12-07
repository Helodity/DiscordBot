using DiscordBotRewrite.Economy;
using DiscordBotRewrite.Global;
using DSharpPlus;

namespace DiscordBotRewrite
{
    public class ModuleContainer
    {
        #region Properties
        public readonly EconomyModule Economy;

        #endregion

        #region Constructors
        public ModuleContainer(DiscordClient client)
        {
            Bot.Database.CreateTable<Cooldown>();

            Economy = new EconomyModule();
        }
        #endregion
    }
}