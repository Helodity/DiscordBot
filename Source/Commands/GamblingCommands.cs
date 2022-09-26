using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotRewrite.Modules;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("gamble", "throw away your money")]
    class GamblingCommands : ApplicationCommandModule {
        #region Blackjack
        [SlashCommand("blackjack", "Play blackjack against the bot")]
        public async Task BlackJack(InteractionContext ctx, [Option("bet", "how much to lose")] long bet) {
            if(!await Bot.Modules.Economy.CheckForProperBetAsync(ctx, bet)) return;

            DiscordButtonComponent[] hitStandButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "hit", "Hit!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "stand", "Stand"),
            };

            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            account.Balance -= bet;
            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();

            Deck deck = Deck.GetStandardDeck();
            List<Card> dealerHand = deck.Draw(2);// yeah yeah i know this isn't "accurate" but it's more efficient
            List<Card> playerHand = deck.Draw(2);

            var embed = new DiscordEmbedBuilder {
                Title = "Blackjack",
                Color = Bot.Style.DefaultColor
            };
            string stateString = string.Empty;
            int playerValue;

            while(true) {
                playerValue = Bot.Modules.Economy.CalculateBlackJackHandValue(playerHand);
                stateString += $"**Dealer**\n" +
                    $"Cards: {dealerHand[0]} ?\n" +
                    $"Total: ?\n";
                stateString += $"**{ctx.User.Username}**\n" +
                    $"Cards: {Card.ListToString(playerHand)}\n" +
                    $"Total: {playerValue}\n";

                if(playerValue > 21) {
                    stateString += $"Over 21! You bust and lose ${bet}";
                    embed.WithColor(Bot.Style.ErrorColor);
                    embed.WithDescription(stateString);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    Bot.Database.Update(account);
                    return;
                }

                embed.WithDescription(stateString);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(hitStandButtons));
                var interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                if(interactivityResult.TimedOut) {
                    embed.WithDescription("Sure, ok. Just leave on me.  I'm keeping your bet, fuck you.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    Bot.Database.Update(account);
                    return;
                }
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                if(interactivityResult.Result.Id == "hit")
                    playerHand.Add(deck.Draw());
                if(interactivityResult.Result.Id == "stand")
                    break;
            }

            stateString = string.Empty;
            while(Bot.Modules.Economy.CalculateBlackJackHandValue(dealerHand) < 18) {
                dealerHand.Add(deck.Draw());
            }

            playerValue = Bot.Modules.Economy.CalculateBlackJackHandValue(playerHand);
            int dealerValue = Bot.Modules.Economy.CalculateBlackJackHandValue(dealerHand);
            stateString += $"**Dealer**\n" +
                    $"Cards: {Card.ListToString(dealerHand)}\n" +
                    $"Total: {dealerValue}\n" +
                    $"**{ctx.User.Username}**\n" +
                    $"Cards: {Card.ListToString(playerHand)}\n" +
                    $"Total: {playerValue}\n";

            if(playerValue > dealerValue || dealerValue > 21) {
                embed.WithColor(Bot.Style.SuccessColor);
                stateString += $"You win ${bet}!";
                account.Balance += bet * 2;
            } else {
                embed.WithColor(Bot.Style.ErrorColor);
                stateString += $"You lose ${bet}!";
            }
            embed.WithDescription(stateString);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            Bot.Database.Update(account);
        }

        #endregion

        #region Highlow
        [SlashCommand("highlow", "Money?")]
        public async Task HighLow(InteractionContext ctx, [Option("bet", "how much to lose")] long bet) {
            if(!await Bot.Modules.Economy.CheckForProperBetAsync(ctx, bet)) return;

            DiscordButtonComponent[] highLowButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "higher", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"))),
                new DiscordButtonComponent(ButtonStyle.Primary, "lower", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"))),
            };

            DiscordButtonComponent[] continueQuitButtons = {
                new DiscordButtonComponent(ButtonStyle.Success, "continue", "Keep Playing"),
                new DiscordButtonComponent(ButtonStyle.Danger, "quit", "Cash Out"),
            };

            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            account.Balance -= bet;
            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();
            while(true) {
                var embed = new DiscordEmbedBuilder {
                    Title = "Higher or Lower"
                };
                bool hasLost = false;
                int gamesWon = 0;
                Deck deck = Deck.GetStandardDeck();
                Card anchorCard = deck.Draw();
                while(true) {
                    Card nextCard = deck.Draw();
                    embed.WithColor(Bot.Style.DefaultColor);
                    embed.WithDescription($"{ctx.User.Mention}, Will the next card I draw be higher or lower than a {anchorCard}?");

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(highLowButtons));
                    var interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                    if(interactivityResult.TimedOut) {
                        embed.WithDescription("Sure, ok. Just leave on me.  I'm keeping your bet, fuck you.");
                        Bot.Database.Update(account);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                    await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                    if(interactivityResult.Result.Id == "lower") {
                        hasLost = anchorCard.value < nextCard.value;
                    }
                    if(interactivityResult.Result.Id == "higher") {
                        hasLost = anchorCard.value > nextCard.value;
                    }

                    if(hasLost) {
                        embed.WithColor(Bot.Style.ErrorColor);
                        embed.WithDescription($"Sorry, I drew {nextCard}. You lost your ${bet} bet.");
                        break;
                    } else {
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
                            break;
                        }
                        embed.WithDescription($"Congrats, {ctx.User.Mention}, I drew {nextCard}! There are {deck.Size()}/52 cards left!.\n " +
                            $"You can cash out with your ${bet} bet and ${currentWinnings - bet} in winnings, or risk it all for ${nextWinnings - currentWinnings} more.");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(continueQuitButtons));

                        interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                        if(!interactivityResult.TimedOut)
                            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                        if(interactivityResult.TimedOut || interactivityResult.Result.Id == "quit") {
                            embed.WithDescription($"You cashed out with ${currentWinnings}.");
                            account.Balance += currentWinnings;
                            Bot.Database.Update(account);
                            break;
                        }
                        anchorCard = nextCard;
                    }
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"replay", $"Replay (${bet})", account.Balance < bet)));
                var replayResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);
                if(replayResult.TimedOut) {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"replay", $"Replay ${bet}", true)));
                    return;
                }
                await replayResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            }
        }
        #endregion
    }
}
