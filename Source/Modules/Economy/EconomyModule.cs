using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Modules {
    public class EconomyModule {
        #region Constructors
        public EconomyModule() {
            Bot.Database.CreateTable<UserAccount>();
        }
        #endregion

        public List<UserAccount> GetAllAccounts() {
            return Bot.Database.Table<UserAccount>().ToList();
        }
        public UserAccount GetAccount(long userId) {
            UserAccount account = Bot.Database.Table<UserAccount>().FirstOrDefault(x => x.UserId == userId);
            if(account == null) {
                account = new UserAccount(userId);
                Bot.Database.Insert(account);
            }
            return account;
        }

        public long Transfer(long id1, long id2, long value, bool allowNegative = false) {
            if(!allowNegative && value <= 0)
                return 0;
            UserAccount account1 = GetAccount(id1);
            UserAccount account2 = GetAccount(id2);

            if(account1.Balance < value)
                value = account1.Balance;

            account1.ModifyBalance(-value);
            account2.ModifyBalance(value);
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

        public async Task<bool> CheckForProperTargetAsync(InteractionContext ctx, DiscordUser user) {
            if(user.IsBot) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"I'm gonna let you in on a secret: bots really don't like being chosen. Maybe pick an actual person next time.",
                    Color = Bot.Style.ErrorColor
                });
                return false;
            }
            return true;
        }

        public async Task<bool> CheckForProperBetAsync(InteractionContext ctx, long bet) {
            UserAccount account = GetAccount((long)ctx.User.Id);
            if(bet < 0) {
                account.ModifyBalance(-1);
                Bot.Database.Update(account);
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Alright bitchass stop trying to game the system. I'm taking a dollar from you cuz of that.",
                    Color = Bot.Style.ErrorColor
                });
                return false;
            }

            if(account.Balance < bet) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"This isn't the stock market, you can only bet what's in your pocket.",
                    Color = Bot.Style.ErrorColor
                });
                return false;
            }
            return true;
        }
        #endregion
    }
}