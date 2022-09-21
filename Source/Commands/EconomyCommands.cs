using System;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("economy", "money")]
    class EconomyCommands : ApplicationCommandModule {
        #region Balance
        [SlashCommand("balance", "Money?")]
        public async Task Balance(InteractionContext ctx, [Option("user", "who are we looking at?")] DiscordUser user = null) {
            if(user == null)
                user = ctx.User;

            if(user.IsBot) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Ok look, bots are broke. Give them a break.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            UserAccount account = Bot.Modules.Economy.GetAccount(user.Id);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = $"{user.Username}'s Account",
                Description = $"Balance: ${account.Balance}\nBank: ${account.Bank}/${account.BankMax}",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Withdraw
        [SlashCommand("withdraw", "Pocket!!!!")]
        public async Task Withdraw(InteractionContext ctx, [Option("amount", "how much inflation")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount(ctx.User.Id);
            if(amount > account.Bank)
                amount = account.Bank;

            account.Bank -= amount;
            account.Balance += amount;
            Bot.Modules.Economy.SaveAccounts();

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Withdrew ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Deposit
        [SlashCommand("deposit", "Bank!!!!")]
        public async Task Deposit(InteractionContext ctx, [Option("amount", "how much inflation")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount(ctx.User.Id);
            if(account.Bank + amount > account.BankMax)
                amount = account.BankMax - account.Bank;
            if(amount > account.Balance)
                amount = account.Balance;

            account.Balance -= amount;
            account.Bank += amount;
            Bot.Modules.Economy.SaveAccounts();

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Deposited ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Give
        [SlashCommand("give", "Not your money!!")]
        public async Task Give(InteractionContext ctx, [Option("amount", "how much inflation")] long amount, [Option("user", "who gets your money?")] DiscordUser user) {
            amount = Bot.Modules.Economy.Transfer(ctx.User.Id, user.Id, (int)amount);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Transferred ${amount} to {user.Username}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Daily
        [SlashCommand("daily", "Print a reasonable amount of money.")]
        public async Task Daily(InteractionContext ctx) {
            UserAccount account = Bot.Modules.Economy.GetAccount(ctx.User.Id);
            if(DateTime.Compare(DateTime.Now,account.DailyCooldown) < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't claim your daily reward yet!\nYou can claim more money {account.DailyCooldown.ToTimestamp()}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            long amount = 100;
            amount = (long)(amount * Math.Pow(1.3, account.Streak));
            //48 total hours, daily cooldown accounts for 20
            if((account.DailyCooldown - DateTime.Now).TotalHours > 28) {
                account.Streak = 0;
            } else {
                account.Streak += 1;
            }

            account.DailyCooldown = DateTime.Now.AddHours(20);
            account.Balance += amount;
            Bot.Modules.Economy.SaveAccounts();

            string streakText = string.Empty;
            if(account.Streak > 0) {
                streakText += account.Streak;
                if(account.Streak > 1) {
                    streakText += " days in a row!";
                } else {
                    streakText += " day in a row!";
                }
            } else {
                streakText = "You lost your streak!";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Claimed your daily ${amount}\n{streakText}",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Total
        [SlashCommand("total", "Money?")]
        public async Task Total(InteractionContext ctx) {
            long amount = Bot.Modules.Economy.GetTotalBalance();
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"There is ${amount} in circulation",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Rob
        [SlashCommand("rob", "My Money!")]
        public async Task Rob(InteractionContext ctx, [Option("user", "Who are you stealing from?")] DiscordUser user) {
            UserAccount target = Bot.Modules.Economy.GetAccount(user.Id);

            if(ctx.User == user) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You... can't rob yourself.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            if(target.Balance == 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"{user.Username} is kinda broke, maybe pick a better target",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            if(Bot.Modules.Economy.HasRobCooldown(ctx.User.Id, out var cooldown)) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You're still under suspicion from your last attempt, try again {cooldown.EndTime.ToTimestamp()}.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            Bot.Modules.Economy.AddRobCooldown(ctx.User.Id);
            int rng = GenerateRandomNumber(-10, 10);
            if(rng < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You couldn't find an opening, try again later!",
                    Color = Bot.Style.WarningColor
                });
                return;
            }

            long amount = (long)(target.Balance * 0.02 * rng);
            amount = Bot.Modules.Economy.Transfer(user.Id, ctx.User.Id, amount);


            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"You got ${amount} from {user.Username}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Highlow
        [SlashCommand("highlow", "Money?")]
        public async Task HighLow(InteractionContext ctx, [Option("bet", "how much to lose")] long bet) {
            UserAccount account = Bot.Modules.Economy.GetAccount(ctx.User.Id);

            if(account.Balance < bet) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"This isn't the stock market, you can only bet what's in your pocket.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            //Just make sure they can't get robbed mid game.
            account.Balance -= bet;

            DiscordButtonComponent[] highLowButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "higher", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"))),
                new DiscordButtonComponent(ButtonStyle.Primary, "lower", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"))),
            };

            DiscordButtonComponent[] continueQuitButtons = {
                new DiscordButtonComponent(ButtonStyle.Success, "continue", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "quit", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:"))),
            };

            await ctx.DeferAsync();

            var interactivity = ctx.Client.GetInteractivity();

            var embed = new DiscordEmbedBuilder {
                Title = "Higher or Lower",
            };
            bool gameEnded = false;
            bool hasLost = false;
            int gamesWon = 0;
            int anchorNumber = GenerateRandomNumber(1, 100), actualNumber = anchorNumber;
            while(!gameEnded) {
                while(actualNumber == anchorNumber)
                    actualNumber = GenerateRandomNumber(1, 100);
                embed.WithColor(Bot.Style.DefaultColor);
                embed.WithDescription($"{ctx.User.Mention}, I'm thinking of a number between 1 and 100. Is it higher or lower than {anchorNumber}?");


                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(highLowButtons));
                var input = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                if(input.TimedOut) {
                    //Act as if the game never happened
                    account.Balance += bet;
                    embed.WithDescription("Sure ok just leave on me. Fuck off.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    break;
                }
                await input.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                if(input.Result.Id == "lower") {
                    hasLost = anchorNumber < actualNumber;
                }
                if(input.Result.Id == "higher") {
                    hasLost = anchorNumber > actualNumber;
                }

                if(hasLost) {
                    embed.WithColor(Bot.Style.ErrorColor);
                    embed.WithDescription($"Sorry, the number was {actualNumber}. You lost your ${bet}.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    break;
                }

                gamesWon++;
                long currentWinnings = (long)MathF.Pow(2, gamesWon) * bet + bet;
                embed.WithColor(Bot.Style.SuccessColor);
                embed.WithDescription($"Congrats, {ctx.User.Mention}, the number was {actualNumber}! You've won {gamesWon} rounds.\n " +
                    $"You currently have ${currentWinnings}. You can risk it for more or stop here. Would you like to continue?");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(continueQuitButtons));

                var continueResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                if(continueResult.TimedOut) {
                    embed.WithDescription($"You cashed out with {currentWinnings}.");
                    account.Balance += currentWinnings;
                    Bot.Modules.Economy.SaveAccounts();
                    gameEnded = true;
                }
                await continueResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                if(continueResult.Result.Id == "quit") {
                    embed.WithDescription($"You cashed out with ${currentWinnings}.");
                    account.Balance += currentWinnings;
                    Bot.Modules.Economy.SaveAccounts();
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    gameEnded = true;
                }
                anchorNumber = actualNumber;
            }

        }
        #endregion
    }
}