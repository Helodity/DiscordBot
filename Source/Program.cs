using System;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite {
    class Program {
        static void Main(string[] args) {
            SetupExceptionHandler();
            Bot.Start().GetAwaiter().GetResult();
        }

        static void SetupExceptionHandler() {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) => {
                if(eventArgs.Exception is DiscordException) {
                    Bot.Client.Logger.LogCritical(((DiscordException)eventArgs.Exception).JsonMessage);
                } else {
                    Bot.Client.Logger.LogCritical($"{eventArgs.Exception.Message} : {eventArgs.Exception.StackTrace}");
                }
            };
        }
    }
}