using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Global;
using DiscordBotRewrite.Modules.Economy;
using DiscordBotRewrite.Modules.Economy.Gambling;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SQLiteNetExtensions.Extensions;

namespace DiscordBotRewrite.Modules
{
    public class EconomyModule {
        #region Constructors
        public EconomyModule() {
            Bot.Database.CreateTable<UserAccount>();
            StockMarket.Init();
        }
        #endregion

        public List<UserAccount> GetAllAccounts() {
            return Bot.Database.Table<UserAccount>().ToList();
        }

        public long Transfer(long id1, long id2, long value, bool allowNegative = false) {
            if(!allowNegative && value <= 0)
                return 0;
            UserAccount account1 = UserAccount.GetAccount(id1);
            UserAccount account2 = UserAccount.GetAccount(id2);

            if(account1.Balance < value)
                value = account1.Balance;

            account1.Pay(value);
            account2.Receive(value);
            return value;
        }
        public double GetMultiplier(int streak, double scale = 1, double exponent = 1) {
            return scale * Math.Pow(streak, exponent) + 1;
        }

        #region Gambling
        public int CalculateBlackJackHandValue(List<Card> hand) {
            int value = 0;
            int aces = 0;
            foreach(Card c in hand) {
                int thisValue = (int)c.value + 2;
                if(thisValue > 10)
                    thisValue = 10;
                if(c.value == Card.Value.Ace) {
                    aces++;
                    thisValue = 11;
                }
                value += thisValue;
            }
            while(value > 21 && aces > 0) {
                value -= 10;
                aces -= 1;
            }
            return value;
        }
        #endregion

        #region Command Guards
        public async Task<bool> CheckForProperTargetAsync(InteractionContext ctx, DiscordUser user) {
            if(user.IsBot) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"I'm gonna let you in on a secret: bots really don't like being chosen. Maybe pick an actual person next time.",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public async Task<bool> PreventSelfTargetAsync(InteractionContext ctx, DiscordUser user) {
            if(user == ctx.User) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can't target yourself!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public async Task<bool> CheckForCooldown(InteractionContext ctx, string cooldownName) {
            Cooldown cooldown = Cooldown.GetCooldown((long)ctx.User.Id, cooldownName);
            if(!cooldown.IsOver) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can run this again {cooldown.EndTime.ToTimestamp()}",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }

        public async Task<bool> CheckForProperBetAsync(InteractionContext ctx, long bet) {
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            if(bet < 0) {
                account.Pay(1);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Alright bitchass stop trying to game the system. I'm taking a dollar from you cuz of that.",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }

            if(account.Balance < bet) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"You can only bet what's in your pocket!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return false;
            }
            return true;
        }
        #endregion
    }
}