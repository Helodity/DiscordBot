using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DiscordBotRewrite.Modules.Economy;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {

    [SlashCommandGroup("stock", "Throw away your money")]
    class StockCommands : ApplicationCommandModule {
       
        [SlashCommand("overview", "Get a quick rundown on all stocks")]
        public async Task Overview(InteractionContext ctx, [Option("Name", "Name of the stock")] string name = null) {

            string imagePath = $"StockImages/img{ctx.User.Id}.png";
            var embed = new DiscordEmbedBuilder()
                .WithColor(Bot.Style.DefaultColor)
                .WithImageUrl($"attachment://{Path.GetFileName(imagePath)}");

            if(name != null) {
                name = name.ToUpper();
                Stock stock = Stock.GetStock(name);
                if(stock == null) {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                        Description = "Nonexistent stock!",
                        Color = Bot.Style.ErrorColor
                    }, true);
                    return;
                }
                StockMarket.CreateDetailedGraph(stock, ctx.User.Id);
                embed
                    .WithTitle(stock.Name)
                    .WithDescription($"${stock.ShareCost} ({stock.GetEarningsPercentString()})");
             
            } else {
                StockMarket.CreateOverviewGraph(ctx.User.Id);
                embed.WithTitle("Stock Overview");
            }

            using(var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read)) {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AddFile(Path.GetFileName(imagePath), fs));
            }
            File.Delete(imagePath);
        }

        [SlashCommand("posession", "See someone's owned stocks")]
        public async Task Owned(InteractionContext ctx, [Option("user", "Who are we checking?")] DiscordUser user = null) {
            if(user == null)
                user = ctx.User;

            long castedUserID = (long)user.Id;
            List<UserStockInfo> ownedStocks = Bot.Database.Table<UserStockInfo>().Where(x =>x.UserId == castedUserID && x.Amount > 0).ToList();

            string output = "";

            foreach(UserStockInfo info in ownedStocks) {
                Stock stock = Stock.GetStock(info.StockName);
                output += $"{stock.Name.ToBold()}: {info.Amount} (${stock.ShareCost * info.Amount})\n";
            }
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = $"{user.Username}'s Shares",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("buy", "Purchase Stocks")]
        public async Task Buy(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to buy")] long amount) {
            name = name.ToUpper();

            if(amount < 1) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Can't buy that many shares",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            Stock stock = Stock.GetStock(name);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            UserAccount user = UserAccount.GetAccount((long)ctx.User.Id);
            long price = stock.ShareCost * amount;
            if(price > user.Balance) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't afford that! It would cost ${price}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            UserStockInfo stockInfo = UserStockInfo.GetStockInfo((long)ctx.User.Id, stock.Name);
            stockInfo.ModifyAmount(amount);

            stock.ModifySales(amount);

            user.Pay(price);        
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You bought {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }

        [SlashCommand("sell", "Sell Stocks")]
        public async Task Sell(InteractionContext ctx, [Option("Name", "Name of the stock")] string name, [Option("Amount", "Amount to sell")] long amount) {
            name = name.ToUpper();

            if(amount < 1) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Can't sell that many shares!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            Stock stock = Stock.GetStock(name);
            if(stock == null) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Nonexistent stock!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }
            UserAccount user = UserAccount.GetAccount((long)ctx.User.Id);
            UserStockInfo stockInfo = UserStockInfo.GetStockInfo((long)ctx.User.Id, stock.Name);

            if(stockInfo.Amount < amount) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You only own {stockInfo.Amount} shares!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            long price = stock.ShareCost * amount;
            stockInfo.ModifyAmount(-amount);
            user.Receive(price);
            stock.ModifySales(-amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You sold {amount} shares of {name} for ${price}!",
                Color = Bot.Style.DefaultColor
            });
        }
    }
}