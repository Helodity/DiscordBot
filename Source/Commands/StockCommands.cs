using DiscordBotRewrite.Modules;
using DiscordBotRewrite.Modules.Economy;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {

    [SlashCommandGroup("stock", "Throw away your money")]
    class StockCommands : ApplicationCommandModule {
        [SlashCommand("check", "Check if the bot is on")]
        public async Task Check(InteractionContext ctx, [Option("Name", "Name of the stock")] string name) {
            Stock s = Bot.Database.Table<Stock>().FirstOrDefault(x => x.Name == name);

            if(s == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"{s.Name}, {s.LastEarnings * 100}%, ${s.ShareCost}",
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("buy", "Check if the bot is on")]
        public async Task Buy(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to buy")] long amount) {
            Stock stock = Bot.Database.Table<Stock>().FirstOrDefault(x => x.Name == name);
            UserAccount user = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            long price = stock.ShareCost * amount;

            if(price >= user.Balance) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't afford that! It would cost ${price}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            UserStockInfo stockInfo = Bot.Modules.Economy.GetStockInfo((long)ctx.User.Id, stock.Name);
            stockInfo.Amount += amount;
            Bot.Database.Update(stockInfo);

            user.ModifyBalance(-price);        
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You bought {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("sell", "Check if the bot is on")]
        public async Task Sell(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to sell")] long amount) {
            Stock stock = Bot.Database.Table<Stock>().FirstOrDefault(x => x.Name == name);
            UserAccount user = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }
            UserStockInfo stockInfo = Bot.Modules.Economy.GetStockInfo((long)ctx.User.Id, stock.Name);


            long price = stock.ShareCost * amount;

            if(stockInfo.Amount < amount) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You only own {stockInfo.Amount} shares!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            stockInfo.Amount -= amount;
            Bot.Database.Update(stockInfo);
            user.ModifyBalance(price);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You sold {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }
    }
}