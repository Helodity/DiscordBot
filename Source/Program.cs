using DSharpPlus.Exceptions;
using System;

namespace DiscordBotRewrite {
    class Program {
        static void Main(string[] args) {
            SetupExceptionHandler();
            new Bot().Start().GetAwaiter().GetResult();
        }

        static void SetupExceptionHandler() {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) => {
                switch(eventArgs.Exception) {
                    case DiscordException:
                        Bot.Client.Logger.LogCritical(((DiscordException)eventArgs.Exception).JsonMessage);
                        break;
                    default:
                        Bot.Client.Logger.LogCritical($"{eventArgs.Exception.Message} : {eventArgs.Exception.StackTrace}");
                        break;
                }
            };
        }
    }
}