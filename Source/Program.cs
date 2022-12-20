global using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite {
    class Program {
        static void Main() {
            SetupGlobalExceptionHandlers();
            Bot.Start().GetAwaiter().GetResult();
        }

        static void SetupGlobalExceptionHandlers() {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                Bot.LogException((Exception)args.ExceptionObject, "AppDomain");
            };
            TaskScheduler.UnobservedTaskException += (sender, args) => {
                Bot.LogException(args.Exception, "TaskScheduler");
            };
        }
    }
}