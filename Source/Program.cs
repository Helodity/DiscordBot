using System;
using System.IO;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite {
    class Program {
        static void Main() {
            SetupExceptionHandler();
            Bot.Start().GetAwaiter().GetResult();
        }

        static void SetupExceptionHandler() {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) => {
                string text = $"\n\n {DateTime.Now}\n";
                if(eventArgs.Exception is DiscordException exception) {
                    Bot.Client.Logger.LogCritical(exception.JsonMessage);
                    text += exception.JsonMessage;
                } else {
                    Bot.Client.Logger.LogCritical($"{eventArgs.Exception.Message} : {eventArgs.Exception.StackTrace} : {eventArgs.Exception.InnerException} : {eventArgs.Exception.TargetSite}");
                    text += $"{eventArgs.Exception.Message} : {eventArgs.Exception.StackTrace} : {eventArgs.Exception.InnerException} : {eventArgs.Exception.TargetSite}";
                }
                if(Bot.Config.TextLogging)
                    File.AppendAllText("log.txt", text);
            };
        }
    }
}