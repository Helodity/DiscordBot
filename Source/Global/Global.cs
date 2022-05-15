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
            FileExtension.CreateFileWithPath(path);
            return (T)Activator.CreateInstance(typeof(T));
        }
        using(var fs = File.OpenRead(path)) {
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
        using(CustomJsonTextWriter jw = new CustomJsonTextWriter(sw)) {
            jw.MaxIndentDepth = 3;
            JsonSerializer ser = new JsonSerializer();
            ser.Serialize(jw, toSave);
            sw.ToString();
            File.WriteAllText(path, sw.ToString());
        }
    }
}
