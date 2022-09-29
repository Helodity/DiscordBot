using System;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite {
    class Program {
        static void Main() {
            SetupExceptionHandler();
            Bot.Start().GetAwaiter().GetResult();
        }


        static void SetupExceptionHandler() {
            #if !DEBUG
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) => {
                if(eventArgs.Exception is DiscordException exception) {
                    Bot.Client.Logger.LogCritical(exception.JsonMessage);
                } else {
                    Bot.Client.Logger.LogCritical($"{eventArgs.Exception.Message} : {eventArgs.Exception.StackTrace}");
                }
            };
            #endif
        }
    }
}