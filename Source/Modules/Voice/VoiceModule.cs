using System.Diagnostics;
using System.Runtime.InteropServices;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DiscordBotRewrite.Modules {
    public class VoiceModule {

        public enum EqualizerPreset {
            [ChoiceName("Pure")]
            Pure,
            [ChoiceName("Base Boost")]
            BaseBoost,
            [ChoiceName("Super Base Boost")]
            SuperBaseBoost,
            [ChoiceName("Center Boost")]
            CenterBoost
        }

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

        public LavalinkBandAdjustment[] GetEqualizerSettings(EqualizerPreset preset) {
            LavalinkBandAdjustment[] eqSettings = Enumerable.Range(0, 15).Select(x => new LavalinkBandAdjustment(x, 0)).ToArray();
            switch (preset) {
                case EqualizerPreset.BaseBoost:
                    eqSettings[14] = new LavalinkBandAdjustment(14, -0.15f);
                    eqSettings[13] = new LavalinkBandAdjustment(13, -0.15f);
                    eqSettings[12] = new LavalinkBandAdjustment(12, -0.1f);
                    eqSettings[11] = new LavalinkBandAdjustment(11, -0.05f);

                    eqSettings[5] = new LavalinkBandAdjustment(5, 0.1f);
                    eqSettings[4] = new LavalinkBandAdjustment(4, 0.2f);
                    eqSettings[3] = new LavalinkBandAdjustment(3, 0.2f);
                    eqSettings[2] = new LavalinkBandAdjustment(2, 0.3f);
                    eqSettings[1] = new LavalinkBandAdjustment(1, 0.3f);
                    eqSettings[0] = new LavalinkBandAdjustment(0, 0.4f);
                    break;
                case EqualizerPreset.SuperBaseBoost:
                    eqSettings[14] = new LavalinkBandAdjustment(14, -0.2f);
                    eqSettings[13] = new LavalinkBandAdjustment(13, -0.2f);
                    eqSettings[12] = new LavalinkBandAdjustment(12, -0.2f);
                    eqSettings[11] = new LavalinkBandAdjustment(11, -0.2f);

                    eqSettings[8] = new LavalinkBandAdjustment(8, 0.5f);
                    eqSettings[5] = new LavalinkBandAdjustment(7, 0.75f);
                    eqSettings[6] = new LavalinkBandAdjustment(6, 1.0f);
                    eqSettings[5] = new LavalinkBandAdjustment(5, 1.0f);
                    eqSettings[4] = new LavalinkBandAdjustment(4, 1.0f);
                    eqSettings[3] = new LavalinkBandAdjustment(3, 1.0f);
                    eqSettings[2] = new LavalinkBandAdjustment(2, 1.0f);
                    eqSettings[1] = new LavalinkBandAdjustment(1, 1.0f);
                    eqSettings[0] = new LavalinkBandAdjustment(0, 1.0f);
                    break;
                case EqualizerPreset.CenterBoost:
                    eqSettings[7] = new LavalinkBandAdjustment(7, 0.2f);
                    eqSettings[6] = new LavalinkBandAdjustment(6, 0.2f);
                    eqSettings[5] = new LavalinkBandAdjustment(5, 0.2f);
                    eqSettings[4] = new LavalinkBandAdjustment(4, 0.2f);
                    eqSettings[3] = new LavalinkBandAdjustment(3, 0.2f);
                    break;
            }
            return eqSettings;
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