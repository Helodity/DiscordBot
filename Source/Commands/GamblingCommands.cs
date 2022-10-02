using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotRewrite.Modules;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("gamble", "Throw away your money")]
    class GamblingCommands : ApplicationCommandModule {
        #region Blackjack
        [SlashCommand("blackjack", "Play blackjack against the bot")]
        public async Task BlackJack(InteractionContext ctx, [Option("bet", "How much money to lose")] long bet) {
            if(!await Bot.Modules.Economy.CheckForProperBetAsync(ctx, bet)) return;

            DiscordButtonComponent[] hitStandButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "hit", "Hit!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "stand", "Stand"),
            };

            UserAccount account = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            account.ModifyBalance(-bet);
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
                    stateString += $"Over 21! You bust and lose ${bet}!";
                    embed.WithColor(Bot.Style.ErrorColor);
                    embed.WithDescription(stateString);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }

                embed.WithDescription(stateString);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(hitStandButtons));
                var interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                if(interactivityResult.TimedOut) {
                    embed.WithDescription("Sure, ok. Just leave on me.  I'm keeping your bet, fuck you.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                stateString = string.Empty;
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
                account.ModifyBalance(bet * 2);
            } else {
                embed.WithColor(Bot.Style.ErrorColor);
                stateString += $"You lose ${bet}!";
            }
            embed.WithDescription(stateString);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        #endregion

        #region Highlow
        [SlashCommand("highlow", "Play higher lower against the bot.")]
        public async Task HighLow(InteractionContext ctx, [Option("bet", "How much money to lose?")] long bet) {
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
            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();
            while(true) {
                account.ModifyBalance(-bet);
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
                            account.ModifyBalance(currentWinnings);
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
                            account.ModifyBalance(currentWinnings);
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

        #region Rock Paper Scissors
        [SlashCommand("rps", "Play rock paper scissors against another user")]
        public async Task RPS(InteractionContext ctx, [Option("opponent", "Who to challenge?")] DiscordUser opponent, [Option("bet", "How much money to lose?")] long bet) {
            if(!await Bot.Modules.Economy.CheckForProperBetAsync(ctx, bet)) return;
            if(!await Bot.Modules.Economy.CheckForProperTargetAsync(ctx, opponent)) return;

            DiscordButtonComponent[] rpsButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "rock", "Rock"),
                new DiscordButtonComponent(ButtonStyle.Primary, "paper", "Paper"),
                new DiscordButtonComponent(ButtonStyle.Primary, "scissors", "Scissors"),
                new DiscordButtonComponent(ButtonStyle.Danger, "decline", "Decline")
            };

            UserAccount account1 = Bot.Modules.Economy.GetAccount((long)ctx.User.Id);
            UserAccount account2 = Bot.Modules.Economy.GetAccount((long)opponent.Id);

            bet = Math.Min(Math.Min(account1.Balance, account2.Balance), bet);

            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();

            var embed = new DiscordEmbedBuilder {
                Title = "RPS"
            };

            string p1status = "Waiting...";
            string p2status = "Waiting...";

            embed.WithColor(Bot.Style.DefaultColor);
            embed.WithDescription($"{ctx.User.Username} challenges {opponent.Username} to a game of Rock, Paper, Scissors for ${bet}!\n" +
                $"**{ctx.User.Username}**: {p1status}\n" +
                $"**{opponent.Username}**: {p2status}");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(rpsButtons));

            var originalMessage = await ctx.GetOriginalResponseAsync();
            int p1_choice = -1;
            string p1_str = "";
            int p2_choice = -1;
            string p2_str = "";
            var task1 = new Task<Task>(async () => {
                var a = interactivity.WaitForButtonAsync(originalMessage, ctx.User);
                if(a.Result.TimedOut) {
                    embed.WithDescription($"{ctx.User.Username} didn't respond...");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                if(a.Result.Result.Id == "decline") {
                    embed.WithDescription($"{ctx.User.Username} rejected the challenge.");
                    await a.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                p1status = "Ready!";
                p1_str = a.Result.Result.Id.ToFirstUpper();
                p1_choice = a.Result.Result.Id switch {
                    "rock" => 0,
                    "paper" => 1,
                    "scissors" => 2,
                    _ => -1
                };
                embed.WithDescription($"{ctx.User.Username} challenges {opponent.Username} to a game of Rock, Paper, Scissors for ${bet}!\n" +
                $"**{ctx.User.Username}**: {p1status}\n" +
                $"**{opponent.Username}**: {p2status}");
                await a.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(rpsButtons));
            });
            var task2 = new Task<Task>(async () => {
                var b = interactivity.WaitForButtonAsync(originalMessage, opponent);
                if(b.Result.TimedOut) {
                    embed.WithDescription($"{opponent.Username} didn't respond...");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                if(b.Result.Result.Id == "decline") {
                    embed.WithDescription($"{opponent.Username} rejected the challenge.");
                    await b.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                p2status = "Ready!";
                p2_str = b.Result.Result.Id.ToFirstUpper();
                p2_choice = b.Result.Result.Id switch {
                    "rock" => 0,
                    "paper" => 1,
                    "scissors" => 2,
                    _ => -1
                };

                embed.WithDescription($"{ctx.User.Username} challenges {opponent.Username} to a game of Rock, Paper, Scissors for **${bet}**!\n" +
                $"**{ctx.User.Username}**: {p1status}\n" +

                $"**{opponent.Username}**: {p2status}");
                await b.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(rpsButtons));
            });
            task1.Start();
            task2.Start();
            await Task.WhenAll(await task1, await task2);

            //Something went wrong, abort
            if(p1_choice == -1 || p2_choice == -1)
                return;

            int result;
            if(p1_choice == p2_choice) {
                result = 0;
            } else if(p1_choice == (p2_choice + 1) % 3) {
                result = 1;
            } else {
                result = -1;
            }
            Bot.Modules.Economy.Transfer((long)opponent.Id, (long)ctx.User.Id, result * bet, true);

            DiscordUser winner = result switch {
                1 => ctx.User,
                -1 => opponent,
                _ => null
            };
            string resultString = $"{ctx.User.Mention} played {p1_str}, {opponent.Mention} played {p2_str}. ";
            resultString += winner != null ? $"{winner.Mention} win ${bet}!" : "Draw.";
            embed.WithDescription(resultString);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
        #endregion
    }
}
