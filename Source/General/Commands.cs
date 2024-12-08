using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DiscordBotRewrite.LifeSim;
using DiscordBotRewrite.Global.Attributes;
using DSharpPlus;

namespace DiscordBotRewrite.General
{
    public class UnsortedCommands : ApplicationCommandModule
    {
        #region Ping
        [SlashCommand("name_test", "Check if the bot is on")]
        public async Task Ping(InteractionContext ctx)
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            List<SimulationCharacter> allCharacters = simData.GetAllCharacters();

            string totalName = $"{allCharacters.Count()}: ";
            foreach (SimulationCharacter c in allCharacters)
            {
                totalName += $"{c.FirstName} {c.LastName}, ";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = totalName,
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("character_info", "Check if the bot is on")]
        public async Task CharacterInfo(
            InteractionContext ctx, [Option("firstName", "Character's first name")] string firstName, [Option("lastName", "Character's last name")] string lastName
            )
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            SimulationCharacter c = simData.GetAllCharacters().FirstOrDefault(x => x.FirstName == firstName && x.LastName == lastName && x.GuildID == (long)ctx.Guild.Id);

            if (c == null)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"Could not find the character {firstName} {lastName}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            string text = $"{LifeSimModule.TicksToYears(c.AgeTicks)} years old.";
            string locationText = $"Has been home for {c.Current_Location_Duration} minutes.";

            if (c.Current_Location != "None")
            {
                locationText = $"Has been at the {c.Current_Location} for {c.Current_Location_Duration} minutes.";
            }

            List<SimulationRelationship> relationships = Bot.Database.Table<SimulationRelationship>().Where(x => x.OwnerID == c.Id).ToList();
            string description = "";
            description += "**Relationships**\n";

            if (relationships.Count() == 0)
            {
                description += "None!\n";
            }
            else
            {
                for (int i = 0; i < relationships.Count(); i++)
                {
                    SimulationCharacter otherChar = Bot.Database.Table<SimulationCharacter>().FirstOrDefault(x => x.Id == relationships[i].TargetID);
                    if (otherChar == null)
                    {
                        continue;
                    }
                    description += $"{otherChar.FirstName} {otherChar.LastName}: {relationships[i].Friendship}";
                }
            }


            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Title = $"{c.FirstName} {c.LastName}",
                Description = description,
                Color = Bot.Style.DefaultColor
            });
        }



        [SlashCommand("toggle", "Pause or unpause the simulation.")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Toggle(InteractionContext ctx)
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            simData.SimulationRunning = !simData.SimulationRunning;
            Bot.Database.Update(simData);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"{(simData.SimulationRunning ? "Enabled" : "Disabled")} auto quoting!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

    }
}