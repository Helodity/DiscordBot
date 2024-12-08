global using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite
{
    internal class Program
    {
        private static void Main()
        {
            SetupGlobalExceptionHandlers();
            Bot.Start().GetAwaiter().GetResult();
        }

        private static void SetupGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Bot.LogException((Exception)args.ExceptionObject, "AppDomain");
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Bot.LogException(args.Exception, "TaskScheduler");
            };
        }
    }
}