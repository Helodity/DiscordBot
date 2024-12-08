using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DiscordBotRewrite.LifeSim;
using DiscordBotRewrite.Global.Attributes;
using DSharpPlus;

namespace DiscordBotRewrite.General
{

    public class UnsortedCommands : ApplicationCommandModule
    {
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
    }

    [SlashCommandGroup("location", "look at simulation locations")]
    public class LocationCommands : ApplicationCommandModule
    {
        [SlashCommand("list", "List all locations!")]
        public async Task LocationList(InteractionContext ctx)
        {
            int locCount = Bot.Modules.LifeSim.Locations.Count();
            string description = $"**There {(locCount != 1 ? $"are {locCount} locations" : $"is {locCount} location")}:**\n";
            foreach (SimulationLocation c in Bot.Modules.LifeSim.Locations)
            {
                int charAmt = Bot.Database.Table<SimulationCharacter>().Where(x => x.GuildID == (long)ctx.Guild.Id && x.Current_Location == c.Name).Count();
                description += charAmt > 0 ? $"**{c.Name}**\n" : $"{c.Name}\n";
            }
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = description,
                Color = Bot.Style.DefaultColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Bold text means there are characters here!"
                }
            });
        }

        [SlashCommand("info", "Get detailed info about a specific character!")]
        public async Task LocationInfo(InteractionContext ctx, [Option("locationName", "LocationName")] string locationName)
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            SimulationLocation? location = Bot.Modules.LifeSim.GetSimulationLocation(locationName);
            List<SimulationCharacter> characters = Bot.Database.Table<SimulationCharacter>()
            .Where(x => x.GuildID == (long)ctx.Guild.Id && x.Current_Location == locationName).ToList();

            if (location == null)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"Could not find the location {locationName}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            int charCount = characters.Count();
            string description = $"**There {(charCount != 1 ? $"are {charCount} characters" : $"is {charCount} character")} at the {locationName}**\n";

            foreach (SimulationCharacter c in characters)
            {
                description += $"{c}\n";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Title = $"{locationName}",
                Description = description,
                Color = Bot.Style.DefaultColor
            });
        }
    }

    [SlashCommandGroup("character", "look at simulation characters")]
    public class CharacterCommands : ApplicationCommandModule
    {
        [SlashCommand("list", "List all the current characters!")]
        public async Task CharacterList(InteractionContext ctx)
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            List<SimulationCharacter> allCharacters = simData.GetAllCharacters();

            int charCount = allCharacters.Count();
            string description = $"**There {(charCount != 1 ? $"are {charCount} characters" : $"is {charCount} character")}:**\n";

            foreach (SimulationCharacter c in allCharacters)
            {
                description += $"{c}\n";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = description,
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("info", "Get detailed info about a specific character!")]
        public async Task CharacterInfo(InteractionContext ctx, [Option("ID", "Character's ID")] long charID)
        {
            GuildSimulationData simData = Bot.Modules.LifeSim.GetSimulationData(ctx.Guild.Id);
            SimulationCharacter c = simData.GetAllCharacters().FirstOrDefault(x => x.Id == charID && x.GuildID == (long)ctx.Guild.Id);

            if (c == null)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"Could not find a character with ID {charID}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            string locationText = "";
            if (c.Current_Location_Duration == 0)
            {
                locationText = $"Just got {(c.Current_Location == "None" ? "home" : $"to the {c.Current_Location}")}.";
            }
            else
            {
                locationText = $"Has been {(c.Current_Location == "None" ? "home" : $"at the {c.Current_Location}")} for {c.Current_Location_Duration} minutes.";
            }

            string relationshipText = "";
            List<SimulationRelationship> relationships = Bot.Database.Table<SimulationRelationship>().Where(x => x.OwnerID == c.Id).ToList();
            if (relationships.Count() == 0)
            {
                relationshipText += "None!";
            }
            else
            {
                for (int i = 0; i < relationships.Count(); i++)
                {
                    int targetID = relationships[i].TargetID;
                    SimulationCharacter otherChar = Bot.Database.Table<SimulationCharacter>().FirstOrDefault(x => x.Id == targetID);
                    if (otherChar == null)
                    {
                        continue;
                    }
                    relationshipText += $"{otherChar.FirstName} {otherChar.LastName}: {relationships[i].GetQuantizedString()}\n";
                }
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Title = c.ToString(),
                Color = Bot.Style.DefaultColor,
            }.AddField("**Info**", $"{LifeSimModule.TicksToYears(c.AgeTicks)} years old.", true)
                .AddField("**Location**", locationText, true)
                .AddField("**Relationships**", relationshipText)
            );
        }
    }
}