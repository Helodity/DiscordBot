global using DiscordBotRewrite.Attributes;
global using DiscordBotRewrite.Commands;
global using DiscordBotRewrite.Extensions;
global using DiscordBotRewrite.Global;
global using DiscordBotRewrite.Modules;
global using DSharpPlus;
global using DSharpPlus.Entities;
global using DSharpPlus.EventArgs;
global using DSharpPlus.Exceptions;
global using DSharpPlus.Interactivity;
global using DSharpPlus.Interactivity.Extensions;
global using DSharpPlus.Lavalink;
global using DSharpPlus.Lavalink.EventArgs;
global using DSharpPlus.Net;
global using DSharpPlus.SlashCommands;
global using Microsoft.Extensions.Logging;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Converters;
global using SkiaSharp;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Threading.Tasks;
global using static DiscordBotRewrite.Global.Global;

namespace DiscordBotRewrite;
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
                    Bot.Client.Logger.LogCritical(eventArgs.Exception.Message);
                    break;
            }
        };
    }
}
