using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        [SlashCommand("balance", "Check how much money someone has.")]
        public async Task Balance(InteractionContext ctx, [Option("user", "Who are we checking?")] DiscordUser user = null) {
            if(user == null)
                user = ctx.User;
            if(!await Bot.Modules.Economy.CheckForProperTargetAsync(ctx, user)) return;

            UserAccount account = Bot.Modules.Economy.GetAccount((long)user.Id);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = $"{user.Username}'s Account",
                Description = $"Balance: ${account.Balance}\nBank: ${account.Bank}/${account.BankMax}",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Baltop
        [SlashCommand("baltop", "Who has the most money?")]
        public async Task Baltop(InteractionContext ctx) {
            var allMembers = await ctx.Guild.GetMembersAsync();
            var accounts = Bot.Modules.Economy.GetAllAccounts().Where(x => allMembers.Any(y => y.Id == (ulong)x.UserId)).ToList();
            List<UserAccount> topAccounts = new List<UserAccount>() { accounts[0] };
            long totalValue = accounts[0].NetWorth;
            foreach(UserAccount a in accounts.Skip(1)) {
                totalValue += a.NetWorth;
                for(int i = 0; i < topAccounts.Count; i++) {
                    if(a.NetWorth > topAccounts[i].NetWorth) {
                        topAccounts.Insert(i, a);
                        break;
                    } else if(i == topAccounts.Count - 1) {
                        topAccounts.Add(a);
                        break;
                    }
                }
            }
            int toCount = Math.Min(topAccounts.Count, 5);
            string description = "";
            for(int i = 0; i < toCount; i++) {
                description += $"**#{i + 1}**: {(await ctx.Client.GetUserAsync((ulong)topAccounts[i].UserId)).Username} - ${topAccounts[i].Balance + topAccounts[i].Bank}\n";
            }
            string footer = $"There is ${totalValue} in circulation!";

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Title = "Top Balances",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter() {Text = footer},
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Daily
        [SlashCommand("daily", "Get your daily stimulus check.")]
        public async Task Daily(InteractionContext ctx) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            if(DateTime.Compare(DateTime.Now, account.DailyCooldown) < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't claim your daily reward yet!\nYou can claim more money {account.DailyCooldown.ToTimestamp()}!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }
            long amount = (long)(100 * Bot.Modules.Economy.GetMultiplier(account.Streak, 0.2, 1.2));

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
        [SlashCommand("deposit", "Send monry to the bank.")]
        public async Task Deposit(InteractionContext ctx, [Option("amount", "How much to store?")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            amount = account.TransferToBank(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Deposited ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion

        #region Expand
        [SlashCommand("expand", "Store more in your bank!")]
        public async Task Expand(InteractionContext ctx, [Option("amount", "How much to expand?")] long amount) {
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
                Description = $"It will cost ${cost} to increase you bank size by ${amount}, are you sure?",
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
        [SlashCommand("give", "Give some money to another user!")]
        public async Task Give(InteractionContext ctx, [Option("user", "Who gets your money?")] DiscordUser user, [Option("amount", "How much to give?")] long amount) {
            if(!await Bot.Modules.Economy.CheckForProperTargetAsync(ctx, user)) return;
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
        [SlashCommand("rob", "Take somebody's money!")]
        public async Task Rob(InteractionContext ctx, [Option("user", "Who are you stealing from?")] DiscordUser user) {
            if(!await Bot.Modules.Economy.CheckForProperTargetAsync(ctx, user)) return;
            if(ctx.User == user) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You... can't rob yourself.",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            UserAccount self = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            UserAccount target = Bot.Modules.Economy.GetAccount((long)user.Id);
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
                long amount = Math.Max((long)(target.Balance * 0.01 * rng),1);
                amount = Bot.Modules.Economy.Transfer((long)user.Id, (long)ctx.User.Id, (int)amount);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You got ${amount} from {user.Username}!",
                    Color = Bot.Style.SuccessColor
                });
            }
        }
        #endregion

        #region Withdraw
        [SlashCommand("withdraw", "Send money to your pocket!")]
        public async Task Withdraw(InteractionContext ctx, [Option("amount", "how much to spend")] long amount) {
            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            amount = account.TransferToBalance(amount);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Withdrew ${amount}!",
                Color = Bot.Style.SuccessColor
            });
        }
        #endregion
    }
}