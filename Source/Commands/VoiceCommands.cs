using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotRewrite.Attributes;
using DiscordBotRewrite.Extensions;
using DiscordBotRewrite.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace DiscordBotRewrite.Commands {
    [SlashCommandGroup("voice", "voice")]
    class VoiceCommands : ApplicationCommandModule {

        #region Join
        [SlashCommand("join", "Join Channel")]
        [UserAbleToSummon]
        public async Task JoinChannel(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            await VGConn.Connect(ctx.Member.VoiceState.Channel);
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Joined {ctx.Member.VoiceState.Channel.Name}!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Leave
        [SlashCommand("leave", "Leave channel")]
        [UserAbleToModify]
        public async Task LeaveChannel(InteractionContext ctx) {
            VoiceGuildConnection VGconn = Bot.Modules.Voice.GetGuildConnection(ctx);
            await VGconn.Disconnect();
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Left {ctx.Member.VoiceState.Channel.Name}!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Play
        [SlashCommand("play", "Play a song")]
        [UserAbleToPlay]
        public async Task Play(InteractionContext ctx, [Option("search", "what to play")] string search) {
            VoiceModule module = Bot.Modules.Voice;
            VoiceGuildConnection VGconn = module.GetGuildConnection(ctx);

            if(!VGconn.IsConnected || VGconn.Conn.Channel == ctx.Member.VoiceState.Channel)
                await VGconn.Connect(ctx.Member.VoiceState.Channel);

            await ctx.DeferAsync();
            var tracks = await module.PerformStandardSearchAsync(search, VGconn.Node);
            if(tracks.Count == 0) {
                await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                    Description = "No results found!",
                    Color = Bot.Style.ErrorColor
                });
                return;
            }

            bool canPlayFirstSong = !VGconn.IsPlayingTrack();
            await VGconn.RequestTracksAsync(tracks);

            string output = canPlayFirstSong ? $"Now playing `{tracks[0].Title}`" : $"Enqueued `{tracks[0].Title}`";
            if(tracks.Count > 1) {
                output += $" and {tracks.Count - 1} more songs";
            }
            output += "!";

            await ctx.EditResponseAsync(new DiscordEmbedBuilder {
                Description = output,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Skip
        [SlashCommand("skip", "Skip the currently playing song")]
        [UserAbleToModify]
        public async Task Skip(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            await VGConn.SkipTrack();

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = "Skipped!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Volume
        [SlashCommand("volume", "Make the music louder")]
        [UserAbleToModify]
        public async Task Volume(InteractionContext ctx, [Option("volume", "how loud")] long volume) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            volume = Math.Clamp(volume, 1, 1000);
            await VGConn.Conn.SetVolumeAsync((int)volume);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Set the volume to {volume}%",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Loop
        [SlashCommand("loop", "Loop the queue")]
        [UserAbleToModify]
        public async Task Loop(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            VGConn.IsLooping = !VGConn.IsLooping;

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Looping {(VGConn.IsLooping ? "enabled" : "disabled")}!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Shuffle
        [SlashCommand("shuffle", "Play songs randomly")]
        [UserAbleToModify]
        public async Task Shuffle(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            VGConn.IsShuffling = !VGConn.IsShuffling;

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Shuffling {(VGConn.IsShuffling ? "enabled" : "disabled")}!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Pause
        [SlashCommand("pause", "Pause the player.")]
        [UserAbleToModify]
        public async Task Pause(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            VGConn.IsPaused = !VGConn.IsPaused;
            string description;
            if(VGConn.IsPaused) {
                description = "Paused!";
                await VGConn.Conn.PauseAsync();
            } else {
                description = "Resuming!";
                await VGConn.Conn.ResumeAsync();
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = description,
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Clear
        [SlashCommand("clear", "Clear the queue")]
        [UserAbleToModify]
        public async Task Clear(InteractionContext ctx) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            int songCount = VGConn.TrackQueue.Count;
            VGConn.TrackQueue = new List<LavalinkTrack>();

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Cleared {songCount} songs from queue!",
                Color = Bot.Style.DefaultColor
            });
        }
        #endregion

        #region Remove
        [SlashCommand("remove", "Remove the song at the specified index")]
        [UserAbleToModify]
        public async Task Remove(InteractionContext ctx, [Option("index", "index")] long index) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            int songCount = VGConn.TrackQueue.Count;

            index--; //Convert from 1 being the first song to 0
            if(index >= songCount || index < 0) {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = $"Index out of bounds!",
                    Color = Bot.Style.ErrorColor
                }, true);
                return;
            }

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                Description = $"Removed {VGConn.TrackQueue[(int)index].Title} from the queue!",
                Color = Bot.Style.DefaultColor
            });
            VGConn.TrackQueue.RemoveAt((int)index);
        }
        #endregion

        #region Queue
        [SlashCommand("queue", "Show the queue")]
        public async Task Queue(InteractionContext ctx, [Option("page", "what page")] long page = 1) {
            VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);
            int pageCount = (int)Math.Ceiling(VGConn.TrackQueue.Count / (decimal)10);
            int activePage = Math.Min(Math.Max(1, (int)page), pageCount);
            int endNumber = Math.Min(activePage * 10, VGConn.TrackQueue.Count);

            var embed = new DiscordEmbedBuilder {
                Title = "Queue",
                Color = Bot.Style.DefaultColor
            };

            string description = string.Empty;
            if(pageCount == 0) {
                description = "Queue is empty!";
            } else {
                if(VGConn.IsShuffling)
                    description += "Shuffling is **enabled**, queue position will not reflect what song is played next\n";
                for(int i = (activePage - 1) * 10; i < endNumber; i++) {
                    description += $"**{i + 1}:** `{VGConn.TrackQueue[i].Title}` by `{VGConn.TrackQueue[i].Author}` \n";
                }
            }
            embed.WithDescription(description);

            embed.WithFooter($"Page {activePage} / {pageCount}");
            await ctx.CreateResponseAsync(embed);
        }
        #endregion

        [SlashCommandGroup("savestate", "Savestate related commands")]
        public class SaveStateCommands : ApplicationCommandModule {
            #region Create Savestate
            [SlashCommand("create", "Create a text file that represents the current state of the player")]
            public async Task CreateSavestate(InteractionContext ctx) {
                VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);

                string toWrite = string.Empty;

                //Create the parameters (shuffle and looping) section of the savestate
                if(VGConn.IsLooping) toWrite += "L";
                if(VGConn.IsShuffling) toWrite += "S";

                //Add a divider between sections
                toWrite += "|";

                //Create the queue section of the savestate
                if(VGConn.IsPlayingTrack())
                    toWrite += VGConn.Conn.CurrentState.CurrentTrack.Identifier;
                foreach(LavalinkTrack track in VGConn.TrackQueue) {
                    toWrite += $";{track.Identifier}";
                }

                //Save the file to memory
                string path = $"Queues/{ctx.User.Id}.txt";
                if(!File.Exists(path))
                    FileExtension.CreateFileWithPath(path);
                File.WriteAllText(path, toWrite);

                //Send the file in the channel
                using var stream = File.OpenRead(path);
                var msgBuilder = new DiscordInteractionResponseBuilder().AddFile(path, stream);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msgBuilder);
            }
            #endregion

            #region Load Savestate
            [SlashCommand("load", "Load a previously made player state")]
            [UserAbleToPlay]
            public async Task LoadSaveState(InteractionContext ctx,
                [Option("Save", "Paste the contents of your savestate here!")] string inputSavestate,
                [Option("clear", "Do we remove preexisting songs from the queue?")] bool reset = true) {

                List<string> sections = inputSavestate.Split("|").ToList();

                VoiceGuildConnection VGConn = Bot.Modules.Voice.GetGuildConnection(ctx);

                //Load the parameters section
                VGConn.IsLooping = sections[0].Contains("L");
                VGConn.IsShuffling = sections[0].Contains("S");

                //Load the queue section
                List<string> uriStrings = sections[1].Split(";").ToList();
                uriStrings.ForEach(str => {
                    str = str.Trim();
                    str = "https://www.youtube.com/watch?v=" + str;
                });
                uriStrings.RemoveAll(x => string.IsNullOrWhiteSpace(x));

                if(reset) {
                    VGConn.TrackQueue = new List<LavalinkTrack>();
                }

                if(!VGConn.IsConnected || VGConn.Conn.Channel == ctx.Member.VoiceState.Channel)
                    await VGConn.Connect(ctx.Member.VoiceState.Channel);

                List<LavalinkTrack> tracks = new List<LavalinkTrack>();
                foreach(var str in uriStrings) {
                    List<LavalinkTrack> searchResult = await Bot.Modules.Voice.TrackSearchAsync(VGConn.Node, str, LavalinkSearchType.Plain);
                    tracks.AddRange(searchResult);
                }
                await VGConn.RequestTracksAsync(tracks);

                await ctx.CreateResponseAsync(new DiscordEmbedBuilder {
                    Description = "Sucessfully loaded the queue!",
                    Color = Bot.Style.SuccessColor
                });

            }
            #endregion

        }
    }
}