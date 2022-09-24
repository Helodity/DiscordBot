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

            UserAccount account = Bot.Modules.Economy.GetAccount((long)user.Id);
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
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(amount > account.Bank)
                amount = account.Bank;

            account.Bank -= amount;
            account.Balance += amount;
            Bot.Database.Update(account);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Withdrew ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Deposit
        [SlashCommand("deposit", "Bank!!!!")]
        public async Task Deposit(InteractionContext ctx, [Option("amount", "how much inflation")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(account.Bank + amount > account.BankMax)
                amount = account.BankMax - account.Bank;
            if(amount > account.Balance)
                amount = account.Balance;

            account.Balance -= amount;
            account.Bank += amount;
            Bot.Database.Update(account);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Deposited ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Give
        [SlashCommand("give", "Not your money!!")]
        public async Task Give(InteractionContext ctx, [Option("amount", "how much inflation")] long amount, [Option("user", "who gets your money?")] DiscordUser user) {
            amount = Bot.Modules.Economy.Transfer((long)ctx.User.Id, (long)user.Id, (int)amount);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Transferred ${amount} to {user.Username}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Daily
        [SlashCommand("daily", "Print a reasonable amount of money.")]
        public async Task Daily(InteractionContext ctx) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
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
            Bot.Database.Update(account);

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
            var accounts = Bot.Modules.Economy.GetUserAccounts();

            long amount = 0;
            foreach(UserAccount a in accounts) {
                amount += a.Balance;
                amount += a.Bank;
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"There is ${amount} in circulation",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Rob
        [SlashCommand("rob", "My Money!")]
        public async Task Rob(InteractionContext ctx, [Option("user", "Who are you stealing from?")] DiscordUser user) {
            UserAccount self = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            UserAccount target = Bot.Modules.Economy.GetAccount((long)user.Id);

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
            if(DateTime.Compare(DateTime.Now, self.RobCooldown) < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You're still under suspicion from your last attempt, try again {self.RobCooldown.ToTimestamp()}.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            self.RobCooldown = DateTime.Now.AddSeconds(30);
            int rng = GenerateRandomNumber(0, 10);
            if(rng == 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You couldn't find an opening, try again later!",
                    Color = Bot.Style.WarningColor
                });
            } else {
                long amount = (long)(target.Balance * 0.01 * rng);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You got ${amount} from {user.Username}!",
                    Color = Bot.Style.SuccessColor
                });
                self.Balance += amount;
                target.Balance -= amount;
                Bot.Database.Update(target);
            }
            Bot.Database.Update(self);
        }
        #endregion

        #region Highlow
        [SlashCommand("highlow", "Money?")]
        public async Task HighLow(InteractionContext ctx, [Option("bet", "how much to lose")] long bet) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);

            if(bet < 0) {
                account.Balance -= 1;
                Bot.Database.Update(account);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Alright bitchass stop trying to game the system. I'm taking a dollar from you cuz of that.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

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
                new DiscordButtonComponent(ButtonStyle.Success, "continue", "Keep Playing"),
                new DiscordButtonComponent(ButtonStyle.Danger, "quit", "Cash Out"),
            };

            await ctx.DeferAsync();

            var interactivity = ctx.Client.GetInteractivity();

            var embed = new DiscordEmbedBuilder {
                Title = "Higher or Lower",
            };
            bool gameEnded = false;
            bool hasLost = false;
            int gamesWon = 0;
            Deck deck = Deck.GetStandardDeck();
            Card anchorCard = deck.Draw();
            while(!gameEnded) {
                Card nextCard = deck.Draw();
                embed.WithColor(Bot.Style.DefaultColor);
                embed.WithDescription($"{ctx.User.Mention}, Will the next card I draw be higher or lower than a {anchorCard}?");


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
                    hasLost = anchorCard.value < nextCard.value;
                }
                if(input.Result.Id == "higher") {
                    hasLost = anchorCard.value > nextCard.value;
                }

                if(hasLost) {
                    embed.WithColor(Bot.Style.ErrorColor);
                    embed.WithDescription($"Sorry, I drew {nextCard}. You lost your ${bet} bet.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    break;
                }

                gamesWon++;
                long currentWinnings = (long)(bet * Bot.Modules.Economy.GetWinningsMultiplier(gamesWon, 0.4));
                long nextWinnings = (long)(bet * Bot.Modules.Economy.GetWinningsMultiplier(gamesWon + 1, 0.4));
                int cardsLeft = deck.Size();

                embed.WithColor(Bot.Style.SuccessColor);
                if(cardsLeft == 0) {
                    embed.WithDescription($"Congrats, {ctx.User.Mention}, I drew {nextCard}! There are no cards left!.\n " +
                    $"You win {currentWinnings}!");
                    account.Balance += currentWinnings;
                    Bot.Database.Update(account);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                embed.WithDescription($"Congrats, {ctx.User.Mention}, I drew {nextCard}! There are {deck.Size()}/52 cards left!.\n " +
                    $"You can cash out with your ${bet} bet and ${currentWinnings - bet} in winnings, or risk it all for ${nextWinnings - currentWinnings} more.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(continueQuitButtons));

                var continueResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                if(!continueResult.TimedOut)
                    await continueResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                if(continueResult.TimedOut || continueResult.Result.Id == "quit") {
                    embed.WithDescription($"You cashed out with ${currentWinnings}.");
                    account.Balance += currentWinnings;
                    Bot.Database.Update(account);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    gameEnded = true;
                }
                anchorCard = nextCard;
            }
        }
        #endregion

        #region Blackjack
        [SlashCommand("blackjack", "Play blackjack against the bot")]
        public async Task BlackJack(InteractionContext ctx, [Option("bet", "how much to lose")] long bet) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);

            if(bet < 0) {
                account.Balance -= 1;
                Bot.Database.Update(account);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Alright bitchass stop trying to game the system. I'm taking a dollar from you cuz of that.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            if(account.Balance < bet) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"This isn't the stock market, you can only bet what's in your pocket.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            //Just make sure they can't get robbed mid game.
            account.Balance -= bet;



            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                //Description = $"{c}",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region baltop
        [SlashCommand("baltop", "Money?")]
        public async Task Baltop(InteractionContext ctx) {
            var accounts = Bot.Modules.Economy.GetUserAccounts();

            accounts.Sort((x,x2) => (x2.Bank + x2.Balance).CompareTo(x.Bank + x.Balance));

            string output = "";
            int toCount = Math.Min(accounts.Count, 5);
            for(int i = 0; i < toCount; i++) {
                output += $"**#{i + 1}**: {(await ctx.Client.GetUserAsync((ulong)accounts[i].Id)).Username} - ${accounts[i].Balance + accounts[i].Bank}\n";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Top Balances",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region multipliers
        [SlashCommand("multipliers", "Money?")]
        public async Task ListWinningMultipliers(InteractionContext ctx, [Option("count", "how much to lose")] long count,
            [Option("scale", "how much to lose")] double scale) {
            string output = "";

            for(int i = 1; i < count + 1; i++) {
                output += $"**{i} win**: x{Bot.Modules.Economy.GetWinningsMultiplier(i, scale)}\n";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Top Balances",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion
    }
}