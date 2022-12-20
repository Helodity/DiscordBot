using System;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite {
    class Program {
        static void Main() {
            SetupExceptionHandler();
            Bot.Start().GetAwaiter().GetResult();
        }

        static void SetupExceptionHandler() {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
                LogException((Exception)eventArgs.ExceptionObject, "AppDomain");
            };
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) => {
                LogException(eventArgs.Exception, "TaskScheduler");
            };
        }

        static void LogException(Exception e, string source = "") {
            string text = $"\n\n{DateTime.Now} | {source}\n";
            text += $"MESSAGE: {e.Message}\nSTACK TRACE:{e.StackTrace}\nINNER EXCEPTION:{e.InnerException}\nTARGETSITE:{e.TargetSite}";
            Bot.Client.Logger.LogCritical(text);
            if(Bot.Config.TextLogging)
                File.AppendAllText("log.txt", text);
        }
    }
}