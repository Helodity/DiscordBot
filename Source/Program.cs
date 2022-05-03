global using DiscordBotRewrite.Global;
global using DiscordBotRewrite.Modules;
global using DiscordBotRewrite.Commands;
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
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;

global using SkiaSharp;

namespace DiscordBotRewrite;
class Program {
    static void Main(string[] args) {
        AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
        {
            if(eventArgs.Exception is BadRequestException)
                Bot.Client.Logger.LogError(((BadRequestException)eventArgs.Exception).JsonMessage);
            if(eventArgs.Exception is Not​Found​Exception)
                Bot.Client.Logger.LogError(((Not​Found​Exception)eventArgs.Exception).JsonMessage);
            if(eventArgs.Exception is Server​Error​Exception)
                Bot.Client.Logger.LogError(((Server​Error​Exception)eventArgs.Exception).JsonMessage);
        };
        Bot bot = new Bot();
        bot.Start().GetAwaiter().GetResult();
    }
}
