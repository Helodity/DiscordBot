using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Economy
{
    [SlashCommandGroup("economy", "money")]
    class EconomyCommands : ApplicationCommandModule
    {
        #region Balance
        [SlashCommand("balance", "Check how much money someone has.")]
        public async Task Balance(InteractionContext ctx, [Option("user", "Who are we checking?")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;
            if (!await CommandGuards.PreventBotTargetAsync(ctx, user)) return;

            UserAccount account = UserAccount.GetAccount((long)user.Id);
            string description = $"Balance: ${account.Balance}\nBank: ${account.Bank}/${account.BankMax}";
            if (account.Debt > 0)
                description += $"\n **Debt**: ${account.Debt}";
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Title = $"{user.Username}'s Account",
                Description = description,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Baltop
        [SlashCommand("baltop", "Who has the most money?")]
        public async Task Baltop(InteractionContext ctx)
        {
            var allMembers = await ctx.Guild.GetMembersAsync();
            var accounts = UserAccount.GetAllAccounts().Where(x => allMembers.Any(y => y.Id == (ulong)x.UserID)).ToList();
            List<UserAccount> topAccounts = new List<UserAccount>() { accounts[0] };
            long totalValue = accounts[0].NetWorth;
            foreach (UserAccount a in accounts.Skip(1))
            {
                totalValue += a.NetWorth;
                for (int i = 0; i < topAccounts.Count; i++)
                {
                    if (a.NetWorth > topAccounts[i].NetWorth)
                    {
                        topAccounts.Insert(i, a);
                        break;
                    }
                    else if (i == topAccounts.Count - 1)
                    {
                        topAccounts.Add(a);
                        break;
                    }
                }
            }
            int toCount = Math.Min(topAccounts.Count, 5);
            string description = "";
            for (int i = 0; i < toCount; i++)
            {
                description += $"**#{i + 1}**: {(await ctx.Client.GetUserAsync((ulong)topAccounts[i].UserID)).Username} - ${topAccounts[i].Balance + topAccounts[i].Bank}\n";
            }
            string footer = $"There is ${totalValue} in circulation!";

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Title = "Top Balances",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = footer },
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Beg
        [SlashCommand("beg", "Cry on the streets for some money.")]
        public async Task Beg(InteractionContext ctx)
        {
            if (!await CommandGuards.CheckForCooldown(ctx, "beg")) return;

            Cooldown begCooldown = Cooldown.GetCooldown((long)ctx.User.Id, "beg");
            begCooldown.SetEndTime(DateTime.Now + TimeSpan.FromMinutes(1));

            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            int amount = GenerateRandomNumber(5, 20);
            account.Receive(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Someone pities you and gave you ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Daily
        [SlashCommand("daily", "Get your daily stimulus check.")]
        public async Task Daily(InteractionContext ctx)
        {
            if (!await CommandGuards.CheckForCooldown(ctx, "daily")) return;

            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            Cooldown dailyCooldown = Cooldown.GetCooldown((long)ctx.User.Id, "daily");
            bool lostStreak = false;
            //48 total hours, daily cooldown accounts for 20
            if ((DateTime.Now - dailyCooldown.EndTime).TotalHours > 28)
            {
                account.ResetStreak(false);
                lostStreak = true;
            }
            else
            {
                account.IncrementStreak(false);
            }

            long amount = (long)(100 * Bot.Modules.Economy.GetMultiplier(account.Streak - 1, 0.2, 1.3));
            dailyCooldown.SetEndTime(DateTime.Now.AddHours(20));
            account.Receive(amount);

            string streakText = string.Empty;
            if (!lostStreak)
            {
                streakText += account.Streak;
                streakText += account.Streak > 1 ? " days in a row!" : " day in a row!";
            }
            else
            {
                streakText = "You lost your streak!";
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Claimed your daily ${amount}\n{streakText}",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Deposit
        [SlashCommand("deposit", "Send monry to the bank.")]
        public async Task Deposit(InteractionContext ctx, [Option("amount", "How much to store?")] long amount)
        {
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            amount = account.TransferToBank(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Deposited ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Expand
        [SlashCommand("expand", "Store more in your bank!")]
        public async Task Expand(InteractionContext ctx, [Option("amount", "How much to expand?")] long amount)
        {
            if (amount < 1)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"You can't change your bank size by that much!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);

            long cost = amount * 10;

            if (account.Balance < cost)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = $"You can't afford this! It will cost ${cost} to increase you bank size by ${amount}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            DiscordButtonComponent[] confirmationButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "yes", "Yes!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "no", "No"),
            };

            var embed = new DiscordEmbedBuilder
            {
                Description = $"It will cost ${cost} to increase you bank size by ${amount}, are you sure?",
                Color = Bot.Style.DefaultColor
            };
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(confirmationButtons));

            var interactivity = ctx.Client.GetInteractivity();
            var interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

            if (interactivityResult.Result.Id == "no")
            {
                embed.WithDescription("Bank expansion cancelled.");
                embed.WithColor(Bot.Style.ErrorColor);
                if (!interactivityResult.TimedOut)
                {
                    await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }
            if (interactivityResult.Result.Id == "yes")
            {
                embed.WithDescription($"Your bank has been expanded by ${amount}!");
                embed.WithColor(Bot.Style.SuccessColor);
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                account.Pay(cost);
                account.ModifyBankMax(amount);
            }

        }
        #endregion

        #region Give
        [SlashCommand("give", "Give some money to another user!")]
        public async Task Give(InteractionContext ctx, [Option("user", "Who gets your money?")] DiscordUser user, [Option("amount", "How much to give?")] long amount)
        {
            if (!await CommandGuards.PreventBotTargetAsync(ctx, user)) return;
            if (amount < 0)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = "Ok now that's a dick move.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            amount = Bot.Modules.Economy.Transfer((long)ctx.User.Id, (long)user.Id, (int)amount);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Gave ${amount} to {user.Username}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Repayment
        [SlashCommand("repayment", "Pay off your debt.")]
        public async Task Repayment(InteractionContext ctx, [Option("amount", "How much to pay?")] long amount)
        {
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);

            if (account.Debt <= 0)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = "You don't have a debt to pay off! Good on you!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            if (amount < 0)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder
                {
                    Description = "You know you actually need to pay something right?.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            amount = Math.Min(amount, account.Debt);
            account.Pay(amount);
            account.ModifyDebt(-amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"You paid ${amount} towards your debt!{(account.Debt <= 0 ? " You are now debt free!" : "")}",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Withdraw
        [SlashCommand("withdraw", "Send money to your pocket!")]
        public async Task Withdraw(InteractionContext ctx, [Option("amount", "how much to spend")] long amount)
        {
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            amount = account.TransferToBalance(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder
            {
                Description = $"Withdrew ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion
    }
}