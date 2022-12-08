using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DiscordBotRewrite.Modules.Economy;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Commands {

    [SlashCommandGroup("stock", "Throw away your money")]
    class StockCommands : ApplicationCommandModule {
        [SlashCommand("detailed", "Get a detailed view of a single stock")]
        public async Task DetailedView(InteractionContext ctx, [Option("Name", "Name of the stock")] string name) {
            name = name.ToUpper();
            Stock s = Bot.Modules.Economy.GetStock(name);

            if(s == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            StockMarket.CreateStockGraph(name, ctx.User.Id);

            string imagePath = $"StockImages/img{ctx.User.Id}.png";
            var embed = new DiscordEmbedBuilder()
                .WithTitle(s.Name)
                .WithColor(Bot.Style.DefaultColor)
                .WithImageUrl($"attachment://{Path.GetFileName(imagePath)}")
                .WithDescription($"${s.ShareCost} ({s.GetOverallEarningPercentage()}%)");

            using(var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read)) {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
            }
            File.Delete(imagePath);
        }

        [SlashCommand("overview", "Get a quick rundown on all stocks")]
        public async Task Overview(InteractionContext ctx) {
            List<Stock> stocks = Bot.Database.GetAllWithChildren<Stock>().ToList();
            string output = "";

            foreach(Stock stock in stocks) {
                output += $"{stock.Name.ToBold()}: ${stock.ShareCost} ({stock.GetOverallEarningPercentage()}%)\n";
            }
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Stock Overview",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("posession", "See the stocks you own")]
        public async Task Owned(InteractionContext ctx) {
            List<UserStockInfo> ownedStocks = Bot.Database.Table<UserStockInfo>().ToList();

            string output = "";

            foreach(UserStockInfo info in ownedStocks) {
                Stock stock = Bot.Modules.Economy.GetStock(info.StockName);
                output += $"{stock.Name.ToBold()}: {info.Amount} (${stock.ShareCost * info.Amount})\n";
            }
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Your Shares",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("buy", "Purchase Stocks")]
        public async Task Buy(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to buy")] long amount) {
            name = name.ToUpper();
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

            stock.ModifySales(amount);

            user.ModifyBalance(-price);        
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You bought {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("sell", "Sell Stocks")]
        public async Task Sell(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to sell")] long amount) {
            name = name.ToUpper();
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
            stock.ModifySales(-amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You sold {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }
    }
}