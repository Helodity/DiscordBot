using DiscordBotRewrite.Modules;
using DiscordBotRewrite.Modules.Economy;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {

    [SlashCommandGroup("stock", "Throw away your money")]
    class StockCommands : ApplicationCommandModule {
        [SlashCommand("detailed", "Get a detailed view of a single stock")]
        public async Task DetailedView(InteractionContext ctx, [Option("Name", "Name of the stock")] string name) {
            Stock s = Bot.Modules.Economy.GetStock(name);

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

        [SlashCommand("overview", "Get a quick rundown on all stocks")]
        public async Task Overview(InteractionContext ctx) {
            throw new NotImplementedException();
        }

        [SlashCommand("posession", "See the stocks you own")]
        public async Task Owned(InteractionContext ctx) {
            throw new NotImplementedException();
        }

        [SlashCommand("buy", "Purchase Stocks")]
        public async Task Buy(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to buy")] long amount) {
            Stock stock = Bot.Modules.Economy.GetStock(name);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            UserAccount user = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            long price = stock.ShareCost * amount;
            if(price >= user.Balance) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't afford that! It would cost ${price}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            UserStockInfo stockInfo = Bot.Modules.Economy.GetStockInfo((long)ctx.User.Id, stock.Name);
            stockInfo.ModifyAmount(amount);
            Bot.Database.Update(stockInfo);

            user.ModifyBalance(-price);        
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You bought {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("sell", "Sell Stocks")]
        public async Task Sell(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to sell")] long amount) {
            Stock stock = Bot.Modules.Economy.GetStock(name);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }
            UserAccount user = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            UserStockInfo stockInfo = Bot.Modules.Economy.GetStockInfo((long)ctx.User.Id, stock.Name);

            if(stockInfo.Amount < amount) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You only own {stockInfo.Amount} shares!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            long price = stock.ShareCost * amount;
            stockInfo.ModifyAmount(-amount);
            user.ModifyBalance(price);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You sold {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }
    }
}