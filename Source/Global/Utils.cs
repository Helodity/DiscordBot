namespace DiscordBotRewrite.Global;

public static class BotUtils {
    /// <summary>
    /// Returns an int that is greater than or equal to min and less than or equal to max 
    /// </summary>
    public static int GenerateRandomNumber(int min, int max) {
        DateTime date = DateTime.Now;
        int seed = (date.Year * 10000) + (date.Month * 100) + date.Day + date.Hour + date.Minute + (int)date.Ticks;
        Random rng = new Random(seed);
        return rng.Next(min, max + 1);
    }

    /// <summary>
    /// Returns a Member reference to the owner. This reference may not get the current server's reference
    /// </summary>
    public static async Task<DiscordMember> GetOwnerAsync(DiscordClient client) {
        foreach(KeyValuePair<ulong, DiscordGuild> guild in client.Guilds) {
            DiscordMember member = await IdToMember(guild.Value, 414596138772070401);
            if(member != null)
                return member;
        }
        client.Logger.Log(LogLevel.Warning, $"Owner couldn't be found! Searched {client.Guilds.Count} guilds.");
        return null;
    }
    /// <summary>
    /// Returns a Member reference from an ID
    /// </summary>
    public static async Task<DiscordMember> IdToMember(DiscordGuild guild, ulong userId) {
        try {
            DiscordMember member = await guild.GetMemberAsync(userId);
            return member;
        } catch(DSharpPlus.Exceptions.NotFoundException) {
            return null;
        }
    }
    /// <summary>
    /// Returns a list of members that aren't bots.
    /// </summary>
    public static async Task<List<DiscordMember>> GetMembersAsync(DiscordGuild guild) {
        var members = await guild.GetAllMembersAsync().ConfigureAwait(false);
        //Convert members to a list that can be written to.
        List<DiscordMember> filteredMembers = new List<DiscordMember>();
        foreach(DiscordMember member in members) {
            if(!member.IsBot)
                filteredMembers.Add(member);
        }
        return filteredMembers;
    }
    public static List<DiscordRole> GetAllRoles(DiscordGuild guild) {
        var roles = guild.Roles;
        List<DiscordRole> roleList = new List<DiscordRole>();
        foreach(DiscordRole role in roles.Values) {
            roleList.Add(role);
        }
        return roleList;
    }
    public static bool IsOwner(DiscordUser user) {
        return user.Id == Bot.Config.OwnerId;
    }

    public static async Task CreateBasicResponse(InteractionContext ctx, string message, bool AsEphemeral = false) {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(AsEphemeral));
    }
    public static async Task EditBasicResponse(InteractionContext ctx, string message) {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
    }

    #region JSON
    public static T LoadJson<T>(string path) {
        if(!File.Exists(path)) {
            string[] directories = path.Split("/");
            for(int i = 1; i < directories.Length; i++) {
                string cur_path = "";
                for(int j = 0; j < i; j++) {
                    if(j > 0)
                        cur_path += "/";
                    cur_path += directories[j];
                }
                if(!Directory.Exists(cur_path)) {
                    Directory.CreateDirectory(cur_path);
                }
            }

            File.Create(path);
            return (T)Activator.CreateInstance(typeof(T));
        }
        using(var fs = File.OpenRead(path)){
            using(var sr = new StreamReader(fs, new UTF8Encoding(false))) {
                string output = sr.ReadToEnd();
                T obj = default(T);
                if(JsonConvert.DeserializeObject(output) != null) {
                    obj = JsonConvert.DeserializeObject<T>(output);
                }
                return obj != null ? obj : (T)Activator.CreateInstance(typeof(T));
            }
        }
    }
    public static void SaveJson(object toSave, string path) {
        using(StringWriter sw = new StringWriter())
        using(CustomIndentingJsonTextWriter jw = new CustomIndentingJsonTextWriter(sw)) {
            jw.MaxIndentDepth = 3;
            JsonSerializer ser = new JsonSerializer();
            ser.Serialize(jw, toSave);
            sw.ToString();
            File.WriteAllText(path, sw.ToString());
        }
    }
    #endregion
}
public class CustomIndentingJsonTextWriter : JsonTextWriter {
    public int? MaxIndentDepth { get; set; }

    public CustomIndentingJsonTextWriter(TextWriter writer) : base(writer) {
        Formatting = Formatting.Indented;
    }

    public override void WriteStartArray() {
        base.WriteStartArray();
        if(MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
            Formatting = Formatting.None;
    }

    public override void WriteStartObject() {
        base.WriteStartObject();
        if(MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
            Formatting = Formatting.None;
    }

    public override void WriteEndArray() {
        base.WriteEndArray();
        if(MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
            Formatting = Formatting.Indented;
    }

    public override void WriteEndObject() {
        base.WriteEndObject();
        if(MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
            Formatting = Formatting.Indented;
    }
}
public readonly struct Cooldown {
    readonly DateTime EndTime;
    public Cooldown(DateTime endTime) {
        EndTime = endTime;
    }
    public bool IsOver => DateTime.Compare(DateTime.Now, EndTime) >= 0;

    public static bool UserHasCooldown(ulong userId, ref Dictionary<ulong, Cooldown> cooldowns) {
        if(cooldowns.TryGetValue(userId, out Cooldown cooldown)) {
            if(!cooldown.IsOver)
                return true;
            cooldowns.Remove(userId);
        }
        return false;
    }
}