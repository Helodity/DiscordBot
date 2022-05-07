namespace DiscordBotRewrite.Global;

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

public static class LavalinkGuildExtensions {
    public static int AmountOfMembersInChannel(this LavalinkGuildConnection conn) {
        List<DiscordMember> members = conn.Channel.Users.ToList();
        int totalMembers = 0;
        for(int i = 0; i < members.Count; i++) {
            if(!members[i].IsBot)
                totalMembers++;
        }
        return totalMembers;
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
            int k = BotUtils.GenerateRandomNumber(0, n);
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}

public static class SKCanvasExtensions {
    public static void SaveToPng(this SKImage image, string path) {
        using(SKData data = image.Encode(SKEncodedImageFormat.Png, 100)) {
            using(var stream = File.OpenWrite(path)) {
                data.SaveTo(stream);
            }
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