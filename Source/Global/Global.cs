namespace DiscordBotRewrite.Global;

public static class Global {
    public static readonly DiscordColor DefaultColor = DiscordColor.Azure;
    public static readonly DiscordColor WarningColor = DiscordColor.Yellow;
    public static readonly DiscordColor ErrorColor = DiscordColor.Red;
    public static readonly DiscordColor SuccessColor = DiscordColor.Green;

    /// <summary>
    /// Returns an int with min inclusive and max inclusive
    /// </summary>
    public static int GenerateRandomNumber(int min, int max) {
        DateTime date = DateTime.Now;
        int seed = (date.Year * 10000) + (date.Month * 100) + date.Day + date.Hour + date.Minute + (int)date.Ticks;
        Random rng = new Random(seed);
        return rng.Next(min, max + 1);
    }
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