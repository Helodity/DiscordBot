using DiscordBotRewrite.Global;
using DiscordBotRewrite.LifeSim;
using DSharpPlus;

namespace DiscordBotRewrite
{
    public class ModuleContainer
    {
        #region Properties

        public readonly LifeSimModule LifeSim;
        #endregion

        #region Constructors
        public ModuleContainer(DiscordClient client)
        {
            Bot.Database.CreateTable<Cooldown>();

            LifeSim = new LifeSimModule();
        }
        #endregion
    }
}