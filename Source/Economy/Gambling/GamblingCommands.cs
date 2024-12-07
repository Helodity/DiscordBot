using DiscordBotRewrite.Economy;
using DiscordBotRewrite.Global;
using DiscordBotRewrite.Global.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Economy.Gambling
{
    [SlashCommandGroup("gamble", "Throw away your money")]
    class GamblingCommands : ApplicationCommandModule
    {
        #region Blackjack
        [SlashCommand("blackjack", "Play blackjack against the bot")]
        public async Task BlackJack(InteractionContext ctx, [Option("bet", "How much money to lose")] long bet)
        {
            if (!await CommandGuards.CheckForProperBetAsync(ctx, bet))
            {
                return;
            }

            DiscordButtonComponent[] hitStandButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "hit", "Hit!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "stand", "Stand"),
            };

            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            await ctx.DeferAsync();
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Blackjack"
            };
            InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = new();
            for (int gamesPlayed = 0; true; gamesPlayed++)
            {
                account.Pay(bet);
                Deck deck = Deck.GetStandardDeck();
                List<Card> dealerHand = deck.Draw(2);
                List<Card> playerHand = deck.Draw(2);

                string stateString;
                int playerValue;
                bool hasBust = false;
                for (int roundsPlayed = 0; true; roundsPlayed++)
                {
                    stateString = GetBlackjackStateString(ctx.User.Username, playerHand, dealerHand, false, out playerValue, out _);

                    if (playerValue > 21)
                    {
                        stateString += $"Over 21! You bust and lose **${bet}**!";
                        embed.WithDescription(stateString);
                        embed.WithColor(Bot.Style.ErrorColor);
                        hasBust = true;
                        break;
                    }
                    embed.WithColor(Bot.Style.DefaultColor);
                    embed.WithDescription(stateString);

                    //This will only trigger on a loop, so we will have an interaction to respond to
                    if (gamesPlayed > 0 || roundsPlayed > 1)
                    {
                        await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                           new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(hitStandButtons));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(hitStandButtons));
                    }

                    interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                    if (interactivityResult.TimedOut)
                    {
                        embed.WithDescription("Sure, ok. Just leave on me.  I'm keeping your bet, fuck you.");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                    //Don't respond to the interaction here, it's handled elsewhere

                    if (interactivityResult.Result.Id == "hit")
                    {
                        playerHand.Add(deck.Draw());
                    }

                    if (interactivityResult.Result.Id == "stand")
                    {
                        break;
                    }
                }
                if (hasBust)
                {
                    await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed)
                    .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"replay", $"Replay (${bet})", account.Balance < bet)));

                    interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);
                    if (interactivityResult.TimedOut)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                    //Don't respond to the interaction here, it's handled at the start of the loop since we need to setup the game first
                    continue;
                }

                while (Bot.Modules.Economy.CalculateBlackJackHandValue(dealerHand) < 18)
                {
                    dealerHand.Add(deck.Draw());
                }

                stateString = GetBlackjackStateString(ctx.User.Username, playerHand, dealerHand, true, out playerValue, out int dealerValue);

                if (playerValue > dealerValue || dealerValue > 21)
                {
                    embed.WithColor(Bot.Style.SuccessColor);
                    stateString += $"You win **${bet}**!";
                    account.Receive(bet * 2);
                }
                else
                {
                    embed.WithColor(Bot.Style.ErrorColor);
                    stateString += $"You lose **${bet}**!";
                }
                embed.WithDescription(stateString);

                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"replay", $"Replay (${bet})", account.Balance < bet)));

                interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);
                if (interactivityResult.TimedOut)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                //Don't respond to the interaction here, it's handled at the start of the loop since we need to setup the game first
            }
        }

        public string GetBlackjackStateString(string playerName, List<Card> playerHand, List<Card> dealerHand, bool showDealer, out int playerValue, out int dealerValue)
        {
            playerValue = Bot.Modules.Economy.CalculateBlackJackHandValue(playerHand);
            dealerValue = Bot.Modules.Economy.CalculateBlackJackHandValue(dealerHand);
            string statestring = $"**Dealer**\n";
            if (showDealer)
            {
                statestring += $"Cards: {Card.ListToString(dealerHand)}\n" +
                               $"Total: {dealerValue}\n";
            }
            else
            {
                statestring += $"Cards: {dealerHand[0]} ?\n" +
                               $"Total: ?\n";
            }
            statestring += $"**{playerName}**\n" +
                           $"Cards: {Card.ListToString(playerHand)}\n" +
                           $"Total: {playerValue}\n";
            return statestring;
        }


        #endregion

        #region Highlow
        [SlashCommand("highlow", "Play higher lower against the bot.")]
        public async Task HighLow(InteractionContext ctx, [Option("bet", "How much money to lose?")] long bet)
        {
            if (!await CommandGuards.CheckForProperBetAsync(ctx, bet))
            {
                return;
            }

            //Create all the used buttons
            DiscordButtonComponent[] highLowButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "higher", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"))),
                new DiscordButtonComponent(ButtonStyle.Primary, "lower", null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"))),
            };
            DiscordButtonComponent[] continueQuitButtons = {
                new DiscordButtonComponent(ButtonStyle.Success, "continue", "Keep Playing"),
                new DiscordButtonComponent(ButtonStyle.Danger, "quit", "Cash Out"),
            };
            UserAccount account = UserAccount.GetAccount((long)ctx.User.Id);
            await ctx.DeferAsync();

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Higher or Lower"
            };
            InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = new();
            for (int gamesPlayed = 0; true; gamesPlayed++)
            {
                account.Pay(bet);
                long effectiveBet = (long)Math.Pow(bet, 0.99);
                Deck deck = Deck.GetStandardDeck();
                Card anchorCard = deck.Draw();

                for (int roundsWon = 1; true; roundsWon++)
                {
                    Card nextCard = deck.Draw();
                    embed.WithColor(Bot.Style.DefaultColor);
                    embed.WithDescription($"{ctx.User.Mention}, Will the next card I draw be higher or lower than a {anchorCard}?");

                    //This will only trigger on a loop, so we will have an interaction to respond to
                    if (gamesPlayed > 0 || roundsWon > 1)
                    {
                        await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                           new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(highLowButtons));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(highLowButtons));
                    }
                    interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                    if (interactivityResult.TimedOut)
                    {
                        embed.WithDescription("Sure, ok. Just leave on me. I'm keeping your bet, fuck you.");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                    bool hasLost = false;
                    if (interactivityResult.Result.Id == "lower")
                    {
                        hasLost = anchorCard.value < nextCard.value;
                    }
                    if (interactivityResult.Result.Id == "higher")
                    {
                        hasLost = anchorCard.value > nextCard.value;
                    }

                    if (hasLost)
                    {
                        embed.WithColor(Bot.Style.ErrorColor);
                        embed.WithDescription($"Sorry, I drew {nextCard}. You lost your ${bet} bet.");
                        break;
                    }
                    long currentWinnings = (long)(effectiveBet * (Bot.Modules.Economy.GetMultiplier(roundsWon, 0.4, 1.4) - 0.3));
                    long nextWinnings = (long)(effectiveBet * (Bot.Modules.Economy.GetMultiplier(roundsWon + 1, 0.4, 1.4) - 0.3));
                    embed.WithColor(Bot.Style.SuccessColor);

                    if (deck.Size() == 0)
                    {
                        embed.WithDescription($"Congrats, {ctx.User.Mention}, I drew {nextCard}! There are no cards left!.\n " +
                           $"You win {currentWinnings}!");
                        account.Receive(currentWinnings);
                        break;
                    }
                    embed.WithDescription($"Congrats, {ctx.User.Mention}, I drew {nextCard}! There are {deck.Size()}/52 cards left!.\n " +
                            $"You can cash out with ${currentWinnings}, or risk it all for ${nextWinnings - currentWinnings} more. You will not recieve your bet back");
                    await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(continueQuitButtons));

                    interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);

                    embed.WithDescription($"You cashed out with ${currentWinnings}.");
                    if (interactivityResult.TimedOut)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        account.Receive(currentWinnings);
                        return;
                    }
                    if (interactivityResult.Result.Id == "quit")
                    {
                        account.Receive(currentWinnings);
                        break;
                    }
                    anchorCard = nextCard;
                    //Don't respond to the interaction here, it's handled at the start of the loop since we need to setup the game first
                }
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder().AddEmbed(embed)
                            .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"replay", $"Replay (${bet})", account.Balance < bet)));

                interactivityResult = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), ctx.User);
                if (interactivityResult.TimedOut)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                //Don't respond to the interaction here, it's handled at the start of the loop since we need to setup the game first
            }
        }
        #endregion

        #region Rock Paper Scissors
        [SlashCommand("rps", "Play rock paper scissors against another user")]
        public async Task RPS(InteractionContext ctx, [Option("opponent", "Who to challenge?")] DiscordUser opponent, [Option("bet", "How much money to lose?")] long bet)
        {
            if (!await CommandGuards.CheckForProperBetAsync(ctx, bet))
            {
                return;
            }

            if (!await CommandGuards.PreventBotTargetAsync(ctx, opponent))
            {
                return;
            }

            if (!await CommandGuards.PreventSelfTargetAsync(ctx, opponent))
            {
                return;
            }

            DiscordButtonComponent[] rpsButtons = {
                new DiscordButtonComponent(ButtonStyle.Primary, "rock", "Rock"),
                new DiscordButtonComponent(ButtonStyle.Primary, "paper", "Paper"),
                new DiscordButtonComponent(ButtonStyle.Primary, "scissors", "Scissors"),
                new DiscordButtonComponent(ButtonStyle.Danger, "decline", "Decline")
            };

            UserAccount account1 = UserAccount.GetAccount((long)ctx.User.Id);
            UserAccount account2 = UserAccount.GetAccount((long)opponent.Id);

            bet = Math.Min(Math.Min(account1.Balance, account2.Balance), bet);

            await ctx.DeferAsync();
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "RPS"
            };

            string p1status = "Waiting...";
            string p2status = "Waiting...";

            embed.WithColor(Bot.Style.DefaultColor);
            embed.WithDescription($"{ctx.User.Username} challenges {opponent.Username} to a game of Rock, Paper, Scissors for **${bet}**!\n" +
                $"**{ctx.User.Username}**: {p1status}\n" +
                $"**{opponent.Username}**: {p2status}");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(rpsButtons));

            DiscordMessage originalMessage = await ctx.GetOriginalResponseAsync();
            int p1_choice = -1;
            string p1_str = "";
            int p2_choice = -1;
            string p2_str = "";
            Task<Task> task1 = new Task<Task>(async () =>
            {
                Task<InteractivityResult<ComponentInteractionCreateEventArgs>> a = interactivity.WaitForButtonAsync(originalMessage, ctx.User);
                if (a.Result.TimedOut)
                {
                    embed.WithDescription($"{ctx.User.Username} didn't respond...");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                if (a.Result.Result.Id == "decline")
                {
                    embed.WithDescription($"{ctx.User.Username} rejected the challenge.");
                    await a.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                p1status = "Ready!";
                p1_str = a.Result.Result.Id.ToFirstUpper();
                p1_choice = a.Result.Result.Id switch
                {
                    "rock" => 0,
                    "paper" => 1,
                    "scissors" => 2,
                    _ => -1
                };
                embed.WithDescription($"{ctx.User.Username} challenges {opponent.Username} to a game of Rock, Paper, Scissors for **${bet}**!\n" +
                $"**{ctx.User.Username}**: {p1status}\n" +
                $"**{opponent.Username}**: {p2status}");
                await a.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(rpsButtons));
            });
            Task<Task> task2 = new Task<Task>(async () =>
            {
                Task<InteractivityResult<ComponentInteractionCreateEventArgs>> b = interactivity.WaitForButtonAsync(originalMessage, opponent);
                if (b.Result.TimedOut)
                {
                    embed.WithDescription($"{opponent.Username} didn't respond...");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                if (b.Result.Result.Id == "decline")
                {
                    embed.WithDescription($"{opponent.Username} rejected the challenge.");
                    await b.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
                p2status = "Ready!";
                p2_str = b.Result.Result.Id.ToFirstUpper();
                p2_choice = b.Result.Result.Id switch
                {
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
            if (p1_choice == -1 || p2_choice == -1)
            {
                return;
            }

            int result;
            if (p1_choice == p2_choice)
            {
                result = 0;
            }
            else if (p1_choice == (p2_choice + 1) % 3)
            {
                result = 1;
            }
            else
            {
                result = -1;
            }
            Bot.Modules.Economy.Transfer((long)opponent.Id, (long)ctx.User.Id, result * bet, true);

            DiscordUser winner = result switch
            {
                1 => ctx.User,
                -1 => opponent,
                _ => null
            };
            string resultString = $"{ctx.User.Mention} played {p1_str}, {opponent.Mention} played {p2_str}. ";
            resultString += winner != null ? $"{winner.Mention} win **${bet * 2}**" : "Draw.";
            embed.WithDescription(resultString);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
        #endregion
    }
}
