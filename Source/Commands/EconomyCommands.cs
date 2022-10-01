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

        #region Baltop
        [SlashCommand("baltop", "Money?")]
        public async Task Baltop(InteractionContext ctx) {
            var accounts = Bot.Modules.Economy.GetAllAccounts();

            accounts.Sort((x, x2) => (x2.Bank + x2.Balance).CompareTo(x.Bank + x.Balance));

            string output = "";
            int toCount = Math.Min(accounts.Count, 5);
            for(int i = 0; i < toCount; i++) {
                output += $"**#{i + 1}**: {(await ctx.Client.GetUserAsync((ulong)accounts[i].UserId)).Username} - ${accounts[i].Balance + accounts[i].Bank}\n";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Top Balances",
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Daily
        [SlashCommand("daily", "Print a reasonable amount of money.")]
        public async Task Daily(InteractionContext ctx) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(DateTime.Compare(DateTime.Now, account.DailyCooldown) < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't claim your daily reward yet!\nYou can claim more money {account.DailyCooldown.ToTimestamp()}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            long amount = (long)(100 * Math.Pow(1.3, account.Streak));

            //48 total hours, daily cooldown accounts for 20
            if((account.DailyCooldown - DateTime.Now).TotalHours > 28) {
                account.ResetStreak(false);
            } else {
                account.IncrementStreak(false);
            }

            account.SetDailyCooldown(DateTime.Now.AddHours(20), false);
            account.ModifyBalance(amount);

            string streakText = string.Empty;
            if(account.Streak > 0) {
                streakText += account.Streak;
                streakText += account.Streak > 1 ? " days in a row!" : " day in a row!";
            } else {
                streakText = "You lost your streak!";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Claimed your daily ${amount}\n{streakText}",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Deposit
        [SlashCommand("deposit", "Bank!!!!")]
        public async Task Deposit(InteractionContext ctx, [Option("amount", "how much inflation")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            account.TransferToBank(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Deposited ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Expand
        [SlashCommand("expand", "Store more in your bank!")]
        public async Task Expand(InteractionContext ctx, [Option("amount", "how much to expand")] long amount) {
            if(amount < 1) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't change your bank size by that much!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);

            long cost = amount * 10;

            if(account.Balance < cost) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't afford this! It will cost ${cost} to increase you bank size by ${amount}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            DiscordButtonComponent[] confirmationButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "yes", "Yes!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "no", "No"),
            };

            var embed = new DiscordEmbedBuilder {
                Description = $"It will cost ${cost} to double your bank size, are you sure?",
                Color = Bot.Style.DefaultColor
            };
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(confirmationButtons));

            var interactivity = ctx.Client.GetInteractivity();
            var interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

            if(interactivityResult.Result.Id == "no") {
                embed.WithDescription("Bank expansion cancelled.");
                embed.WithColor(Bot.Style.ErrorColor);
                if(!interactivityResult.TimedOut) {
                    await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }
            if(interactivityResult.Result.Id == "yes") {
                embed.WithDescription($"Your bank has been expanded by ${amount}!");
                embed.WithColor(Bot.Style.SuccessColor);
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                account.ModifyBalance(-cost, false);
                account.ModifyBankMax(amount);
            }

        }
        #endregion

        #region Give
        [SlashCommand("give", "Not your money!!")]
        public async Task Give(InteractionContext ctx, [Option("amount", "how much inflation")] long amount, [Option("user", "who gets your money?")] DiscordUser user) {
            if(amount < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Ok now that's a dick move.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            amount = Bot.Modules.Economy.Transfer((long)ctx.User.Id, (long)user.Id, (int)amount);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Gave ${amount} to {user.Username}!",
                Color = Bot.Style.SuccessColor
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
            self.SetRobCooldown(DateTime.Now.AddSeconds(30));
            int rng = GenerateRandomNumber(0, 10);
            if(rng == 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You couldn't find an opening, try again later!",
                    Color = Bot.Style.WarningColor
                });
            } else {
                long amount = (long)(target.Balance * 0.01 * rng);
                amount = Bot.Modules.Economy.Transfer((long)user.Id, (long)ctx.User.Id, (int)amount);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You got ${amount} from {user.Username}!",
                    Color = Bot.Style.SuccessColor
                });
            }
        }
        #endregion

        #region Total
        [SlashCommand("total", "Money?")]
        public async Task Total(InteractionContext ctx) {
            var accounts = Bot.Modules.Economy.GetAllAccounts();

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

        #region Withdraw
        [SlashCommand("withdraw", "Pocket!!!!")]
        public async Task Withdraw(InteractionContext ctx, [Option("amount", "how much inflation")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            account.TransferToBalance(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Withdrew ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion
    }
}