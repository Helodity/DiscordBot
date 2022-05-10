namespace DiscordBotRewrite.Global;

//Extensions that are not module exclusive
public static class StringExtensions {
    public static string ToItalics(this string value) {
        return "*" + value + "*";
    }
    public static string ToBold(this string value) {
        return "**" + value + "**";
    }
    public static string ToCode(this string value) {
        return "`" + value + "`";
    }
    public static string ToStrikethrough(this string value) {
        return "~~" + value + "~~";
    }
    public static string ToUnderline(this string value) {
        return "__" + value + "__";
    }
    public static string ToFirstUpper(this string str) {
        if(string.IsNullOrEmpty(str))
            return string.Empty;
        return char.ToUpper(str[0]) + str[1..].ToLower();
    }
}

public static class CollectionExtensions {
    public static void AddOrUpdate<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue value) {
        if(dict.TryGetValue(key, out TValue v)) {
            dict.Remove(key);
        }
        dict.Add(key, value);
    }
    public static void Randomize<T>(this List<T> list) {
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = GenerateRandomNumber(0, n);
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}

public static class DiscordAttachmentExtensions {

    static List<string> ImageFileExtensions = new List<string> {
        ".jpg",
        ".jpeg",
        ".gif",
        ".png",
        ".webp"
    };

    public static bool IsImage(this DiscordAttachment attachment) {
        string url = attachment.Url.ToLower();
        for(int i = 0; i < ImageFileExtensions.Count; i++) {
            if(url.EndsWith(ImageFileExtensions[i]))
                return true;
        }
        return false;
    }
}

public static class DiscordGuildExtensions {
    /// <summary>
    /// Guild.GetMemberAsync() with an error catch
    /// </summary>
    public static async Task<DiscordMember> TryGetMember(this DiscordGuild guild, ulong userId) {
        try {
            DiscordMember member = await guild.GetMemberAsync(userId);
            return member;
        } catch(NotFoundException) {
            return null;
        }
    }
    /// <summary>
    /// Returns a list of members.
    /// </summary>
    public static async Task<List<DiscordMember>> GetMembersAsync(this DiscordGuild guild, bool includeBots = false) {
        var members = await guild.GetAllMembersAsync();
        //Convert members to a list that can be written to.
        List<DiscordMember> filteredMembers = new List<DiscordMember>();
        foreach(DiscordMember member in members) {
            if(!member.IsBot || includeBots)
                filteredMembers.Add(member);
        }
        return filteredMembers;
    }

    public static List<DiscordRole> GetAllRoles(this DiscordGuild guild) {
        var roles = guild.Roles;
        List<DiscordRole> roleList = new List<DiscordRole>();
        foreach(DiscordRole role in roles.Values) {
            roleList.Add(role);
        }
        return roleList;
    }
}
public static class InteractionContextExtensions {
    public static async Task CreateBasicResponse(this InteractionContext ctx, string message, bool AsEphemeral = false) {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(AsEphemeral));
    }
    public static async Task EditBasicResponse(this InteractionContext ctx, string message) {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
    }
}
public static class DiscordUserExtensions {
    public static bool IsOwner(this DiscordUser user) {
        return user.Id == Bot.Config.OwnerId;
    }
}