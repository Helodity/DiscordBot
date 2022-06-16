using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Modules {
    public class VoiceModule {
        #region Properties
        public DiscordClient Client;
        public LavalinkConfiguration Config;
        public LavalinkExtension LavalinkExtension;

        Dictionary<ulong, VoiceGuildConnection> GuildConnections = new Dictionary<ulong, VoiceGuildConnection>();
        #endregion

        #region Constructors
        public VoiceModule(DiscordClient client) {
            var endpoint = new ConnectionEndpoint {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333 // From your server configuration
            };

            Config = new LavalinkConfiguration {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo() {
                    FileName = "Lavalink/lava_start.bat"
                });
            } else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start(new ProcessStartInfo() {
                    FileName = "/bin/bash",
                    Arguments = "./Lavalink/lava_start.bat"
                });
            }

            Client = client;
            Client.Ready += OnClientReady;
        }
        public async Task EnableLavalink() {
            await Task.Delay(3000);
            LavalinkExtension = Client.UseLavalink();
        }
        async Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
            await LavalinkExtension.ConnectAsync(Config);
        }
        #endregion

        #region Public
        public VoiceGuildConnection GetGuildConnection(InteractionContext ctx) {
            if(GuildConnections.ContainsKey(ctx.Guild.Id)) {
                return GuildConnections[ctx.Guild.Id];
            }
            return CreateNewConnection(ctx.Client, ctx.Guild);
        }
        public void OnVoiceGuildDisconnect(ulong guild_id) {
            GuildConnections.Remove(guild_id);
            Bot.Client.Logger.LogDebug(GuildConnections.Count.ToString());
        }
        public async Task<List<LavalinkTrack>> PerformStandardSearchAsync(string search, LavalinkNodeConnection node) {

            var tracks = new List<LavalinkTrack>();
            tracks = await TrackSearchAsync(node, search, LavalinkSearchType.Plain, true);
            if(tracks.Any())
                return tracks;

            tracks = await TrackSearchAsync(node, search, LavalinkSearchType.Youtube);
            if(tracks.Any())
                return tracks;

            return new List<LavalinkTrack>();
        }

        public async Task<List<LavalinkTrack>> TrackSearchAsync(LavalinkNodeConnection node, string search, LavalinkSearchType type, bool returnAll = false) {
            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search, type);
            if(loadResult.LoadResultType != LavalinkLoadResultType.LoadFailed
                        && loadResult.LoadResultType != LavalinkLoadResultType.NoMatches
                        && loadResult.Tracks.Any()) {
                if(returnAll) {
                    return loadResult.Tracks.ToList();
                } else {
                    return new List<LavalinkTrack> { loadResult.Tracks.First() };
                }
            }

            return new List<LavalinkTrack>();
        }
        #endregion

        #region Private
        VoiceGuildConnection CreateNewConnection(DiscordClient client, DiscordGuild guild) {
            VoiceGuildConnection conn = new VoiceGuildConnection(client, guild);
            GuildConnections.Add(guild.Id, conn);
            return GuildConnections[guild.Id];
        }
        #endregion
    }
}